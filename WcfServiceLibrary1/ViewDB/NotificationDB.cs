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
        private static bool _schemaChecked = false;
        private static readonly object _schemaLock = new object();

        public NotificationDB()
        {
            EnsureSchema();
        }

        private void EnsureSchema()
        {
            if (_schemaChecked) return;
            lock (_schemaLock)
            {
                if (_schemaChecked) return;
                try
                {
                    using (var conn = GetConnection())
                    {
                        conn.Open();

                        bool tableExists = false;
                        var tables = conn.GetSchema("Tables", new[] { null, null, null, "TABLE" });
                        foreach (System.Data.DataRow row in tables.Rows)
                        {
                            if (string.Equals(row["TABLE_NAME"]?.ToString(), "Notifications",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                tableExists = true;
                                break;
                            }
                        }

                        bool needsRecreate = false;
                        if (tableExists)
                        {
                            var requiredCols = new[]
                            {
                                "id", "SenderId", "SenderName", "SenderType",
                                "RecipientId", "RecipientType", "Title", "Message",
                                "NotificationType", "IsRead", "CreatedAt", "ReadAt"
                            };
                            var existingCols = new System.Collections.Generic.HashSet<string>(
                                StringComparer.OrdinalIgnoreCase);
                            var colSchema = conn.GetSchema("Columns", new[] { null, null, "Notifications", null });
                            foreach (System.Data.DataRow row in colSchema.Rows)
                            {
                                existingCols.Add(row["COLUMN_NAME"]?.ToString() ?? "");
                            }
                            foreach (var c in requiredCols)
                            {
                                if (!existingCols.Contains(c))
                                {
                                    needsRecreate = true;
                                    System.Diagnostics.Debug.WriteLine(
                                        "Notifications table missing column: " + c + " - recreating.");
                                    break;
                                }
                            }
                        }

                        if (tableExists && needsRecreate)
                        {
                            using (var drop = new OleDbCommand("DROP TABLE [Notifications]", conn))
                            {
                                drop.ExecuteNonQuery();
                            }
                            tableExists = false;
                        }

                        if (!tableExists)
                        {
                            string ddl = @"CREATE TABLE [Notifications] (
                                [id] COUNTER PRIMARY KEY,
                                [SenderId] LONG,
                                [SenderName] TEXT(50),
                                [SenderType] TEXT(20),
                                [RecipientId] LONG,
                                [RecipientType] TEXT(20),
                                [Title] TEXT(255),
                                [Message] MEMO,
                                [NotificationType] TEXT(30),
                                [IsRead] BIT,
                                [CreatedAt] DATETIME,
                                [ReadAt] DATETIME
                            )";
                            using (var cmd = new OleDbCommand(ddl, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            System.Diagnostics.Debug.WriteLine("Notifications table created.");
                        }
                    }
                    _schemaChecked = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("EnsureSchema(Notifications) error: " + ex.Message);
                }
            }
        }

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
        /// Send a new notification. Bypasses BaseDB.SaveChanges so OleDb errors propagate
        /// instead of being swallowed - lets the WCF fault carry the real cause to the client.
        /// </summary>
        public int SendNotification(Notification notification)
        {
            string sql = @"INSERT INTO [Notifications]
                ([SenderId], [SenderName], [SenderType], [RecipientId], [RecipientType],
                 [Title], [Message], [NotificationType], [IsRead], [CreatedAt])
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.Add(new OleDbParameter("@senderId", OleDbType.Integer) { Value = notification.SenderId });
                cmd.Parameters.Add(new OleDbParameter("@senderName", OleDbType.VarWChar, 50) { Value = (object)notification.SenderName ?? "" });
                cmd.Parameters.Add(new OleDbParameter("@senderType", OleDbType.VarWChar, 20) { Value = (object)notification.SenderType ?? "System" });
                cmd.Parameters.Add(new OleDbParameter("@recipientId", OleDbType.Integer) { Value = notification.RecipientId });
                cmd.Parameters.Add(new OleDbParameter("@recipientType", OleDbType.VarWChar, 20) { Value = (object)notification.RecipientType ?? "" });
                cmd.Parameters.Add(new OleDbParameter("@title", OleDbType.VarWChar, 255) { Value = (object)notification.Title ?? "" });
                cmd.Parameters.Add(new OleDbParameter("@message", OleDbType.LongVarWChar) { Value = (object)notification.Message ?? "" });
                cmd.Parameters.Add(new OleDbParameter("@notificationType", OleDbType.VarWChar, 30) { Value = (object)notification.NotificationType ?? "Message" });
                cmd.Parameters.Add(new OleDbParameter("@isRead", OleDbType.Boolean) { Value = false });
                cmd.Parameters.Add(new OleDbParameter("@createdAt", OleDbType.Date) { Value = DateTime.Now });

                try
                {
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected <= 0)
                        throw new InvalidOperationException("INSERT into Notifications affected 0 rows.");
                    return affected;
                }
                catch (OleDbException ex)
                {
                    throw new InvalidOperationException(
                        "Notifications INSERT failed: " + ex.Message +
                        " | SQL: " + sql, ex);
                }
            }
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
        /// Mark a notification as read. Bypasses BaseDB.SaveChanges so OleDb errors propagate.
        /// </summary>
        public void MarkAsRead(int notificationId)
        {
            string sql = "UPDATE [Notifications] SET [IsRead] = ?, [ReadAt] = ? WHERE [id] = ?";
            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.Add(new OleDbParameter("@isRead", OleDbType.Boolean) { Value = true });
                cmd.Parameters.Add(new OleDbParameter("@readAt", OleDbType.Date) { Value = DateTime.Now });
                cmd.Parameters.Add(new OleDbParameter("@id", OleDbType.Integer) { Value = notificationId });

                try
                {
                    conn.Open();
                    int affected = cmd.ExecuteNonQuery();
                    if (affected <= 0)
                        throw new InvalidOperationException(
                            $"MarkAsRead: notification id={notificationId} not found.");
                }
                catch (OleDbException ex)
                {
                    throw new InvalidOperationException(
                        "MarkAsRead failed: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Mark all notifications as read for a user. Bypasses BaseDB.SaveChanges.
        /// </summary>
        public void MarkAllAsRead(int userId, string userType)
        {
            string sql = @"UPDATE [Notifications]
                SET [IsRead] = ?, [ReadAt] = ?
                WHERE [RecipientId] = ? AND [RecipientType] = ? AND [IsRead] = ?";

            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.Add(new OleDbParameter("@isRead", OleDbType.Boolean) { Value = true });
                cmd.Parameters.Add(new OleDbParameter("@readAt", OleDbType.Date) { Value = DateTime.Now });
                cmd.Parameters.Add(new OleDbParameter("@recipientId", OleDbType.Integer) { Value = userId });
                cmd.Parameters.Add(new OleDbParameter("@recipientType", OleDbType.VarWChar, 20) { Value = userType ?? "" });
                cmd.Parameters.Add(new OleDbParameter("@isReadOld", OleDbType.Boolean) { Value = false });

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (OleDbException ex)
                {
                    throw new InvalidOperationException(
                        "MarkAllAsRead failed: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Delete a notification. Bypasses BaseDB.SaveChanges so errors propagate.
        /// </summary>
        public void DeleteNotification(int notificationId)
        {
            string sql = "DELETE FROM [Notifications] WHERE [id] = ?";
            using (var conn = GetConnection())
            using (var cmd = new OleDbCommand(sql, conn))
            {
                cmd.Parameters.Add(new OleDbParameter("@id", OleDbType.Integer) { Value = notificationId });
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (OleDbException ex)
                {
                    throw new InvalidOperationException(
                        "DeleteNotification failed: " + ex.Message, ex);
                }
            }
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

            int affectedRows = SendNotification(notification);
            if (affectedRows <= 0)
                throw new InvalidOperationException("Lesson cancellation notification was not saved.");
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

            int affectedRows = SendNotification(notification);
            if (affectedRows <= 0)
                throw new InvalidOperationException("Teacher message was not saved.");
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

            int affectedRows = SendNotification(notification);
            if (affectedRows <= 0)
                throw new InvalidOperationException("Student message was not saved.");
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

            int affectedRows = SendNotification(notification);
            if (affectedRows <= 0)
                throw new InvalidOperationException("Payment notification was not saved.");
        }
    }
}
