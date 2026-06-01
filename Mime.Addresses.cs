using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mime
{
    /// <summary>
    /// Base class for MIME addresses (RFC 2822, Section 3.4)
    /// </summary>
    public abstract class Address
    {
        /// <summary>Gets if this is a group address</summary>
        public abstract bool IsGroupAddress { get; }

        /// <summary>Gets the display string representation</summary>
        public abstract string GetDisplayString();
    }

    /// <summary>
    /// Represents a mailbox address (email address)
    /// </summary>
    public class MailboxAddress : Address
    {
        /// <summary>Gets or sets the display name</summary>
        public string DisplayName { get; set; }

        /// <summary>Gets or sets the local part (user@domain left side)</summary>
        public string LocalPart { get; set; }

        /// <summary>Gets or sets the domain part</summary>
        public string Domain { get; set; }

        /// <summary>Gets the complete email address</summary>
        public string EmailAddress => $"{LocalPart}@{Domain}".TrimStart('@').TrimEnd('@');

        /// <summary>Gets if this is a group address</summary>
        public override bool IsGroupAddress => false;

        /// <summary>Initializes a new mailbox address</summary>
        public MailboxAddress(string email) : this(null, email) { }

        /// <summary>Initializes a new mailbox address with display name</summary>
        public MailboxAddress(string displayName, string email)
        {
            DisplayName = displayName;
            ParseEmail(email);
        }

        private void ParseEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            email = email.Trim('<', '>', ' ', '\t');
            var atIndex = email.LastIndexOf('@');

            if (atIndex <= 0)
                throw new ArgumentException("Invalid email format", nameof(email));

            LocalPart = email.Substring(0, atIndex).Trim();
            Domain = email.Substring(atIndex + 1).Trim();
        }

        /// <summary>Gets the display string (either display name or email address)</summary>
        public override string GetDisplayString()
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
                return $"{DisplayName} <{EmailAddress}>";
            return EmailAddress;
        }

        /// <summary>Returns string representation</summary>
        public override string ToString() => GetDisplayString();

        /// <summary>Checks equality based on email address (case-insensitive)</summary>
        public override bool Equals(object obj)
        {
            return obj is MailboxAddress other && 
                   EmailAddress.Equals(other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Gets hash code based on email</summary>
        public override int GetHashCode() => EmailAddress.ToLowerInvariant().GetHashCode();
    }

    /// <summary>
    /// Represents a group of mailbox addresses (RFC 2822 Section 3.4)
    /// </summary>
    public class GroupAddress : Address
    {
        /// <summary>Gets or sets the group name</summary>
        public string GroupName { get; set; }

        /// <summary>Gets the collection of addresses in this group</summary>
        public List<MailboxAddress> Members { get; } = new();

        /// <summary>Gets if this is a group address</summary>
        public override bool IsGroupAddress => true;

        /// <summary>Initializes a new group address</summary>
        public GroupAddress(string groupName)
        {
            GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
        }

        /// <summary>Adds a member to the group</summary>
        public void AddMember(MailboxAddress address)
        {
            if (address != null && !Members.Contains(address))
                Members.Add(address);
        }

        /// <summary>Removes a member from the group</summary>
        public void RemoveMember(MailboxAddress address)
        {
            Members.Remove(address);
        }

        /// <summary>Gets the display string</summary>
        public override string GetDisplayString()
        {
            var membersList = string.Join(", ", Members.ConvertAll(m => m.EmailAddress));
            return $"{GroupName}: {membersList};";
        }

        /// <summary>Returns string representation</summary>
        public override string ToString() => GetDisplayString();
    }

    /// <summary>
    /// Collection of mailbox addresses
    /// </summary>
    public class MailboxAddressCollection : List<MailboxAddress>
    {
        /// <summary>Gets an address by email (case-insensitive)</summary>
        public MailboxAddress GetByEmail(string email)
        {
            return Find(a => a.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Checks if an email address exists in the collection</summary>
        public bool ContainsEmail(string email)
        {
            return GetByEmail(email) != null;
        }

        /// <summary>Removes an address by email (case-insensitive)</summary>
        public bool RemoveByEmail(string email)
        {
            var address = GetByEmail(email);
            if (address != null)
                return Remove(address);
            return false;
        }

        /// <summary>Parses comma-separated email addresses and adds them</summary>
        public void AddFromString(string emailString)
        {
            if (string.IsNullOrWhiteSpace(emailString))
                return;

            // Simple parsing: split by comma, trim, and add
            var emails = emailString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var email in emails)
            {
                var trimmed = email.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    try
                    {
                        Add(new MailboxAddress(trimmed));
                    }
                    catch
                    {
                        // Skip invalid email addresses
                    }
                }
            }
        }

        /// <summary>Gets all email addresses as comma-separated string</summary>
        public string ToEmailString()
        {
            return string.Join(", ", ConvertAll(a => a.EmailAddress));
        }
    }

    /// <summary>
    /// Collection of addresses (mailbox or group)
    /// </summary>
    public class AddressCollection : List<Address>
    {
        /// <summary>Gets the mailbox addresses from this collection</summary>
        public List<MailboxAddress> GetMailboxAddresses()
        {
            var result = new List<MailboxAddress>();
            foreach (var addr in this)
            {
                if (addr is MailboxAddress mailbox)
                    result.Add(mailbox);
                else if (addr is GroupAddress group)
                    result.AddRange(group.Members);
            }
            return result;
        }

        /// <summary>Gets all email addresses as comma-separated string</summary>
        public string ToEmailString()
        {
            var emails = GetMailboxAddresses().ConvertAll(a => a.EmailAddress);
            return string.Join(", ", emails);
        }
    }
}
