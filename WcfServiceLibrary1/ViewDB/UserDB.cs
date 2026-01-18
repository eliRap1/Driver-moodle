using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using Model.Helpers;

namespace ViewDB
{
    public class UserDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new UserInfo();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    UserInfo s = (UserInfo)entity;
                    s.Username = reader["username"].ToString();
                    s.Password = reader["password"].ToString(); // Still store hash in object

                    try
                    {
                        s.Rewiew = reader["Rewiew"].ToString();
                    }
                    catch { }

                    try
                    {
                        s.Rating = (double)reader["Rating"];
                    }
                    catch { }

                    s.Confirmed = bool.Parse(reader["Confirmed"].ToString());
                    s.Email = reader["email"].ToString();
                    s.Phone = reader["phone"].ToString();
                    s.TeacherId = (int)reader["TeacherId"];

                    if (s.TeacherId != 0)
                        s.StudentId = (int)reader["id"];

                    s.LessonPrice = (int)reader["lessonPrice"];
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// SECURE: Get user by ID with parameterized query
        /// </summary>
        public UserInfo GetUserById(int id, string table)
        {
            // Validate table name (whitelist approach)
            if (table != "Student" && table != "Teacher")
            {
                throw new ArgumentException("Invalid table name");
            }

            string sqlStr = $"SELECT * FROM [{table}] WHERE id = ?";
            var param = new OleDbParameter("@id", id);

            List<UserInfo> list = Select(sqlStr, param).OfType<UserInfo>().ToList();

            if (list.Count == 1)
                return list[0];

            return null;
        }

        public AllUsers GetAllStudents()
        {
            List<Base> list = Select("SELECT * FROM [Student]");
            return new AllUsers(list);
        }

        public AllUsers GetAllTeacher()
        {
            List<Base> list = Select("SELECT * FROM [Teacher]");
            return new AllUsers(list);
        }

