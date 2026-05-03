using Model;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;

namespace ViewDB
{
    [System.Runtime.Serialization.DataContract]
    public class Payment : Base
    {
        [System.Runtime.Serialization.DataMember]
        public int PaymentID { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int StudentID { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int TeacherID { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int Amount { get; set; }
        [System.Runtime.Serialization.DataMember]
        public DateTime PaymentDate { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string PaymentMethod { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int NumberOfPayments { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int ParcialAmount { get; set; }
        [System.Runtime.Serialization.DataMember]
        public bool paid { get; set; }
        [System.Runtime.Serialization.DataMember]
        public int LessonId { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Status { get; set; }
        [System.Runtime.Serialization.DataMember]
        public string Notes { get; set; }
    }

    public class PaymentDB : BaseDB
    {
        protected override Base NewEntity()
        {
            return new Payment();
        }

        protected override void CreateModel(Base entity)
        {
            base.CreateModel(entity);
            if (entity != null)
            {
                try
                {
                    Payment s = (Payment)entity;
                    s.PaymentID = int.Parse(reader["PaymentID"].ToString());
                    s.StudentID = int.Parse(reader["StudentID"].ToString());
                    s.TeacherID = int.Parse(reader["TeacherID"].ToString());
                    s.Amount = int.Parse(reader["Amount"].ToString());
                    s.PaymentDate = DateTime.Parse(reader["PaymentDate"].ToString());
                    s.PaymentMethod = reader["PaymentMethod"].ToString();
                    s.NumberOfPayments = int.Parse(reader["NumberOfPayments"].ToString());
                    s.paid = bool.Parse(reader["paid"].ToString());

                    try { s.LessonId = int.Parse(reader["LessonId"].ToString()); } catch { s.LessonId = 0; }
                    try { s.Status = reader["Status"].ToString(); } catch { s.Status = s.paid ? "Paid" : "Pending"; }
                    try { s.Notes = reader["Notes"].ToString(); } catch { s.Notes = ""; }
                    try { s.ParcialAmount = int.Parse(reader["ParcialAmount"].ToString()); } catch { s.ParcialAmount = 0; }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("PaymentDB CreateModel Error: " + ex.Message);
                }
            }
        }

        private int GetNextPaymentId()
        {
            object result = SelectScalar("SELECT MAX(PaymentID) FROM [Payments]");
            return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) + 1 : 1;
        }

        public List<Payment> SelectPaymentByStudentID(int id)
        {
            return Select("SELECT * FROM [Payments] WHERE StudentID = ?",
                new OleDbParameter("@studentId", id)).OfType<Payment>().ToList();
        }

        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            return Select("SELECT * FROM [Payments] WHERE TeacherID = ?",
                new OleDbParameter("@teacherId", id)).OfType<Payment>().ToList();
        }

        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            return Select("SELECT * FROM [Payments] WHERE PaymentID = ?",
                new OleDbParameter("@paymentId", id)).OfType<Payment>().ToList();
        }

        public List<Payment> GetPaymentsByLessonId(int lessonId)
        {
            return Select("SELECT * FROM [Payments] WHERE LessonId = ?",
                new OleDbParameter("@lessonId", lessonId)).OfType<Payment>().ToList();
        }

        /// <summary>
        /// FIXED: Process payment - updates Lessons table AND inserts payment
        /// </summary>
        public void Pay(Payment payment)
        {
            // Calculate partial amount
            if (payment.NumberOfPayments > 0)
                payment.ParcialAmount = payment.Amount / payment.NumberOfPayments;
            else
            {
                payment.ParcialAmount = payment.Amount;
                payment.NumberOfPayments = 1;
            }

            // STEP 1: Mark the lesson as paid FIRST
            if (payment.paid && payment.LessonId > 0)
            {
                int lessonUpdated = SaveChanges("UPDATE [Lessons] SET [Paid] = ? WHERE [LessonID] = ?",
                    new OleDbParameter("@paid", true),
                    new OleDbParameter("@lessonId", payment.LessonId));

                System.Diagnostics.Debug.WriteLine($"Lesson {payment.LessonId} paid update: {lessonUpdated} rows");
            }

            // STEP 2: Insert payment record
            int nextId = GetNextPaymentId();

            SaveChanges(@"INSERT INTO [Payments] 
                ([PaymentID], [StudentID], [TeacherID], [Amount], [PaymentDate], [PaymentMethod], 
                 [NumberOfPayments], [paid], [ParcialAmount], [LessonId], [Status], [Notes])
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                new OleDbParameter("@paymentId", nextId),
                new OleDbParameter("@studentId", payment.StudentID),
                new OleDbParameter("@teacherId", payment.TeacherID),
                new OleDbParameter("@amount", payment.Amount),
                new OleDbParameter("@paymentDate", payment.PaymentDate),
                new OleDbParameter("@paymentMethod", payment.PaymentMethod ?? "Cash"),
                new OleDbParameter("@numberOfPayments", payment.NumberOfPayments),
                new OleDbParameter("@paid", payment.paid),
                new OleDbParameter("@parcialAmount", payment.ParcialAmount),
                new OleDbParameter("@lessonId", payment.LessonId),
                new OleDbParameter("@status", payment.Status ?? (payment.paid ? "Paid" : "Pending")),
                new OleDbParameter("@notes", payment.Notes ?? ""));
        }

        public void UpdatePayment(Payment payment)
        {
            SaveChanges("UPDATE [Payments] SET [paid] = ? WHERE [PaymentID] = ?",
                new OleDbParameter("@paid", payment.paid),
                new OleDbParameter("@paymentId", payment.PaymentID));
        }

        public bool CheckPaid(int id)
        {
            var list = Select("SELECT * FROM [Payments] WHERE PaymentID = ?",
                new OleDbParameter("@paymentId", id)).OfType<Payment>().ToList();
            return list.Count > 0 && list[0].paid;
        }

        public decimal GetTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            object result = SelectScalar(@"SELECT SUM(Amount) FROM [Payments] 
                WHERE TeacherID = ? AND paid = ? AND PaymentDate >= ? AND PaymentDate <= ?",
                new OleDbParameter("@teacherId", teacherId),
                new OleDbParameter("@paid", true),
                new OleDbParameter("@fromDate", fromDate),
                new OleDbParameter("@toDate", toDate));

            return (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
        }

        public List<Payment> GetOutstandingPayments(int studentId)
        {
            return Select("SELECT * FROM [Payments] WHERE StudentID = ? AND paid = ? ORDER BY PaymentDate",
                new OleDbParameter("@studentId", studentId),
                new OleDbParameter("@paid", false)).OfType<Payment>().ToList();
        }

        public List<Payment> GetOverduePayments()
        {
            return Select("SELECT * FROM [Payments] WHERE paid = ? AND PaymentDate < ? ORDER BY PaymentDate",
                new OleDbParameter("@paid", false),
                new OleDbParameter("@today", DateTime.Today)).OfType<Payment>().ToList();
        }

        public void MarkAsOverdue(int paymentId)
        {
            SaveChanges("UPDATE [Payments] SET [Status] = ? WHERE [PaymentID] = ?",
                new OleDbParameter("@status", "Overdue"),
                new OleDbParameter("@paymentId", paymentId));
        }

        /// <summary>
        /// Verified teacher income that ALSO confirms each payment is linked to a real,
        /// non-cancelled lesson. INNER JOIN Payments + Lessons + aggregation.
        /// </summary>
        public decimal GetVerifiedTeacherIncome(int teacherId, DateTime fromDate, DateTime toDate)
        {
            string sql = @"SELECT SUM(P.[Amount])
                           FROM [Payments] AS P
                           INNER JOIN [Lessons] AS L ON P.[LessonId] = L.[LessonID]
                           WHERE P.[TeacherID] = ?
                             AND P.[paid] = TRUE
                             AND L.[Canceled] = 0
                             AND P.[PaymentDate] >= ?
                             AND P.[PaymentDate] <= ?";
            object result = SelectScalar(sql,
                new OleDbParameter("@teacherId", teacherId),
                new OleDbParameter("@fromDate", fromDate),
                new OleDbParameter("@toDate", toDate));
            return (result != null && result != DBNull.Value) ? Convert.ToDecimal(result) : 0m;
        }

        /// <summary>
        /// Income breakdown per student for a teacher in the given date range.
        /// JOIN Payments + Student + GROUP BY + SUM.
        /// Returned as a Dictionary keyed by username so callers do not need a new
        /// DataContract type.
        /// </summary>
        public Dictionary<string, decimal> GetTeacherIncomeByStudent(int teacherId, DateTime fromDate, DateTime toDate)
        {
            var result = new Dictionary<string, decimal>();
            string sql = @"SELECT S.[username], SUM(P.[Amount]) AS Total
                           FROM [Payments] AS P
                           INNER JOIN [Student] AS S ON P.[StudentID] = S.[id]
                           WHERE P.[TeacherID] = ?
                             AND P.[paid] = TRUE
                             AND P.[PaymentDate] >= ?
                             AND P.[PaymentDate] <= ?
                           GROUP BY S.[username]
                           ORDER BY SUM(P.[Amount]) DESC";

            try
            {
                connection.Open();
                command.CommandText = sql;
                command.Parameters.Clear();
                command.Parameters.Add(new OleDbParameter("@teacherId", teacherId));
                command.Parameters.Add(new OleDbParameter("@fromDate", fromDate));
                command.Parameters.Add(new OleDbParameter("@toDate", toDate));
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader["username"].ToString();
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    result[name] = total;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetTeacherIncomeByStudent Error: " + ex.Message);
            }
            finally
            {
                if (reader != null) reader.Close();
                if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            }
            return result;
        }
    }
}