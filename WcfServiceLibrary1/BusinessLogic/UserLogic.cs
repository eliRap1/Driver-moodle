using Model;
using Model.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using ViewDB;

namespace BusinessLogic
{
    /// <summary>
    /// Business logic for users, teachers, students: registration, authentication,
    /// confirmation, admin status, pricing and discounts, credentials.
    /// Service1 only exposes these as WCF endpoints; the rules live here.
    /// </summary>
    public static class UserLogic
    {
        // ==================== USER OPERATIONS ====================

        public static bool AddUser(string name, string password, string email, string phone, bool admin, int tID, int lessonPrice = 200)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== AddUser called ===");
                System.Diagnostics.Debug.WriteLine($"Username: {name}, Email: {email}, IsTeacher: {admin}, TeacherId: {tID}, LessonPrice: {lessonPrice}");

                if (!SecurityHelper.IsSafeString(name, 50))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: Username failed safety check");
                    return false;
                }

                if (!SecurityHelper.IsSafeString(email, 100))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: Email failed safety check");
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: Password is empty");
                    return false;
                }

                if (CheckUserExist(name))
                {
                    System.Diagnostics.Debug.WriteLine("AddUser: User already exists");
                    return false;
                }

                UserInfo user = new UserInfo
                {
                    Username = name,
                    Password = password,
                    Email = email,
                    Phone = phone,
                    IsAdmin = admin,
                    TeacherId = tID,
                    LessonPrice = lessonPrice > 0 ? lessonPrice : 200
                };

                bool worked = false;

                if (admin)
                {
                    // Teacher registration
                    System.Diagnostics.Debug.WriteLine("AddUser: Registering as Teacher");
                    worked = new UserDB().AddUser(user);
                }
                else
                {
                    // Student registration
                    System.Diagnostics.Debug.WriteLine("AddUser: Registering as Student");
                    worked = new UserDB().AddStudent(user);
                    if (worked)
                    {
                        int sid = new UserDB().GetUserID(name, "Student");
                        // Note: original code mutated an in-memory pre-insert list here
                        // (allUsers.SetStudentId) which was a guaranteed no-op; removed.
                        System.Diagnostics.Debug.WriteLine($"AddUser: Student ID assigned: {sid}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"AddUser: Registration result = {worked}");
                return worked;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddUser Exception: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static bool CheckUserExist(string username)
        {
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            AllUsers allUsers = new UserDB().GetAllStudents();
            var allAdmins = new UserDB().GetAllTeacher();

            return allUsers.Any(x => x.Username == username) ||
                   allAdmins.Any(x => x.Username == username);
        }

        public static bool CheckUserPassword(string username, string password)
        {
            if (!SecurityHelper.IsSafeString(username, 50) ||
                string.IsNullOrEmpty(password))
            {
                return false;
            }

            return new UserDB().VerifyUserPassword(username, password);
        }

        public static UserInfo GetUserById(int id, string table)
        {
            return new UserDB().GetUserById(id, table);
        }

        public static bool CheckUserAdmin(string username)
        {
            if (!SecurityHelper.IsSafeString(username, 50))
                return false;

            var allAdmins = new UserDB().GetAllTeacher();
            return allAdmins.Any(x => x.Username == username);
        }

        public static bool IsUserAdmin(string username)
        {
            return new UserDB().IsUserAdmin(username);
        }

        public static AllUsers GetAllUsers()
        {
            return new UserDB().GetAllStudents();
        }

        public static AllUsers GetAllTeacher()
        {
            return new UserDB().GetAllTeacher();
        }

        public static int GetUserID(string username, string table)
        {
            return new UserDB().GetUserID(username, table);
        }

        public static void TeacherConfirm(int id, int tID)
        {
            new UserDB().TeacherConfirm(id, tID);
        }

        public static List<UserInfo> GetTeacherStudents(int tid)
        {
            return new UserDB().GetTeacherStudents(tid);
        }

        public static bool IsConfirmed(int id)
        {
            return new UserDB().IsConfirmed(id);
        }

        public static int GetTeacherId(int studentId)
        {
            return new UserDB().GetTeacherId(studentId);
        }

        public static void UpdateTeacherId(int sid, int tid)
        {
            new UserDB().UpdateTeacherId(sid, tid);
        }

        // ==================== ADMIN OPERATIONS ====================

        public static void SetAdminStatus(int teacherId, bool isAdmin)
        {
            new UserDB().SetAdminStatus(teacherId, isAdmin);
        }

        public static void ResetPassword(int userId, string table, string newPassword)
        {
            new UserDB().ResetPassword(userId, table, newPassword);
        }

        // ==================== PRICING OPERATIONS ====================

        public static void UpdateLessonPrice(int teacherId, int price)
        {
            try { new UserDB().UpdateLessonPrice(teacherId, price); }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "UpdateLessonPrice failed: " + ex.GetBaseException().Message);
            }
        }

        public static int GetStudentLessonPrice(int studentId)
        {
            return new UserDB().GetStudentLessonPrice(studentId);
        }

        public static void UpdatePaymentMethods(int teacherId, string paymentMethods)
        {
            try { new UserDB().UpdatePaymentMethods(teacherId, paymentMethods); }
            catch (Exception ex)
            {
                throw new System.ServiceModel.FaultException(
                    "UpdatePaymentMethods failed: " + ex.GetBaseException().Message);
            }
        }

        // ==================== RATING OPERATIONS ====================

        public static void UpdateRating(int tid, int rating, string rewiew)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            new UserDB().UpdateRating(tid, rating, rewiew);
        }

        public static List<string> GetTeacherReviews(int tid)
        {
            return new UserDB().GetTeacherReviews(tid);
        }

        // ==================== MIGRATION ====================

        public static void MigrateAllPasswords()
        {
            new UserDB().MigrateAllPasswords();
        }

        // ==================== STUDENT PRICING OPERATIONS ====================

        public static void SetStudentLessonPrice(int studentId, int price)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"SetStudentLessonPrice: StudentId={studentId}, Price={price}");
                new UserDB().SetStudentLessonPrice(studentId, price);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetStudentLessonPrice Error: {ex.Message}");
                throw new System.ServiceModel.FaultException(
                    "SetStudentLessonPrice failed: " + ex.GetBaseException().Message);
            }
        }

        public static void SetStudentDiscount(int studentId, int discountPercent)
        {
            try
            {
                if (discountPercent < 0 || discountPercent > 100)
                    throw new ArgumentException("Discount must be between 0 and 100");

                System.Diagnostics.Debug.WriteLine($"SetStudentDiscount: StudentId={studentId}, Discount={discountPercent}%");
                new UserDB().SetStudentDiscount(studentId, discountPercent);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SetStudentDiscount Error: {ex.Message}");
                throw new System.ServiceModel.FaultException(
                    "SetStudentDiscount failed: " + ex.GetBaseException().Message);
            }
        }

        public static int GetEffectiveLessonPrice(int studentId)
        {
            try
            {
                return new UserDB().GetStudentLessonPrice(studentId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetEffectiveLessonPrice Error: {ex.Message}");
                return 200; // Default fallback
            }
        }

        public static void UpdateStudentCredentials(int studentId, string email, string phone, int teacherId)
        {
            new UserDB().UpdateStudentCredentials(studentId, email, phone, teacherId);
        }

        public static void UpdateStudentTeacher(int studentId, int newTeacherId)
        {
            new UserDB().UpdateStudentTeacher(studentId, newTeacherId);
        }
    }
}
