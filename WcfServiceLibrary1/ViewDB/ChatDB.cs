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
        public DateTime Time { get; set; }
        public int UserId { get; set; }
        public bool IsTeacher { get; set; }
        public string Username { get; set; }
        public int id { get; set; }
    }

    public class ChatDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Chats();
        }
        protected List<Chats> SelectChat(string sqlCommandTxt)
        {
            List<Chats> list = new List<Chats>();
            try
            {
                connection.Open(); //was missing
                command.CommandText = sqlCommandTxt;
                reader = command.ExecuteReader();
                //NULLבנתיים לא בודקים האם אחד השדות הוא 
                while (reader.Read())
                {
                    Chats entity = new Chats(); //יוצר אובייקט מטיפוס המתאים
                    CreateModel(entity);
                    list.Add(entity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message); //will word is every world, not only in world of Console

                //the output - we'll see in the output window of VisualStudio
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return list;
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
                    s.Time = DateTime.Parse(reader["Time"].ToString());
                    s.UserId = int.Parse(reader["UserId"].ToString());
                    s.IsTeacher = bool.Parse(reader["IsTeacher"].ToString());
                    s.Username = reader["Username"].ToString();
                    s.id = int.Parse(reader["id"].ToString());

                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public List<Chats> GetAllChat()
        {
            string sqlStr = "Select * From Chat";
            List<Chats> list = SelectChat(sqlStr);
            return list;
        }
        public void AddMessage(string message,int userid,string username, bool IsTeacher)
        {
            DateTime time = DateTime.Now;
            string timeStr = time.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = $"Insert into [Chat] ([Message],[UserID],[Time], IsTeacher,[username]) Values ('{message}',{userid},'{timeStr}',{IsTeacher},'{username}')";
            SaveChanges(sql);
        }
    }
}
