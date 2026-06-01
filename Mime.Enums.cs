using System;

namespace Mime
{
    /// <summary>
    /// MIME media types (RFC 2045)
    /// </summary>
    [Flags]
    public enum MediaType
    {
        /// <summary>Text (plain, html, xml)</summary>
        Text = 1,
        /// <summary>Multipart (mixed, alternative, related)</summary>
        Multipart = 2,
        /// <summary>Message (rfc822, partial, external-body)</summary>
        Message = 4,
        /// <summary>Application (octet-stream, pdf, json, etc)</summary>
        Application = 8,
        /// <summary>Image (jpeg, png, gif, etc)</summary>
        Image = 16,
        /// <summary>Audio (mp3, wav, etc)</summary>
        Audio = 32,
        /// <summary>Video (mp4, mpeg, etc)</summary>
        Video = 64,
        /// <summary>Unknown/Other</summary>
        Other = 128
    }

    /// <summary>
    /// Content transfer encoding types (RFC 2045)
    /// </summary>
    public enum ContentTransferEncoding
    {
        /// <summary>7-bit ASCII</summary>
        SevenBit = 0,
        /// <summary>8-bit data</summary>
        EightBit = 1,
        /// <summary>Binary data</summary>
        Binary = 2,
        /// <summary>Quoted-printable encoding</summary>
        QuotedPrintable = 3,
        /// <summary>Base64 encoding</summary>
        Base64 = 4,
        /// <summary>Unknown encoding</summary>
        Unknown = 5
    }

    /// <summary>
    /// Content disposition types (RFC 2183)
    /// </summary>
    public enum ContentDisposition
    {
        /// <summary>Inline content</summary>
        Inline = 0,
        /// <summary>Attachment</summary>
        Attachment = 1,
        /// <summary>Unknown disposition</summary>
        Unknown = 2
    }
}
