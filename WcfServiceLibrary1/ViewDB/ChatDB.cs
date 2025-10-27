using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

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
        //protected List<Chats> SelectChat(string sqlCommandTxt)
        //{
        //    List<Chats> list = new List<Chats>();
        //    try
        //    {
        //        connection.Open(); //was missing
        //        command.CommandText = sqlCommandTxt;
        //        reader = command.ExecuteReader();
        //        //NULLבנתיים לא בודקים האם אחד השדות הוא 
        //        while (reader.Read())
        //        {
        //            Chats entity = new Chats(); //יוצר אובייקט מטיפוס המתאים
        //            CreateModel(entity);
        //            list.Add(entity);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message); //will word is every world, not only in world of Console

        //        //the output - we'll see in the output window of VisualStudio
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //            reader.Close();
        //        if (connection.State == ConnectionState.Open)
        //            connection.Close();
        //    }
        //    return list;
        //}
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
                    s.studentId = int.Parse(reader["studentId"].ToString());
                    s.teacherId = int.Parse(reader["teacherId"].ToString());

                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public List<Chats> GetAllChatGlobal()
        {
            string sqlStr = "Select * From GlobalChat";
            List<Chats> list = Select(sqlStr).Cast<Chats>().ToList();
            return list;
        }
        public void AddMessageGlobal(string message,int userid,string username, bool IsTeacher)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = $"Insert into [GlobalChat] ([Message],[UserID],[SentAT], IsTeacher,[username]) Values ('{message}',{userid},'{timeStr}',{IsTeacher},'{username}')";
            SaveChanges(sql);
        }
        public List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            string sqlStr = "Select * From GlobalChat where (studentId=" + studentid + " and teacherId=" + teacherid+")";
            List<Chats> list = Select(sqlStr).Cast<Chats>().ToList();
            return list;
        }
        public void AddMessagePrivate(string message, int studentid, int teacherid, string username)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = $"INSERT into [PrivateChat] ([Message],teacherID,studentID,[username],sentAt) VALUES ('{message}',{teacherid},{studentid},'{username}')";
            SaveChanges(sql);
        }
    }
}
