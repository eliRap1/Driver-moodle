using Model;
using System;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for the support ticket system: creating tickets, messages,
    /// status/priority updates, and ticket queries.
    /// </summary>
    public static class SupportTicketLogic
    {
        public static int CreateSupportTicket(SupportTicket ticket)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CreateSupportTicket Called ===");
                System.Diagnostics.Debug.WriteLine($"UserId: {ticket.UserId}");
                System.Diagnostics.Debug.WriteLine($"Username: {ticket.Username}");
                System.Diagnostics.Debug.WriteLine($"UserType: {ticket.UserType}");
                System.Diagnostics.Debug.WriteLine($"Subject: {ticket.Subject}");

                int result = new SupportTicketDB().CreateTicket(ticket);

                System.Diagnostics.Debug.WriteLine($"Ticket created with ID: {result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CreateSupportTicket Error: {ex.Message}");
                throw;
            }
        }

        public static List<SupportTicket> GetUserTickets(int userId)
        {
            return new SupportTicketDB().GetUserTickets(userId);
        }

        public static List<SupportTicket> GetAllTickets()
        {
            return new SupportTicketDB().GetAllTickets();
        }

        public static SupportTicket GetTicketById(int ticketId)
        {
            return new SupportTicketDB().GetTicketById(ticketId);
        }

        public static void UpdateTicketStatus(int ticketId, string status, string assignedTo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== UpdateTicketStatus called ===");
                System.Diagnostics.Debug.WriteLine($"TicketId: {ticketId}, Status: {status}, AssignedTo: {assignedTo}");

                new SupportTicketDB().UpdateTicketStatus(ticketId, status, assignedTo);

                System.Diagnostics.Debug.WriteLine("UpdateTicketStatus completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTicketStatus Error: {ex.Message}");
                throw;
            }
        }

        public static void CloseTicket(int ticketId, string resolution, string adminNotes)
        {
            new SupportTicketDB().CloseTicket(ticketId, resolution, adminNotes);
        }

        public static void AddTicketMessage(TicketMessage message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== AddTicketMessage called ===");
                System.Diagnostics.Debug.WriteLine($"TicketId: {message.TicketId}");
                System.Diagnostics.Debug.WriteLine($"Sender: {message.SenderUsername}");
                System.Diagnostics.Debug.WriteLine($"IsAdmin: {message.IsAdmin}");
                System.Diagnostics.Debug.WriteLine($"Message: {message.Message}");

                new SupportTicketDB().AddTicketMessage(message);

                System.Diagnostics.Debug.WriteLine("AddTicketMessage completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTicketMessage Error: {ex.Message}");
                throw;
            }
        }

        public static List<TicketMessage> GetTicketMessages(int ticketId)
        {
            return new SupportTicketDB().GetTicketMessages(ticketId);
        }

        public static void UpdateTicketPriority(int ticketId, string priority)
        {
            new SupportTicketDB().UpdateTicketPriority(ticketId, priority);
        }
    }
}
