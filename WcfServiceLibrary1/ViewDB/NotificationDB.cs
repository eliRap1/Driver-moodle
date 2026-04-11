using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    /// <summary>
    /// Database access class for Notifications table.
    /// Handles notification/messaging between teachers and students.
    ///
    /// TABLE SCHEMA (Notifications):
    /// - id (AutoNumber, Primary Key)
    /// - SenderId (Integer)
    /// - SenderName (Text, 50)
    /// - SenderType (Text, 20)
    /// - RecipientId (Integer)
    /// - RecipientType (Text, 20)
    /// - Title (Text, 100)
    /// - Message (Memo)
    /// - NotificationType (Text, 30)
    /// - IsRead (Yes/No)
    /// - CreatedAt (Date/Time)
    /// - ReadAt (Date/Time)
    /// </summary>
    public class NotificationDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Notification();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Notification n = (Notification)entity;

                    try { n.SenderId = Convert.ToInt32(reader["SenderId"]); }
                    catch { n.SenderId = 0; }

                    try { n.SenderName = reader["SenderName"].ToString(); }
                    catch { n.SenderName = ""; }

                    try { n.SenderType = reader["SenderType"].ToString(); }
                    catch { n.SenderType = "System"; }

                    try { n.RecipientId = Convert.ToInt32(reader["RecipientId"]); }
                    catch { n.RecipientId = 0; }

                    try { n.RecipientType = reader["RecipientType"].ToString(); }
                    catch { n.RecipientType = ""; }

                    try { n.Title = reader["Title"].ToString(); }
                    catch { n.Title = ""; }

                    try { n.Message = reader["Message"].ToString(); }
                    catch { n.Message = ""; }

                    try { n.NotificationType = reader["NotificationType"].ToString(); }
                    catch { n.NotificationType = "Message"; }

                    try { n.IsRead = bool.Parse(reader["IsRead"].ToString()); }
                    catch { n.IsRead = false; }

                    try { n.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()); }
                    catch { n.CreatedAt = DateTime.Now; }

                    try
                    {
                        var readAt = reader["ReadAt"];
                        if (readAt != DBNull.Value)
                            n.ReadAt = DateTime.Parse(readAt.ToString());
                    }
                    catch { n.ReadAt = null; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("NotificationDB CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Send a new notification
        /// </summary>
        public int SendNotification(Notification notification)
        {
            string sql = @"INSERT INTO [Notifications]
                ([SenderId], [SenderName], [SenderType], [RecipientId], [RecipientType],
                 [Title], [Message], [NotificationType], [IsRead], [CreatedAt])
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@senderId", OleDbType.Integer) { Value = notification.SenderId },
                new OleDbParameter("@senderName", OleDbType.VarWChar) { Value = notification.SenderName ?? "" },
                new OleDbParameter("@senderType", OleDbType.VarWChar) { Value = notification.SenderType ?? "System" },
                new OleDbParameter("@recipientId", OleDbType.Integer) { Value = notification.RecipientId },
                new OleDbParameter("@recipientType", OleDbType.VarWChar) { Value = notification.RecipientType ?? "" },
                new OleDbParameter("@title", OleDbType.VarWChar) { Value = notification.Title ?? "" },
                new OleDbParameter("@message", OleDbType.LongVarWChar) { Value = notification.Message ?? "" },
                new OleDbParameter("@notificationType", OleDbType.VarWChar) { Value = notification.NotificationType ?? "Message" },
                new OleDbParameter("@isRead", OleDbType.Boolean) { Value = false },
                new OleDbParameter("@createdAt", OleDbType.Date) { Value = DateTime.Now }
            };

            return SaveChanges(sql, parameters);
        }

        /// <summary>
        /// Get notifications for a user (student or teacher)
        /// </summary>
        public List<Notification> GetUserNotifications(int userId, string userType)
        {
            string sql = @"SELECT * FROM [Notifications]
                WHERE [RecipientId] = ? AND [RecipientType] = ?
                ORDER BY [CreatedAt] DESC";

            return Select(sql,
                new OleDbParameter("@recipientId", userId),
                new OleDbParameter("@recipientType", userType))
                .OfType<Notification>()
                .ToList();
        }

        /// <summary>
        /// Get unread notifications count for a user
        /// </summary>
        public int GetUnreadCount(int userId, string userType)
        {
            string sql = @"SELECT COUNT(*) FROM [Notifications]
                WHERE [RecipientId] = ? AND [RecipientType] = ? AND [IsRead] = ?";

            object result = SelectScalar(sql,
                new OleDbParameter("@recipientId", userId),
                new OleDbParameter("@recipientType", userType),
                new OleDbParameter("@isRead", false));

            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : 0;
        }

        /// <summary>
        /// Get unread notifications for a user
        /// </summary>
        public List<Notification> GetUnreadNotifications(int userId, string userType)
        {
            string sql = @"SELECT * FROM [Notifications]
                WHERE [RecipientId] = ? AND [RecipientType] = ? AND [IsRead] = ?
                ORDER BY [CreatedAt] DESC";

            return Select(sql,
                new OleDbParameter("@recipientId", userId),
                new OleDbParameter("@recipientType", userType),
                new OleDbParameter("@isRead", false))
                .OfType<Notification>()
                .ToList();
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        public void MarkAsRead(int notificationId)
        {
            string sql = "UPDATE [Notifications] SET [IsRead] = ?, [ReadAt] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@isRead", true),
                new OleDbParameter("@readAt", DateTime.Now),
                new OleDbParameter("@id", notificationId));
        }

        /// <summary>
        /// Mark all notifications as read for a user
        /// </summary>
        public void MarkAllAsRead(int userId, string userType)
        {
            string sql = @"UPDATE [Notifications]
                SET [IsRead] = ?, [ReadAt] = ?
                WHERE [RecipientId] = ? AND [RecipientType] = ? AND [IsRead] = ?";

            SaveChanges(sql,
                new OleDbParameter("@isRead", true),
                new OleDbParameter("@readAt", DateTime.Now),
                new OleDbParameter("@recipientId", userId),
                new OleDbParameter("@recipientType", userType),
                new OleDbParameter("@isReadOld", false));
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        public void DeleteNotification(int notificationId)
        {
            string sql = "DELETE FROM [Notifications] WHERE [id] = ?";
            SaveChanges(sql, new OleDbParameter("@id", notificationId));
        }

        /// <summary>
        /// Send a lesson cancellation notification to the teacher
        /// </summary>
        public void SendLessonCancelledNotification(int studentId, string studentName, int teacherId, string lessonDate, string lessonTime)
        {
            var notification = new Notification
            {
                SenderId = studentId,
                SenderName = studentName,
                SenderType = "Student",
                RecipientId = teacherId,
                RecipientType = "Teacher",
                Title = "Lesson Cancelled",
                Message = $"Student {studentName} has cancelled their lesson scheduled for {lessonDate} at {lessonTime}.",
                NotificationType = "LessonCancelled"
            };

            SendNotification(notification);
        }

        /// <summary>
        /// Send a message from teacher to student
        /// </summary>
        public void SendTeacherMessage(int teacherId, string teacherName, int studentId, string title, string message)
        {
            var notification = new Notification
            {
                SenderId = teacherId,
                SenderName = teacherName,
                SenderType = "Teacher",
                RecipientId = studentId,
                RecipientType = "Student",
                Title = title,
                Message = message,
                NotificationType = "Message"
            };

            SendNotification(notification);
        }

        /// <summary>
        /// Send a message from student to teacher
        /// </summary>
        public void SendStudentMessage(int studentId, string studentName, int teacherId, string title, string message)
        {
            var notification = new Notification
            {
                SenderId = studentId,
                SenderName = studentName,
                SenderType = "Student",
                RecipientId = teacherId,
                RecipientType = "Teacher",
                Title = title,
                Message = message,
                NotificationType = "Message"
            };

            SendNotification(notification);
        }

        /// <summary>
        /// Send a payment confirmation notification
        /// </summary>
        public void SendPaymentNotification(int studentId, string studentName, int teacherId, int amount, string paymentMethod)
        {
            var notification = new Notification
            {
                SenderId = studentId,
                SenderName = studentName,
                SenderType = "Student",
                RecipientId = teacherId,
                RecipientType = "Teacher",
                Title = "Payment Received",
                Message = $"Payment of {amount} NIS received from {studentName} via {paymentMethod}.",
                NotificationType = "PaymentReceived"
            };

            SendNotification(notification);
        }
    }
}
