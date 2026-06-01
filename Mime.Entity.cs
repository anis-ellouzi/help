using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mime
{
    /// <summary>
    /// Represents a MIME entity (RFC 2045, 2046)
    /// </summary>
    public class MimeEntity
    {
        private MimeEntity _parentEntity;

        /// <summary>Gets the header fields collection</summary>
        public HeaderFieldCollection Headers { get; } = new();

        /// <summary>Gets the parent entity (if this is a child of a multipart entity)</summary>
        public MimeEntity ParentEntity
        {
            get => _parentEntity;
            internal set => _parentEntity = value;
        }

        /// <summary>Gets the child entities collection (for multipart entities)</summary>
        public List<MimeEntity> ChildEntities { get; } = new();

        /// <summary>Gets or sets the entity body data</summary>
        public byte[] Body { get; set; }

        /// <summary>Gets or sets the entity body as a string</summary>
        public string BodyAsText
        {
            get => Body != null ? Encoding.UTF8.GetString(Body) : null;
            set => Body = value != null ? Encoding.UTF8.GetBytes(value) : null;
        }

        /// <summary>Gets the Content-Type</summary>
        public MediaType ContentType => ParseContentType();

        /// <summary>Gets the Content-Type boundary (for multipart)</summary>
        public string ContentTypeBoundary => GetHeaderParameter("Content-Type", "boundary");

        /// <summary>Gets the Content-Transfer-Encoding</summary>
        public ContentTransferEncoding ContentTransferEncoding => ParseContentTransferEncoding();

        /// <summary>Gets the Content-Disposition</summary>
        public ContentDisposition ContentDisposition => ParseContentDisposition();

        /// <summary>Gets the Content-Disposition filename</summary>
        public string ContentDispositionFilename => GetHeaderParameter("Content-Disposition", "filename");

        /// <summary>Gets the message subject</summary>
        public string Subject => Headers.GetFieldValue("Subject");

        /// <summary>Gets the message from address</summary>
        public string From => Headers.GetFieldValue("From");

        /// <summary>Gets the message to addresses</summary>
        public string To => Headers.GetFieldValue("To");

        /// <summary>Gets the message cc addresses</summary>
        public string Cc => Headers.GetFieldValue("Cc");

        /// <summary>Gets the message bcc addresses</summary>
        public string Bcc => Headers.GetFieldValue("Bcc");

        /// <summary>Gets the message date</summary>
        public string Date => Headers.GetFieldValue("Date");

        /// <summary>Gets the message ID</summary>
        public string MessageId => Headers.GetFieldValue("Message-ID");

        /// <summary>Initializes a new MIME entity</summary>
        public MimeEntity()
        {
        }

        /// <summary>Gets a header parameter value</summary>
        public string GetHeaderParameter(string headerName, string parameterName)
        {
            var field = Headers.GetField(headerName);
            if (field is ParameterizedHeaderField pf)
                return pf.GetParameter(parameterName);
            return null;
        }

        /// <summary>Sets a header field value</summary>
        public void SetHeader(string name, string value)
        {
            Headers.SetField(name, value);
        }

        /// <summary>Gets if this is a multipart entity</summary>
        public bool IsMultipart => (ContentType & MediaType.Multipart) != 0;

        /// <summary>Adds a child entity (for multipart)</summary>
        public void AddChild(MimeEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            entity.ParentEntity = this;
            ChildEntities.Add(entity);
        }

        /// <summary>Gets all descendant entities (recursive)</summary>
        public List<MimeEntity> GetAllEntities()
        {
            var result = new List<MimeEntity> { this };
            foreach (var child in ChildEntities)
                result.AddRange(child.GetAllEntities());
            return result;
        }

        /// <summary>Converts entity to string representation</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MIME Entity ===");
            sb.AppendLine($"Subject: {Subject}");
            sb.AppendLine($"From: {From}");
            sb.AppendLine($"To: {To}");
            sb.AppendLine($"Content-Type: {ContentType}");
            sb.AppendLine($"Charset: {GetHeaderParameter("Content-Type", "charset")}");
            sb.AppendLine($"Body Length: {Body?.Length ?? 0} bytes");
            sb.AppendLine($"Child Entities: {ChildEntities.Count}");
            return sb.ToString();
        }

        private MediaType ParseContentType()
        {
            var contentTypeValue = Headers.GetFieldValue("Content-Type") ?? "text/plain";
            var mediaType = contentTypeValue.Split(';')[0].Trim().ToLowerInvariant();

            return mediaType switch
            {
                var x when x.StartsWith("text/") => MediaType.Text,
                var x when x.StartsWith("multipart/") => MediaType.Multipart,
                var x when x.StartsWith("message/") => MediaType.Message,
                var x when x.StartsWith("application/") => MediaType.Application,
                var x when x.StartsWith("image/") => MediaType.Image,
                var x when x.StartsWith("audio/") => MediaType.Audio,
                var x when x.StartsWith("video/") => MediaType.Video,
                _ => MediaType.Other
            };
        }

        private ContentTransferEncoding ParseContentTransferEncoding()
        {
            var encoding = Headers.GetFieldValue("Content-Transfer-Encoding") ?? "7bit";
            return encoding.ToLowerInvariant().Trim() switch
            {
                "7bit" => ContentTransferEncoding.SevenBit,
                "8bit" => ContentTransferEncoding.EightBit,
                "binary" => ContentTransferEncoding.Binary,
                "quoted-printable" => ContentTransferEncoding.QuotedPrintable,
                "base64" => ContentTransferEncoding.Base64,
                _ => ContentTransferEncoding.Unknown
            };
        }

        private ContentDisposition ParseContentDisposition()
        {
            var disposition = Headers.GetFieldValue("Content-Disposition") ?? "inline";
            return disposition.Split(';')[0].Trim().ToLowerInvariant() switch
            {
                "inline" => ContentDisposition.Inline,
                "attachment" => ContentDisposition.Attachment,
                _ => ContentDisposition.Unknown
            };
        }
    }

    /// <summary>
    /// Collection of MIME entities
    /// </summary>
    public class MimeEntityCollection : List<MimeEntity>
    {
        /// <summary>Gets all text/plain parts</summary>
        public List<MimeEntity> GetTextParts()
        {
            var result = new List<MimeEntity>();
            foreach (var entity in this)
            {
                var all = entity.GetAllEntities();
                result.AddRange(all.FindAll(e => (e.ContentType & MediaType.Text) != 0 &&
                                                   e.Headers.GetFieldValue("Content-Type")?.Contains("plain") == true));
            }
            return result;
        }

        /// <summary>Gets all text/html parts</summary>
        public List<MimeEntity> GetHtmlParts()
        {
            var result = new List<MimeEntity>();
            foreach (var entity in this)
            {
                var all = entity.GetAllEntities();
                result.AddRange(all.FindAll(e => (e.ContentType & MediaType.Text) != 0 &&
                                                   e.Headers.GetFieldValue("Content-Type")?.Contains("html") == true));
            }
            return result;
        }

        /// <summary>Gets all attachment parts</summary>
        public List<MimeEntity> GetAttachments()
        {
            var result = new List<MimeEntity>();
            foreach (var entity in this)
            {
                var all = entity.GetAllEntities();
                result.AddRange(all.FindAll(e => e.ContentDisposition == ContentDisposition.Attachment));
            }
            return result;
        }

        /// <summary>Gets an attachment by filename</summary>
        public MimeEntity GetAttachmentByName(string filename)
        {
            var attachments = GetAttachments();
            return attachments.Find(a => a.ContentDispositionFilename?.Equals(filename, StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
