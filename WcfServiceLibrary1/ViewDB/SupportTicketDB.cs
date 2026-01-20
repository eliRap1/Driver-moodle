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
                    Console.WriteLine("CreateModel Error: " + ex.Message);
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
                string sql = @"INSERT INTO [SupportTickets] 
                    ([UserId], [Username], [UserType], [Subject], [Description], [Status], [Priority], [CreatedAt]) 
                    VALUES (?, ?, ?, ?, ?, ?, ?, ?)";

                int rowsAffected = SaveChanges(sql,
                    new OleDbParameter("@userId", ticket.UserId),
                    new OleDbParameter("@username", ticket.Username),
                    new OleDbParameter("@userType", ticket.UserType),
                    new OleDbParameter("@subject", ticket.Subject),
                    new OleDbParameter("@description", ticket.Description),
                    new OleDbParameter("@status", ticket.Status ?? "Open"),
                    new OleDbParameter("@priority", ticket.Priority ?? "Medium"),
                    new OleDbParameter("@createdAt", ticket.CreatedAt));

                if (rowsAffected > 0)
                {
                    // Get the newly created ticket ID
                    string getIdSql = "SELECT MAX(TicketId) FROM [SupportTickets]";
                    object result = SelectScalar(getIdSql);
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateTicket Error: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                throw; // Re-throw to see the error in the client
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
        /// Update ticket status
        /// </summary>
        public void UpdateTicketStatus(int ticketId, string status, string assignedTo = null)
        {
            string sql = @"UPDATE [SupportTickets] 
                SET Status = ?, UpdatedAt = ?, AssignedTo = ?
                WHERE TicketId = ?";

            SaveChanges(sql,
                new OleDbParameter("@status", status),
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@assignedTo", (object)assignedTo ?? DBNull.Value),
                new OleDbParameter("@ticketId", ticketId));
        }

        /// <summary>
        /// Close a ticket with resolution
        /// </summary>
        public void CloseTicket(int ticketId, string resolution, string adminNotes = null)
        {
            string sql = @"UPDATE [SupportTickets] 
                SET Status = 'Closed', Resolution = ?, AdminNotes = ?, ClosedAt = ?, UpdatedAt = ?
                WHERE TicketId = ?";

            SaveChanges(sql,
                new OleDbParameter("@resolution", resolution),
                new OleDbParameter("@adminNotes", (object)adminNotes ?? DBNull.Value),
                new OleDbParameter("@closedAt", DateTime.Now),
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@ticketId", ticketId));
        }

        /// <summary>
        /// Add a message to a ticket
        /// </summary>
        public void AddTicketMessage(TicketMessage message)
        {
            string sql = @"INSERT INTO [TicketMessages] 
                (TicketId, SenderUsername, IsAdmin, Message, SentAt) 
                VALUES (?, ?, ?, ?, ?)";

            SaveChanges(sql,
                new OleDbParameter("@ticketId", message.TicketId),
                new OleDbParameter("@senderUsername", message.SenderUsername),
                new OleDbParameter("@isAdmin", message.IsAdmin),
                new OleDbParameter("@message", message.Message),
                new OleDbParameter("@sentAt", message.SentAt));

            // Update ticket's UpdatedAt
            string updateSql = "UPDATE [SupportTickets] SET UpdatedAt = ? WHERE TicketId = ?";
            SaveChanges(updateSql,
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@ticketId", message.TicketId));
        }

        /// <summary>
        /// Get all messages for a ticket
        /// </summary>
        public List<TicketMessage> GetTicketMessages(int ticketId)
        {
            string sql = "SELECT * FROM [TicketMessages] WHERE TicketId = ? ORDER BY SentAt ASC";
            List<TicketMessage> messages = new List<TicketMessage>();

            try
            {
                connection.Open();
                command.CommandText = sql;
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
                Console.WriteLine("GetTicketMessages Error: " + ex.Message);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return messages;
        }

        /// <summary>
        /// Update ticket priority
        /// </summary>
        public void UpdateTicketPriority(int ticketId, string priority)
        {
            string sql = "UPDATE [SupportTickets] SET Priority = ?, UpdatedAt = ? WHERE TicketId = ?";
            SaveChanges(sql,
                new OleDbParameter("@priority", priority),
                new OleDbParameter("@updatedAt", DateTime.Now),
                new OleDbParameter("@ticketId", ticketId));
        }
    }
}