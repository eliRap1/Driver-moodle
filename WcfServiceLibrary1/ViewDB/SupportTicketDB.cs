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

                    try { ticket.UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? DateTime.Parse(reader["UpdatedAt"].ToString()) : (DateTime?)null; }
                    catch { ticket.UpdatedAt = null; }

                    try { ticket.ClosedAt = reader["ClosedAt"] != DBNull.Value ? DateTime.Parse(reader["ClosedAt"].ToString()) : (DateTime?)null; }
                    catch { ticket.ClosedAt = null; }

                    try { ticket.AssignedTo = reader["AssignedTo"]?.ToString(); }
                    catch { ticket.AssignedTo = null; }

                    try { ticket.AdminNotes = reader["AdminNotes"]?.ToString(); }
                    catch { ticket.AdminNotes = null; }

                    try { ticket.Resolution = reader["Resolution"]?.ToString(); }
                    catch { ticket.Resolution = null; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("SupportTicketDB CreateModel Error: " + ex.Message);
                }
            }
        }

        private int GetNextTicketId()
        {
            object result = SelectScalar("SELECT MAX(TicketId) FROM [SupportTickets]");
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) + 1 : 1;
        }

        private int GetNextMessageId()
        {
            object result = SelectScalar("SELECT MAX(MessageId) FROM [TicketMessages]");
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) + 1 : 1;
        }

        public int CreateTicket(SupportTicket ticket)
        {
            int nextId = GetNextTicketId();

            string sql = @"INSERT INTO [SupportTickets] 
                ([TicketId], [UserId], [Username], [UserType], [Subject], [Description], 
                 [Status], [Priority], [CreatedAt]) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

            int result = SaveChanges(sql,
                new OleDbParameter("@ticketId", nextId),
                new OleDbParameter("@userId", ticket.UserId),
                new OleDbParameter("@username", ticket.Username ?? ""),
                new OleDbParameter("@userType", ticket.UserType ?? ""),
                new OleDbParameter("@subject", ticket.Subject ?? ""),
                new OleDbParameter("@description", ticket.Description ?? ""),
                new OleDbParameter("@status", ticket.Status ?? "Open"),
                new OleDbParameter("@priority", ticket.Priority ?? "Medium"),
                new OleDbParameter("@createdAt", ticket.CreatedAt));

            return result > 0 ? nextId : -1;
        }

        public List<SupportTicket> GetUserTickets(int userId)
        {
            return Select("SELECT * FROM [SupportTickets] WHERE UserId = ? ORDER BY CreatedAt DESC",
                new OleDbParameter("@userId", userId))
                .OfType<SupportTicket>().ToList();
        }

        public List<SupportTicket> GetAllTickets()
        {
            return Select("SELECT * FROM [SupportTickets] ORDER BY CreatedAt DESC")
                .OfType<SupportTicket>().ToList();
        }

        public SupportTicket GetTicketById(int ticketId)
        {
            return Select("SELECT * FROM [SupportTickets] WHERE TicketId = ?",
                new OleDbParameter("@ticketId", ticketId))
                .OfType<SupportTicket>().FirstOrDefault();
        }

        public void UpdateTicketStatus(int ticketId, string status, string assignedTo = null)
        {
            if (!string.IsNullOrEmpty(assignedTo))
            {
                SaveChanges("UPDATE [SupportTickets] SET [Status] = ?, [UpdatedAt] = ?, [AssignedTo] = ? WHERE [TicketId] = ?",
                    new OleDbParameter("@status", status),
                    new OleDbParameter("@updatedAt", DateTime.Now),
                    new OleDbParameter("@assignedTo", assignedTo),
                    new OleDbParameter("@ticketId", ticketId));
            }
            else
            {
                SaveChanges("UPDATE [SupportTickets] SET [Status] = ?, [UpdatedAt] = ? WHERE [TicketId] = ?",
                    new OleDbParameter("@status", status),
                    new OleDbParameter("@updatedAt", DateTime.Now),
                    new OleDbParameter("@ticketId", ticketId));
            }
        }

        public void CloseTicket(int ticketId, string resolution, string adminNotes = null)
        {
            SaveChanges(@"UPDATE [SupportTickets] 
                SET [Status] = ?, [Resolution] = ?, [AdminNotes] = ?, [ClosedAt] = ?, [UpdatedAt] = ?
                WHERE [TicketId] = ?",
                new OleDbParameter("@status", "Closed"),
                new OleDbParameter("@resolution", resolution ?? ""),
                new OleDbParameter("@adminNotes", adminNotes ?? ""),
                new OleDbParameter("@closedAt", DateTime.Now),
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@ticketId", ticketId));
        }

        public void AddTicketMessage(TicketMessage message)
        {
            int nextId = GetNextMessageId();

            int result = SaveChanges(@"INSERT INTO [TicketMessages] 
                ([TicketId], [SenderUsername], [IsAdmin], [Message], [SentAt]) 
                VALUES (?, ?, ?, ?, ?, ?)",
                new OleDbParameter("@ticketId", message.TicketId),
                new OleDbParameter("@senderUsername", message.SenderUsername ?? "Unknown"),
                new OleDbParameter("@isAdmin", message.IsAdmin),
                new OleDbParameter("@message", message.Message ?? ""),
                new OleDbParameter("@sentAt", message.SentAt));

            if (result > 0)
            {
                SaveChanges("UPDATE [SupportTickets] SET [UpdatedAt] = ? WHERE [TicketId] = ?",
                    new OleDbParameter("@updatedAt", DateTime.Now),
                    new OleDbParameter("@ticketId", message.TicketId));
            }
        }

        public List<TicketMessage> GetTicketMessages(int ticketId)
        {
            List<TicketMessage> messages = new List<TicketMessage>();
            try
            {
                connection.Open();
                command.CommandText = "SELECT * FROM [TicketMessages] WHERE TicketId = ? ORDER BY SentAt ASC";
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@ticketId", ticketId));
                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    messages.Add(new TicketMessage
                    {
                        MessageId = (int)reader["MessageId"],
                        TicketId = (int)reader["TicketId"],
                        SenderUsername = reader["SenderUsername"].ToString(),
                        IsAdmin = bool.Parse(reader["IsAdmin"].ToString()),
                        Message = reader["Message"].ToString(),
                        SentAt = DateTime.Parse(reader["SentAt"].ToString())
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetTicketMessages Error: " + ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            }
            return messages;
        }

        public void UpdateTicketPriority(int ticketId, string priority)
        {
            SaveChanges("UPDATE [SupportTickets] SET [Priority] = ?, [UpdatedAt] = ? WHERE [TicketId] = ?",
                new OleDbParameter("@priority", priority),
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@ticketId", ticketId));
        }
    }
}