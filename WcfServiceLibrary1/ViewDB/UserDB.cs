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
                    s.Password = reader["password"].ToString();

                    try { s.Rewiew = reader["Rewiew"].ToString(); }
                    catch { }

                    try { s.Rating = (double)reader["Rating"]; }
                    catch { s.Rating = 0; }

                    try { s.Confirmed = bool.Parse(reader["Confirmed"].ToString()); }
                    catch { s.Confirmed = true; } // Teachers are auto-confirmed

                    s.Email = reader["email"].ToString();
                    s.Phone = reader["phone"].ToString();

                    try { s.TeacherId = (int)reader["TeacherId"]; }
                    catch { s.TeacherId = 0; }

                    if (s.TeacherId != 0)
                        s.StudentId = (int)reader["id"];

                    try { s.LessonPrice = (int)reader["lessonPrice"]; }
                    catch { s.LessonPrice = 200; } // Default price

                    // Check for IsAdmin field (for teachers who are also admins)
                    try { s.IsAdmin = bool.Parse(reader["IsAdmin"].ToString()); }
                    catch { s.IsAdmin = false; }

                    // Payment methods for teachers
                    try { s.PaymentMethods = reader["PaymentMethods"].ToString(); }
                    catch { s.PaymentMethods = "Cash,Credit Card,Bank Transfer"; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("UserDB CreateModel Error: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Get user by ID with parameterized query
        /// </summary>
        public UserInfo GetUserById(int id, string table)
        {
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
        /// Add teacher with lesson price and admin flag
        /// </summary>
        public bool AddUser(UserInfo user)
        {
            if (!SecurityHelper.IsSafeString(user.Username, 50))
                return false;

            if (!SecurityHelper.IsSafeString(user.Email, 100))
                return false;

            string hashedPassword = SecurityHelper.HashPassword(user.Password);

            string sqlstr = @"INSERT INTO [Teacher] 
                ([username], [password], [email], [phone], [Rating], [lessonPrice], [IsAdmin], [PaymentMethods], [Confirmed]) 
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@username", user.Username),
                new OleDbParameter("@password", hashedPassword),
                new OleDbParameter("@email", user.Email),
                new OleDbParameter("@phone", user.Phone),
                new OleDbParameter("@rating", user.Rating),
                new OleDbParameter("@lessonPrice", user.LessonPrice > 0 ? user.LessonPrice : 200),
                new OleDbParameter("@isAdmin", user.IsAdmin),
                new OleDbParameter("@paymentMethods", user.PaymentMethods ?? "Cash,Credit Card,Bank Transfer"),
                new OleDbParameter("@confirmed", true)
            };

            return SaveChanges(sqlstr, parameters) != 0;
        }

        /// <summary>
        /// Add student with hashed password
        /// </summary>
        public bool AddStudent(UserInfo user)
        {
            if (!SecurityHelper.IsSafeString(user.Username, 50))
                return false;

            if (!SecurityHelper.IsSafeString(user.Email, 100))
                return false;

            string hashedPassword = SecurityHelper.HashPassword(user.Password);

            string sqlstr = @"INSERT INTO [Student] 
                ([username], [password], [email], [phone], [teacherId], [Confirmed], [lessonPrice]) 
                VALUES (?, ?, ?, ?, ?, ?, ?)";

            var parameters = new[]
            {
                new OleDbParameter("@username", user.Username),
                new OleDbParameter("@password", hashedPassword),
                new OleDbParameter("@email", user.Email),
                new OleDbParameter("@phone", user.Phone),
                new OleDbParameter("@teacherId", user.TeacherId),
                new OleDbParameter("@confirmed", false),
                new OleDbParameter("@lessonPrice", 0) // 0 means use teacher's default price
            };

            return SaveChanges(sqlstr, parameters) != 0;
        }

        /// <summary>
        /// Verify password with hash comparison
        /// </summary>
        public bool VerifyUserPassword(string username, string password)
        {
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

            return SecurityHelper.VerifyPassword(password, hashedPassword.ToString());
        }

        /// <summary>
        /// Check if user is an admin
        /// </summary>
        public bool IsUserAdmin(string username)
        {
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            string sql = "SELECT [IsAdmin] FROM [Teacher] WHERE [username] = ?";
            object result = SelectScalar(sql, new OleDbParameter("@username", username));

            if (result != null && result != DBNull.Value)
            {
                return bool.Parse(result.ToString());
            }

            return false;
        }

        /// <summary>
        /// Set admin status for a teacher
        /// </summary>
        public void SetAdminStatus(int teacherId, bool isAdmin)
        {
            string sql = "UPDATE [Teacher] SET [IsAdmin] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@isAdmin", isAdmin),
                new OleDbParameter("@id", teacherId));
        }

        /// <summary>
        /// Update teacher's lesson price
        /// </summary>
        public void UpdateLessonPrice(int teacherId, int price)
        {
            string sql = "UPDATE [Teacher] SET [lessonPrice] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@price", price),
                new OleDbParameter("@id", teacherId));
        }

        /// <summary>
        /// Set custom lesson price for a specific student
        /// </summary>
        public void SetStudentLessonPrice(int studentId, int price)
        {
            string sql = "UPDATE [Student] SET [lessonPrice] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@price", price),
                new OleDbParameter("@id", studentId));
        }

        /// <summary>
        /// Get the effective lesson price for a student (custom or teacher's default)
        /// </summary>
        public int GetStudentLessonPrice(int studentId)
        {
            // First check if student has a custom price
            string sql = "SELECT [lessonPrice] FROM [Student] WHERE [id] = ?";
            object result = SelectScalar(sql, new OleDbParameter("@id", studentId));

            if (result != null && result != DBNull.Value)
            {
                int customPrice = Convert.ToInt32(result);
                if (customPrice > 0)
                    return customPrice;
            }

            // Otherwise, get the teacher's default price
            int teacherId = GetTeacherId(studentId);
            if (teacherId > 0)
            {
                sql = "SELECT [lessonPrice] FROM [Teacher] WHERE [id] = ?";
                result = SelectScalar(sql, new OleDbParameter("@id", teacherId));

                if (result != null && result != DBNull.Value)
                    return Convert.ToInt32(result);
            }

            return 200; // Default fallback
        }

        /// <summary>
        /// Update teacher's payment methods
        /// </summary>
        public void UpdatePaymentMethods(int teacherId, string paymentMethods)
        {
            string sql = "UPDATE [Teacher] SET [PaymentMethods] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@methods", paymentMethods),
                new OleDbParameter("@id", teacherId));
        }

        /// <summary>
        /// Update student's assigned teacher
        /// </summary>
        public void UpdateStudentTeacher(int studentId, int newTeacherId)
        {
            string sql = "UPDATE [Student] SET [teacherId] = ?, [Confirmed] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@teacherId", newTeacherId),
                new OleDbParameter("@confirmed", false), // Needs re-confirmation
                new OleDbParameter("@id", studentId));
        }

        /// <summary>
        /// Update student credentials (admin function)
        /// </summary>
        public void UpdateStudentCredentials(int studentId, string email, string phone, int teacherId)
        {
            string sql = "UPDATE [Student] SET [email] = ?, [phone] = ?, [teacherId] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@email", email),
                new OleDbParameter("@phone", phone),
                new OleDbParameter("@teacherId", teacherId),
                new OleDbParameter("@id", studentId));
        }

        /// <summary>
        /// Reset user password (admin function)
        /// </summary>
        public void ResetPassword(int userId, string table, string newPassword)
        {
            if (table != "Student" && table != "Teacher")
                throw new ArgumentException("Invalid table name");

            string hashedPassword = SecurityHelper.HashPassword(newPassword);
            string sql = $"UPDATE [{table}] SET [password] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@password", hashedPassword),
                new OleDbParameter("@id", userId));
        }

        /// <summary>
        /// Update rating with review
        /// </summary>
        public void UpdateRating(int tid, int rating, string review)
        {
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
            sqlStr = "UPDATE [Teacher] SET [Rating] = ? WHERE [id] = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@rating", newAvg),
                new OleDbParameter("@tid", tid));

            // Insert new rating
            sqlStr = "INSERT INTO [Ratings] ([teacherID], [rating], [rewiew]) VALUES (?, ?, ?)";
            SaveChanges(sqlStr,
                new OleDbParameter("@tid", tid),
                new OleDbParameter("@rating", rating),
                new OleDbParameter("@review", review));
        }

        public void UpdateTeacherId(int sid, int tid)
        {
            string sqlStr = "UPDATE [Student] SET [teacherId] = ? WHERE [id] = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@tid", tid),
                new OleDbParameter("@sid", sid));
        }

        public List<string> GetTeacherReviews(int tid)
        {
            string sqlStr = "SELECT [Rewiew] FROM [Ratings] WHERE [teacherID] = ?";
            return SelectReview(sqlStr, new OleDbParameter("@tid", tid));
        }

        public void TeacherConfirm(int sid, int tid)
        {
            string sqlStr = "UPDATE [Student] SET [Confirmed] = ? WHERE [id] = ? AND [teacherId] = ?";
            SaveChanges(sqlStr,
                new OleDbParameter("@confirmed", true),
                new OleDbParameter("@sid", sid),
                new OleDbParameter("@tid", tid));
        }

        public List<UserInfo> GetTeacherStudents(int tid)
        {
            string sqlStr = "SELECT * FROM [Student] WHERE [teacherId] = ?";
            return Select(sqlStr, new OleDbParameter("@tid", tid))
                .OfType<UserInfo>()
                .ToList();
        }

        public bool IsConfirmed(int id)
        {
            string sqlStr = "SELECT * FROM [Student] WHERE [id] = ?";
            List<UserInfo> list = Select(sqlStr, new OleDbParameter("@id", id))
                .OfType<UserInfo>()
                .ToList();

            if (list.Count == 1)
                return list[0].Confirmed;

            return false;
        }

        public int GetUserID(string username, string table)
        {
            if (table != "Student" && table != "Teacher")
                throw new ArgumentException("Invalid table name");

            if (!SecurityHelper.IsSafeString(username, 50))
                return -1;

            string sqlStr = $"SELECT [id] FROM [{table}] WHERE [username] = ?";
            List<Base> list = Select(sqlStr, new OleDbParameter("@username", username));

            if (list.Count == 1)
                return list[0].Id;

            return -1;
        }

        public int GetTeacherId(int sid)
        {
            string sql = "SELECT * FROM [Student] WHERE [id] = ?";
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
            string sql = "UPDATE [Student] SET [lessons] = ? WHERE [id] = ?";
            SaveChanges(sql,
                new OleDbParameter("@lessons", "," + all + ","),
                new OleDbParameter("@id", id));
        }

        public string GetStudentLessons(int id)
        {
            string sql = "SELECT * FROM [Student] WHERE [id] = ?";
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
                    if (string.IsNullOrEmpty(user.Password) || user.Password.Length > 50)
                        continue;

                    string hashed = SecurityHelper.HashPassword(user.Password);

                    SaveChanges(
                        $"UPDATE [{table}] SET [password] = ? WHERE [id] = ?",
                        new OleDbParameter("@password", hashed),
                        new OleDbParameter("@id", user.Id)
                    );
                }
            }
        }
    }
}