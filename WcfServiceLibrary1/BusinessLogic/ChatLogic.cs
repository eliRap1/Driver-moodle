using Model;
using System.Collections.Generic;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for chat: global room and private student/teacher conversations.
    /// </summary>
    public static class ChatLogic
    {
        public static List<Chats> GetAllChatGlobal()
        {
            return new ChatDB().GetAllChatGlobal();
        }

        public static void AddMessageGlobal(string message, int userid, string username, bool IsTeacher)
        {
            new ChatDB().AddMessageGlobal(message, userid, username, IsTeacher);
        }

        public static List<Chats> GetChatPrivate(int studentid, int teacherid)
        {
            return new ChatDB().GetChatPrivate(studentid, teacherid);
        }

        public static void AddMessagePrivate(string message, int studentid, int teacherid, string username)
        {
            new ChatDB().AddMessagePrivate(message, studentid, teacherid, username);
        }
    }
}
