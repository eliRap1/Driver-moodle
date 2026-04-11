using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Model
{
    /// <summary>
    /// Represents a notification/message between users (Teacher-Student communication)
    /// Used for lesson cancellations, payment confirmations, reminders, etc.
    ///
    /// TABLE SCHEMA (Notifications):
    /// - NotificationID (AutoNumber, Primary Key)
    /// - SenderId (Integer)
    /// - SenderName (Text, 50)
    /// - SenderType (Text, 20) - "Teacher", "Student", "System"
    /// - RecipientId (Integer)
    /// - RecipientType (Text, 20) - "Teacher", "Student"
    /// - Title (Text, 100)
    /// - Message (Memo)
    /// - NotificationType (Text, 30) - "Message", "LessonCancelled", "PaymentReceived", "Reminder"
    /// - IsRead (Yes/No)
    /// - CreatedAt (Date/Time)
    /// - ReadAt (Date/Time)
    /// </summary>
    [DataContract]
    public class Notification : Base
    {
        [DataMember]
        public int SenderId { get; set; }

        [DataMember]
        public string SenderName { get; set; }

        [DataMember]
        public string SenderType { get; set; } // Teacher, Student, System

        [DataMember]
        public int RecipientId { get; set; }

        [DataMember]
        public string RecipientType { get; set; } // Teacher, Student

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string NotificationType { get; set; } // Message, LessonCancelled, PaymentReceived, Reminder

        [DataMember]
        public bool IsRead { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime? ReadAt { get; set; }

        public Notification()
        {
            IsRead = false;
            CreatedAt = DateTime.Now;
            NotificationType = "Message";
            SenderType = "System";
        }
    }

    /// <summary>
    /// Collection class for notifications, used for WCF serialization
    /// </summary>
    [CollectionDataContract]
    public class NotificationList : List<Notification>
    {
        public NotificationList() { }

        public NotificationList(IEnumerable<Base> list)
            : base(list.Cast<Notification>().ToList()) { }
    }
}
