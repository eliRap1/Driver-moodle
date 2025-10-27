using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewDB
{
    public class Payment : Base
    {
        public int PaymentID { get; set; }
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public int Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public int NumberOfPayments { get; set; }
        public int ParcialAmount { get; set; }
        public bool paid { get; set; }
    }

    public class PaymentDB : BaseDB
    {
        public PaymentDB() { }
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

                }
                catch
                {
                    Console.WriteLine("No ID in DB");
                }
            }
        }
        public List<Payment> SelectPaymentByStudentID(int id)
        {
            string sqlStr = "Select * From [Payments] Where StudentID=" + id;
            List<Payment> list = Select(sqlStr).OfType<Payment>().ToList();
            if (list.Count > 0) { return list; }
            return new List<Payment>();
        }
        public List<Payment> SelectPaymentByTeacherID(int id)
        {
            string sqlStr = "Select * From [Payments] Where TeacherID=" + id;
            List<Payment> list = Select(sqlStr).OfType<Payment>().ToList();
            if (list.Count > 0) { return list; }
            return new List<Payment>();
        }
        public List<Payment> SelectPaymentByPaymentID(int id)
        {
            string sqlStr = "Select * From [Payments] Where PaymentID=" + id;
            List<Payment> list = Select(sqlStr).OfType<Payment>().ToList();
            if (list.Count > 0) { return list; }
            return new List<Payment>();
        }
        public void PaymentUpdate(Payment payment)
        {
            string sqlStr = "Update Payments Set paid=" + payment.paid + " Where PaymentID=" + payment.PaymentID;
            SaveChanges(sqlStr);
        }
        public void Pay(Payment payment)
        {
            if(payment.NumberOfPayments > 0)
            { 
                payment.ParcialAmount = payment.Amount/payment.NumberOfPayments; 
            }
            string sql = "Insert into [Payments]"
                + "(StudentID,TeacherID,Amount,PaymentDate,PaymentMethod,NumberOfPayments,paid,ParcialAmount)"
                + "Values("
                + payment.StudentID + ","
                + payment.TeacherID + ","
                + payment.Amount + ","
                + "'" + payment.PaymentDate + "',"
                + "'" + payment.PaymentMethod + "',"
                + payment.NumberOfPayments + ","
                + payment.paid + "," + payment.ParcialAmount+")";

        }
        public bool CheckPaid(int id)
        {
            string sqlStr = "Select * From Payments Where PaymentID=" + id;
            List<Payment> list = Select(sqlStr).OfType<Payment>().ToList();
            if (list.Count > 0) { return list[0].paid; }
            return false;
        }
    }
}
