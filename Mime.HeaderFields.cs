using System;
using System.Collections.Generic;

namespace Mime
{
    /// <summary>
    /// Represents a MIME header field parameter (RFC 2045)
    /// </summary>
    public class HeaderParameter
    {
        /// <summary>Gets or sets the parameter name</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the parameter value</summary>
        public string Value { get; set; }

        /// <summary>Initializes a new header parameter</summary>
        public HeaderParameter(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? string.Empty;
        }

        /// <summary>Returns string representation</summary>
        public override string ToString() => $"{Name}={Value}";
    }

    /// <summary>
    /// Represents a MIME header field (RFC 2822)
    /// </summary>
    public class HeaderField
    {
        /// <summary>Gets or sets the field name</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the field value</summary>
        public string Value { get; set; }

        /// <summary>Gets the parameters collection</summary>
        public List<HeaderParameter> Parameters { get; } = new();

        /// <summary>Initializes a new header field</summary>
        public HeaderField(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? string.Empty;
        }

        /// <summary>Gets a parameter value by name</summary>
        public string GetParameter(string paramName)
        {
            var param = Parameters.Find(p => p.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase));
            return param?.Value;
        }

        /// <summary>Returns string representation</summary>
        public override string ToString() => $"{Name}: {Value}";
    }

    /// <summary>
    /// Represents a header field with parameters (RFC 2045, 2046)
    /// </summary>
    public class ParameterizedHeaderField : HeaderField
    {
        /// <summary>Initializes a new parameterized header field</summary>
        public ParameterizedHeaderField(string name, string value) : base(name, value)
        {
        }

        /// <summary>Adds or updates a parameter</summary>
        public void SetParameter(string name, string value)
        {
            var existing = Parameters.Find(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                existing.Value = value;
            else
                Parameters.Add(new HeaderParameter(name, value));
        }
    }

    /// <summary>
    /// Collection of header fields
    /// </summary>
    public class HeaderFieldCollection : List<HeaderField>
    {
        /// <summary>Gets the first field with the specified name, case-insensitive</summary>
        public HeaderField GetField(string fieldName)
        {
            return Find(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Gets all fields with the specified name, case-insensitive</summary>
        public List<HeaderField> GetFields(string fieldName)
        {
            return FindAll(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Gets the value of the first field with the specified name</summary>
        public string GetFieldValue(string fieldName)
        {
            return GetField(fieldName)?.Value;
        }

        /// <summary>Removes all fields with the specified name</summary>
        public void RemoveField(string fieldName)
        {
            RemoveAll(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Adds or updates a field</summary>
        public void SetField(string name, string value)
        {
            var existing = GetField(name);
            if (existing != null)
                existing.Value = value;
            else
                Add(new HeaderField(name, value));
        }
    }
}
