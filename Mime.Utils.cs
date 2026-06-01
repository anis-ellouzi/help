using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Mime
{
    /// <summary>
    /// MIME encoding/decoding utilities
    /// </summary>
    public static class MimeUtils
    {
        /// <summary>Encodes text to Base64</summary>
        public static string EncodeBase64(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>Decodes Base64 text</summary>
        public static string DecodeBase64(string base64Text)
        {
            if (string.IsNullOrEmpty(base64Text))
                return base64Text;
            try
            {
                byte[] bytes = Convert.FromBase64String(base64Text);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return base64Text;
            }
        }

        /// <summary>Encodes text to Quoted-Printable</summary>
        public static string EncodeQuotedPrintable(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new StringBuilder();
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            foreach (byte b in bytes)
            {
                if (b == 32 || (b >= 33 && b <= 126 && b != 61)) // Printable ASCII except '='
                {
                    result.Append((char)b);
                }
                else
                {
                    result.Append($"={b:X2}");
                }
            }

            return result.ToString();
        }

        /// <summary>Decodes Quoted-Printable text</summary>
        public static string DecodeQuotedPrintable(string quotedText)
        {
            if (string.IsNullOrEmpty(quotedText))
                return quotedText;

            var result = new StringBuilder();
            var i = 0;

            while (i < quotedText.Length)
            {
                if (quotedText[i] == '=' && i + 2 < quotedText.Length)
                {
                    if (quotedText[i + 1] == '\r' && quotedText[i + 2] == '\n')
                    {
                        i += 3; // Skip soft line break
                        continue;
                    }

                    if (byte.TryParse(quotedText.Substring(i + 1, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
                    {
                        result.Append((char)b);
                        i += 3;
                        continue;
                    }
                }

                result.Append(quotedText[i]);
                i++;
            }

            return result.ToString();
        }

        /// <summary>Decodes encoded-word (RFC 2047) text</summary>
        public static string DecodeEncodedWord(string encodedWord)
        {
            if (string.IsNullOrEmpty(encodedWord) || !encodedWord.StartsWith("=?"))
                return encodedWord;

            // Format: =?charset?encoding?encoded-text?=
            var match = Regex.Match(encodedWord, @"=\?(?<charset>[^?]+)\?(?<encoding>[^?])\?(?<text>[^?]*)\?=");
            if (!match.Success)
                return encodedWord;

            var charset = match.Groups["charset"].Value;
            var encoding = match.Groups["encoding"].Value.ToUpperInvariant();
            var text = match.Groups["text"].Value;

            try
            {
                var decodedText = encoding switch
                {
                    "B" => DecodeBase64(text),
                    "Q" => DecodeQuotedPrintable(text.Replace("_", " ")),
                    _ => text
                };

                return decodedText;
            }
            catch
            {
                return encodedWord;
            }
        }

        /// <summary>Encodes text to encoded-word (RFC 2047)</summary>
        public static string EncodeEncodedWord(string text, string charset = "UTF-8")
        {
            if (string.IsNullOrEmpty(text) || IsASCII(text))
                return text;

            var encoded = EncodeBase64(text);
            return $"=?{charset}?B?{encoded}?=";
        }

        /// <summary>Checks if text is pure ASCII</summary>
        public static bool IsASCII(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            foreach (char c in text)
            {
                if (c > 127)
                    return false;
            }

            return true;
        }

        /// <summary>Parses RFC 2822 date/time string</summary>
        public static DateTime? ParseDateTime(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Try standard formats
            if (DateTime.TryParse(dateString, out var result))
                return result;

            // Try RFC 2822 format: "Thu, 19 Oct 2023 15:30:45 +0100"
            try
            {
                return DateTime.Parse(dateString, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Formats DateTime to RFC 2822 format</summary>
        public static string FormatDateTime(DateTime dateTime, TimeSpan? offset = null)
        {
            var offsetStr = offset.HasValue
                ? $"{(offset.Value.TotalSeconds >= 0 ? "+" : "-")}{Math.Abs(offset.Value.Hours):00}{Math.Abs(offset.Value.Minutes):00}"
                : DateTime.Now.ToString("zzz");

            return dateTime.ToString("ddd, dd MMM yyyy HH:mm:ss") + " " + offsetStr;
        }

        /// <summary>Validates email address format</summary>
        public static bool IsValidEmailAddress(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Simple validation: must have @ and dot
                var atIndex = email.LastIndexOf('@');
                if (atIndex <= 0 || atIndex == email.Length - 1)
                    return false;

                var dotIndex = email.LastIndexOf('.');
                return dotIndex > atIndex;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Parses MIME boundary from Content-Type value</summary>
        public static string ExtractBoundary(string contentTypeValue)
        {
            if (string.IsNullOrEmpty(contentTypeValue))
                return null;

            var match = Regex.Match(contentTypeValue, @"boundary\s*=\s*[""']?(?<boundary>[^""'\s;]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["boundary"].Value : null;
        }

        /// <summary>Parses MIME charset from Content-Type value</summary>
        public static string ExtractCharset(string contentTypeValue)
        {
            if (string.IsNullOrEmpty(contentTypeValue))
                return "UTF-8";

            var match = Regex.Match(contentTypeValue, @"charset\s*=\s*[""']?(?<charset>[^\""'\s;]+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups["charset"].Value : "UTF-8";
        }

        /// <summary>Sanitizes filename (removes unsafe characters)</summary>
        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return "attachment";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(filename.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return sanitized.Trim('.');
        }

        /// <summary>Parses email address into local and domain parts</summary>
        public static bool TryParseEmailAddress(string email, out string localPart, out string domain)
        {
            localPart = null;
            domain = null;

            if (string.IsNullOrWhiteSpace(email))
                return false;

            var atIndex = email.LastIndexOf('@');
            if (atIndex <= 0 || atIndex == email.Length - 1)
                return false;

            localPart = email.Substring(0, atIndex).Trim();
            domain = email.Substring(atIndex + 1).Trim();

            return !string.IsNullOrWhiteSpace(localPart) && !string.IsNullOrWhiteSpace(domain);
        }
    }

    /// <summary>
    /// MIME message builder for creating messages programmatically
    /// </summary>
    public class MimeMessageBuilder
    {
        private readonly MimeEntity _root;

        /// <summary>Initializes a new message builder</summary>
        public MimeMessageBuilder()
        {
            _root = new MimeEntity();
            _root.SetHeader("MIME-Version", "1.0");
        }

        /// <summary>Sets the message subject</summary>
        public MimeMessageBuilder WithSubject(string subject)
        {
            _root.SetHeader("Subject", MimeUtils.EncodeEncodedWord(subject));
            return this;
        }

        /// <summary>Sets the from address</summary>
        public MimeMessageBuilder WithFrom(string email, string displayName = null)
        {
            var addr = new MailboxAddress(displayName, email);
            _root.SetHeader("From", addr.GetDisplayString());
            return this;
        }

        /// <summary>Sets the to addresses</summary>
        public MimeMessageBuilder WithTo(params string[] emails)
        {
            var addresses = new MailboxAddressCollection();
            foreach (var email in emails)
                addresses.Add(new MailboxAddress(email));
            _root.SetHeader("To", addresses.ToEmailString());
            return this;
        }

        /// <summary>Sets the CC addresses</summary>
        public MimeMessageBuilder WithCc(params string[] emails)
        {
            var addresses = new MailboxAddressCollection();
            foreach (var email in emails)
                addresses.Add(new MailboxAddress(email));
            _root.SetHeader("Cc", addresses.ToEmailString());
            return this;
        }

        /// <summary>Sets the body</summary>
        public MimeMessageBuilder WithBody(string text, bool isHtml = false)
        {
            var contentType = isHtml ? "text/html" : "text/plain";
            _root.SetHeader("Content-Type", $"{contentType}; charset=UTF-8");
            _root.SetHeader("Content-Transfer-Encoding", "base64");
            _root.BodyAsText = text;
            return this;
        }

        /// <summary>Builds the MIME message</summary>
        public MimeEntity Build() => _root;
    }
}
