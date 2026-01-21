using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    public class SupportTicketDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new SupportTicket();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    SupportTicket ticket = (SupportTicket)entity;
                    ticket.TicketId = (int)reader["TicketId"];
                    ticket.UserId = (int)reader["UserId"];
                    ticket.Username = reader["Username"].ToString();
                    ticket.UserType = reader["UserType"].ToString();
                    ticket.Subject = reader["Subject"].ToString();
                    ticket.Description = reader["Description"].ToString();
                    ticket.Status = reader["Status"].ToString();
                    ticket.Priority = reader["Priority"].ToString();
                    ticket.CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString());

                    if (reader["UpdatedAt"] != DBNull.Value)
                        ticket.UpdatedAt = DateTime.Parse(reader["UpdatedAt"].ToString());

                    if (reader["ClosedAt"] != DBNull.Value)
                        ticket.ClosedAt = DateTime.Parse(reader["ClosedAt"].ToString());

                    if (reader["AssignedTo"] != DBNull.Value)
                        ticket.AssignedTo = reader["AssignedTo"].ToString();

                    if (reader["AdminNotes"] != DBNull.Value)
                        ticket.AdminNotes = reader["AdminNotes"].ToString();

                    if (reader["Resolution"] != DBNull.Value)
                        ticket.Resolution = reader["Resolution"].ToString();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SupportTicketDB CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Create a new support ticket
        /// </summary>
        public int CreateTicket(SupportTicket ticket)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CreateTicket in DB ===");

                // Get next TicketId
                int nextId = GetNextTicketId();
                System.Diagnostics.Debug.WriteLine($"Next TicketId: {nextId}");

                string sql = @"INSERT INTO [SupportTickets] 
                    ([TicketId], [UserId], [Username], [UserType], [Subject], [Description], [Status], [Priority], [CreatedAt]) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

                int rowsAffected = SaveChanges(sql,
                    new OleDbParameter("@ticketId", nextId),
                    new OleDbParameter("@userId", ticket.UserId),
                    new OleDbParameter("@username", ticket.Username),
                    new OleDbParameter("@userType", ticket.UserType),
                    new OleDbParameter("@subject", ticket.Subject),
                    new OleDbParameter("@description", ticket.Description),
                    new OleDbParameter("@status", ticket.Status ?? "Open"),
                    new OleDbParameter("@priority", ticket.Priority ?? "Medium"),
                    new OleDbParameter("@createdAt", ticket.CreatedAt));

                System.Diagnostics.Debug.WriteLine($"Rows affected: {rowsAffected}");

                if (rowsAffected > 0)
                {
                    return nextId;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateTicket Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                throw;
            }
        }

        private int GetNextTicketId()
        {
            try
            {
                string sql = "SELECT MAX(TicketId) FROM [SupportTickets]";
                object result = SelectScalar(sql);
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result) + 1;
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Get all tickets for a specific user
        /// </summary>
        public List<SupportTicket> GetUserTickets(int userId)
        {
            string sql = "SELECT * FROM [SupportTickets] WHERE UserId = ? ORDER BY CreatedAt DESC";
            return Select(sql, new OleDbParameter("@userId", userId))
                .OfType<SupportTicket>()
                .ToList();
        }

        /// <summary>
        /// Get all tickets (for admin)
        /// </summary>
        public List<SupportTicket> GetAllTickets()
        {
            string sql = "SELECT * FROM [SupportTickets] ORDER BY CreatedAt DESC";
            return Select(sql).OfType<SupportTicket>().ToList();
        }

        /// <summary>
        /// Get tickets by status
        /// </summary>
        public List<SupportTicket> GetTicketsByStatus(string status)
        {
            string sql = "SELECT * FROM [SupportTickets] WHERE Status = ? ORDER BY CreatedAt DESC";
            return Select(sql, new OleDbParameter("@status", status))
                .OfType<SupportTicket>()
                .ToList();
        }

        /// <summary>
        /// Get a specific ticket by ID
        /// </summary>
        public SupportTicket GetTicketById(int ticketId)
        {
            string sql = "SELECT * FROM [SupportTickets] WHERE TicketId = ?";
            var tickets = Select(sql, new OleDbParameter("@ticketId", ticketId))
                .OfType<SupportTicket>()
                .ToList();

            return tickets.FirstOrDefault();
        }

        /// <summary>
        /// FIXED: Update ticket status - now properly updates the database
        /// </summary>
        public void UpdateTicketStatus(int ticketId, string status, string assignedTo = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTicketStatus: TicketId={ticketId}, Status={status}, AssignedTo={assignedTo}");

                string sql;
                int result;

                if (!string.IsNullOrEmpty(assignedTo))
                {
                    sql = @"UPDATE [SupportTickets] 
                        SET [Status] = ?, [UpdatedAt] = ?, [AssignedTo] = ?
                        WHERE [TicketId] = ?";

                    result = SaveChanges(sql,
                        new OleDbParameter("@status", status),
                        new OleDbParameter("@updatedAt", DateTime.Now),
                        new OleDbParameter("@assignedTo", assignedTo),
                        new OleDbParameter("@ticketId", ticketId));
                }
                else
                {
                    sql = @"UPDATE [SupportTickets] 
                        SET [Status] = ?, [UpdatedAt] = ?
                        WHERE [TicketId] = ?";

                    result = SaveChanges(sql,
                        new OleDbParameter("@status", status),
                        new OleDbParameter("@updatedAt", DateTime.Now),
                        new OleDbParameter("@ticketId", ticketId));
                }

                System.Diagnostics.Debug.WriteLine($"UpdateTicketStatus result: {result} rows affected");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTicketStatus Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Close a ticket with resolution
        /// </summary>
        public void CloseTicket(int ticketId, string resolution, string adminNotes = null)
        {
            try
            {
                string sql = @"UPDATE [SupportTickets] 
                    SET [Status] = ?, [Resolution] = ?, [AdminNotes] = ?, [ClosedAt] = ?, [UpdatedAt] = ?
                    WHERE [TicketId] = ?";

                int result = SaveChanges(sql,
                    new OleDbParameter("@status", "Closed"),
                    new OleDbParameter("@resolution", resolution ?? ""),
                    new OleDbParameter("@adminNotes", adminNotes ?? ""),
                    new OleDbParameter("@closedAt", DateTime.Now),
                    new OleDbParameter("@updatedAt", DateTime.Now),
                    new OleDbParameter("@ticketId", ticketId));

                System.Diagnostics.Debug.WriteLine($"CloseTicket result: {result} rows affected");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseTicket Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// FIXED: Add a message to a ticket - now properly inserts and updates
        /// </summary>
        public void AddTicketMessage(TicketMessage message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"AddTicketMessage: TicketId={message.TicketId}, Sender={message.SenderUsername}");

                // Get next MessageId
                int nextId = GetNextMessageId();
                System.Diagnostics.Debug.WriteLine($"Next MessageId: {nextId}");

                string sql = @"INSERT INTO [TicketMessages] 
                    ([MessageId], [TicketId], [SenderUsername], [IsAdmin], [Message], [SentAt]) 
                    VALUES (?, ?, ?, ?, ?, ?)";

                int result = SaveChanges(sql,
                    new OleDbParameter("@messageId", nextId),
                    new OleDbParameter("@ticketId", message.TicketId),
                    new OleDbParameter("@senderUsername", message.SenderUsername),
                    new OleDbParameter("@isAdmin", message.IsAdmin),
                    new OleDbParameter("@message", message.Message),
                    new OleDbParameter("@sentAt", message.SentAt));

                System.Diagnostics.Debug.WriteLine($"Message insert result: {result} rows affected");

                // Update ticket's UpdatedAt
                if (result > 0)
                {
                    string updateSql = "UPDATE [SupportTickets] SET [UpdatedAt] = ? WHERE [TicketId] = ?";
                    int updateResult = SaveChanges(updateSql,
                        new OleDbParameter("@updatedAt", DateTime.Now),
                        new OleDbParameter("@ticketId", message.TicketId));

                    System.Diagnostics.Debug.WriteLine($"Ticket UpdatedAt result: {updateResult} rows affected");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTicketMessage Error: {ex.Message}");
                throw;
            }
        }

        private int GetNextMessageId()
        {
            try
            {
                string sql = "SELECT MAX(MessageId) FROM [TicketMessages]";
                object result = SelectScalar(sql);
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result) + 1;
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// FIXED: Get all messages for a ticket
        /// </summary>
        public List<TicketMessage> GetTicketMessages(int ticketId)
        {
            List<TicketMessage> messages = new List<TicketMessage>();

            OleDbConnection conn = null;
            OleDbCommand cmd = null;
            OleDbDataReader rdr = null;

            try
            {
                conn = BaseDB.GetConnection();
                conn.Open();

                string sql = "SELECT * FROM [TicketMessages] WHERE TicketId = ? ORDER BY SentAt ASC";
                cmd = new OleDbCommand(sql, conn);
                cmd.Parameters.Add(new OleDbParameter("@ticketId", ticketId));

                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    var msg = new TicketMessage
                    {
                        MessageId = (int)rdr["MessageId"],
                        TicketId = (int)rdr["TicketId"],
                        SenderUsername = rdr["SenderUsername"].ToString(),
                        IsAdmin = bool.Parse(rdr["IsAdmin"].ToString()),
                        Message = rdr["Message"].ToString(),
                        SentAt = DateTime.Parse(rdr["SentAt"].ToString())
                    };
                    messages.Add(msg);
                }

                System.Diagnostics.Debug.WriteLine($"GetTicketMessages: Found {messages.Count} messages for ticket {ticketId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetTicketMessages Error: " + ex.Message);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null && conn.State == System.Data.ConnectionState.Open) conn.Close();
            }

            return messages;
        }

        /// <summary>
        /// Update ticket priority
        /// </summary>
        public void UpdateTicketPriority(int ticketId, string priority)
        {
            string sql = "UPDATE [SupportTickets] SET [Priority] = ?, [UpdatedAt] = ? WHERE [TicketId] = ?";
            SaveChanges(sql,
                new OleDbParameter("@priority", priority),
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@ticketId", ticketId));
        }
    }
}