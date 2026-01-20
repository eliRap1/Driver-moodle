using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Model
{
    [DataContract]
    [KnownType(typeof(SupportTicket))]  
    public class SupportTicket : Base
    {
        [DataMember]
        public int TicketId { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserType { get; set; } // "Student" or "Teacher"

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Status { get; set; } // "Open", "In Progress", "Resolved", "Closed"

        [DataMember]
        public string Priority { get; set; } // "Low", "Medium", "High", "Urgent"

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime? UpdatedAt { get; set; }

        [DataMember]
        public DateTime? ClosedAt { get; set; }

        [DataMember]
        public string AssignedTo { get; set; } // Admin username

        [DataMember]
        public string AdminNotes { get; set; }

        [DataMember]
        public string Resolution { get; set; }

        [DataMember]
        public List<TicketMessage> Messages { get; set; }

        public SupportTicket()
        {
            Messages = new List<TicketMessage>();
            Status = "Open";
            Priority = "Medium";
            CreatedAt = DateTime.Now;
        }
    }

    [DataContract]
    public class TicketMessage
    {
        [DataMember]
        public int MessageId { get; set; }

        [DataMember]
        public int TicketId { get; set; }

        [DataMember]
        public string SenderUsername { get; set; }

        [DataMember]
        public bool IsAdmin { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime SentAt { get; set; }

        public TicketMessage()
        {
            SentAt = DateTime.Now;
        }
    }

    [CollectionDataContract]
    public class SupportTicketList : List<SupportTicket>
    {
        public SupportTicketList() { }

        public SupportTicketList(IEnumerable<Base> list)
            : base(list.Cast<SupportTicket>().ToList()) { }
    }
}