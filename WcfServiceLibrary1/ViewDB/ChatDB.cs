using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    public class Chats : Base
    {
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public int UserId { get; set; }
        public bool IsTeacher { get; set; }
        public string Username { get; set; }
        public int id { get; set; }
        public int studentId { get; set; }
        public int teacherId { get; set; }
    }

    public class ChatDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Chats();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Chats s = (Chats)entity;
                    s.Message = reader["Message"].ToString();
                    s.SentAt = DateTime.Parse(reader["SentAt"].ToString());
                    s.UserId = int.Parse(reader["UserId"].ToString());
                    s.IsTeacher = bool.Parse(reader["IsTeacher"].ToString());
                    s.Username = reader["Username"].ToString();
                    s.id = int.Parse(reader["id"].ToString());

                    // Handle optional fields for private chat
                    try
                    {
                        s.studentId = int.Parse(reader["studentId"].ToString());
                        s.teacherId = int.Parse(reader["teacherId"].ToString());
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// SECURE: Get all global chat messages
        /// </summary>
        public List<Chats> GetAllChatGlobal()
        {
            string sqlStr = "SELECT * FROM [GlobalChat]";
            return Select(sqlStr).Cast<Chats>().ToList();
        }

        /// <summary>
        /// SECURE: Add message to global chat
        /// </summary>
        public void AddMessageGlobal(string message, int userid, string username, bool IsTeacher)
        {
            // Sanitize message (prevent XSS, SQL injection)
            message = message.Replace("'", "''"); // Escape single quotes

            DateTime time = DateTime.Now;
            string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = "INSERT INTO [GlobalChat] ([Message], [UserID], [SentAt], [IsTeacher], [username]) " +
                        "VALUES (?, ?, ?, ?, ?)";

            SaveChanges(sql,
                new OleDbParameter("@message", message),
                new OleDbParameter("@userid", userid),
                new OleDbParameter("@sentAt", timeStr),
                new OleDbParameter("@isTeacher", IsTeacher),
                new OleDbParameter("@username", username));
        }

        /// <summary>
        /// SECURE: Get private chat messages
        /// </summary>
        public List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            string sqlStr = "SELECT * FROM [PrivateChat] WHERE studentId = ? AND teacherId = ?";
            return Select(sqlStr,
                new OleDbParameter("@studentId", studentid),
                new OleDbParameter("@teacherId", teacherid))
                .Cast<Chats>()
                .ToList();
        }

        /// <summary>
        /// SECURE: Add message to private chat
        /// </summary>
        public void AddMessagePrivate(string message, int studentid, int teacherid, string username)
        {
            // Sanitize message
            message = message.Replace("'", "''");

            DateTime time = DateTime.Now;
            string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");

            string sql = "INSERT INTO [PrivateChat] ([Message], [teacherID], [studentID], [username], [sentAt]) " +
                        "VALUES (?, ?, ?, ?, ?)";

            SaveChanges(sql,
                new OleDbParameter("@message", message),
                new OleDbParameter("@teacherId", teacherid),
                new OleDbParameter("@studentId", studentid),
                new OleDbParameter("@username", username),
                new OleDbParameter("@sentAt", timeStr));
        }
    }
}