        /// <summary>
        /// SECURE: Add user with hashed password
        /// </summary>
        public bool AddUser(UserInfo user)
        {
            // Validate inputs
            if (!SecurityHelper.IsSafeString(user.Username, 50))
                return false;

            if (!SecurityHelper.IsSafeString(user.Email, 100))
                return false;

            // Hash the password
            string hashedPassword = SecurityHelper.HashPassword(user.Password);

            string sqlstr = "INSERT INTO [Teacher] ([username], [password], [email], [phone], [Rating]) " +
                           "VALUES (?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@username", user.Username),
                new OleDbParameter("@password", hashedPassword),
                new OleDbParameter("@email", user.Email),
                new OleDbParameter("@phone", user.Phone),
                new OleDbParameter("@rating", user.Rating)
            };

            return SaveChanges(sqlstr, parameters) != 0;
        }

        /// <summary>
        /// SECURE: Add student with hashed password
        /// </summary>
        public bool AddStudent(UserInfo user)
        {
            // Validate inputs
            if (!SecurityHelper.IsSafeString(user.Username, 50))
                return false;

            if (!SecurityHelper.IsSafeString(user.Email, 100))
                return false;

            // Hash the password
            string hashedPassword = SecurityHelper.HashPassword(user.Password);

            string sqlstr = "INSERT INTO [Student] ([username], [password], [email], [phone], [teacherId], [Confirmed]) " +
                           "VALUES (?, ?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@username", user.Username),
                new OleDbParameter("@password", hashedPassword),
                new OleDbParameter("@email", user.Email),
                new OleDbParameter("@phone", user.Phone),
                new OleDbParameter("@teacherId", user.TeacherId),
                new OleDbParameter("@confirmed", false)
            };

            return SaveChanges(sqlstr, parameters) != 0;
        }

        /// <summary>
        /// SECURE: Verify password with hash comparison
        /// </summary>
        public bool VerifyUserPassword(string username, string password)
        {
            // Validate inputs
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            // Try student table first
            string sqlStr = "SELECT [password] FROM [Student] WHERE [username] = ?";
            var param = new OleDbParameter("@username", username);

            object hashedPassword = SelectScalar(sqlStr, param);

            // If not found in Student, try Teacher
            if (hashedPassword == null)
            {
                sqlStr = "SELECT [password] FROM [Teacher] WHERE [username] = ?";
                hashedPassword = SelectScalar(sqlStr, param);
            }

            if (hashedPassword == null)
                return false;

            // Verify password against hash
            return SecurityHelper.VerifyPassword(password, hashedPassword.ToString());
        }

        /// <summary>
        /// SECURE: Update rating with parameterized query
        /// </summary>
        public void UpdateRating(int tid, int rating, string review)
        {
            // Validate rating
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            // Get current average
            string sqlStr = "SELECT AVG(Rating) FROM [Ratings] WHERE TeacherID = ?";
            var param = new OleDbParameter("@tid", tid);
            object avgObj = SelectScalar(sqlStr, param);
            double avg = avgObj != DBNull.Value && avgObj != null ? Convert.ToDouble(avgObj) : 0;

            // Get current count
            sqlStr = "SELECT COUNT(*) FROM [Ratings] WHERE TeacherID = ?";
            object countObj = SelectScalar(sqlStr, param);
            int count = countObj != DBNull.Value && countObj != null ? Convert.ToInt32(countObj) : 0;

            // Calculate new average
            double newAvg = (avg * count + rating) / (count + 1);

            // Update teacher rating
            sqlStr = "UPDATE [Teacher] SET Rating = ? WHERE id = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@rating", newAvg),
                new OleDbParameter("@tid", tid));

            // Insert new rating
            sqlStr = "INSERT INTO [Ratings] (teacherID, rating, rewiew) VALUES (?, ?, ?)";
            SaveChanges(sqlStr,
                new OleDbParameter("@tid", tid),
                new OleDbParameter("@rating", rating),
                new OleDbParameter("@review", review));
        }

        public void UpdateTeacherId(int sid, int tid)
        {
            string sqlStr = "UPDATE [Student] SET teacherId = ? WHERE id = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@tid", tid),
                new OleDbParameter("@sid", sid));
        }

        public List<string> GetTeacherReviews(int tid)
        {
            string sqlStr = "SELECT [Rewiew] FROM [Ratings] WHERE teacherID = ?";
            return SelectReview(sqlStr, new OleDbParameter("@tid", tid));
        }

        public void TeacherConfirm(int sid, int tid)
        {
            string sqlStr = "UPDATE [Student] SET Confirmed = 1 WHERE id = ? AND teacherId = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@sid", sid),
                new OleDbParameter("@tid", tid));
        }

        public List<UserInfo> GetTeacherStudents(int tid)
        {
            string sqlStr = "SELECT * FROM [Student] WHERE teacherId = ?";
            return Select(sqlStr, new OleDbParameter("@tid", tid))
                .OfType<UserInfo>()
                .ToList();
        }

        public bool IsConfirmed(int id)
        {
            string sqlStr = "SELECT * FROM [Student] WHERE id = ?";
            List<UserInfo> list = Select(sqlStr, new OleDbParameter("@id", id))
                .OfType<UserInfo>()
                .ToList();

            if (list.Count == 1)
                return list[0].Confirmed;

            return false;
        }

        public int GetUserID(string username, string table)
        {
            // Validate table name
            if (table != "Student" && table != "Teacher")
                throw new ArgumentException("Invalid table name");

            if (!SecurityHelper.IsSafeString(username, 50))
                return -1;

            string sqlStr = $"SELECT id FROM [{table}] WHERE username = ?";
            List<Base> list = Select(sqlStr, new OleDbParameter("@username", username));

            if (list.Count == 1)
                return list[0].Id;

            return -1;
        }

        public int GetTeacherId(int sid)
        {
            string sql = "SELECT * FROM [Student] WHERE id = ?";
            List<UserInfo> list = Select(sql, new OleDbParameter("@sid", sid))
                .OfType<UserInfo>()
                .ToList();

            if (list.Count == 1)
                return list[0].TeacherId;

            return 0;
        }

        public void AddLessonToStudent(int id, string less)
        {
            string all = GetStudentLessons(id) + less;
            string sql = "UPDATE [Student] SET [lessons] = ? WHERE id = ?";
            SaveChanges(sql,
                new OleDbParameter("@lessons", "," + all + ","),
                new OleDbParameter("@id", id));
        }

        public string GetStudentLessons(int id)
        {
            string sql = "SELECT * FROM [Student] WHERE id = ?";
            List<UserInfo> list = Select(sql, new OleDbParameter("@id", id))
                .OfType<UserInfo>()
                .ToList();

            if (list.Count == 1)
                return list[0].Lessons ?? "";

            return "";
        }
        public void MigrateAllPasswords()
        {
            string[] tables = { "Student", "Teacher" };

            foreach (string table in tables)
            {
                List<UserInfo> users = Select($"SELECT * FROM [{table}]")
                                       .OfType<UserInfo>()
                                       .ToList();

                foreach (UserInfo user in users)
                {
                    // Skip already-hashed passwords (prevents double hashing)
                    if (string.IsNullOrEmpty(user.Password) || user.Password.Length > 50)
                        continue;

                    string hashed = SecurityHelper.HashPassword(user.Password);

                    SaveChanges(
                        $"UPDATE [{table}] SET [password] = ? WHERE id = ?",
                        new OleDbParameter("@password", hashed),
                        new OleDbParameter("@id", user.Id)
                    );
                }
            }
        }

    }

}