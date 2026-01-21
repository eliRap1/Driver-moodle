using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [DataContract]
    public class UserInfo : Base
    {
        private string username;
        private string password;
        private string email;
        private string phone;
        private bool isAdmin;
        private int teacherId;
        private double rating;
        private string ratingT;
        private int lessonPrice;
        private string paymentMethods;

        [DataMember]
        public bool IsAdmin { get => isAdmin; set => isAdmin = value; }

        [DataMember]
        public string Username { get => username; set => username = value; }

        [DataMember]
        public string Password { get => password; set => password = value; }

        [DataMember]
        public string Email { get => email; set => email = value; }

        [DataMember]
        public string Phone { get => phone; set => phone = value; }

        [DataMember]
        public int TeacherId { get => teacherId; set => teacherId = value; }

        [DataMember]
        public int StudentId { get; set; }

        [DataMember]
        public double Rating { get => rating; set => rating = value; }

        [DataMember]
        public string RatingText { get => ratingT; set => ratingT = value; }

        [DataMember]
        public string Rewiew { get; set; }

        [DataMember]
        public bool Confirmed { get; set; }

        [DataMember]
        public string Lessons { get; set; }

        [DataMember]
        public int LessonPrice { get => lessonPrice; set => lessonPrice = value; }

        /// <summary>
        /// Comma-separated list of accepted payment methods (for teachers)
        /// Example: "Cash,Credit Card,Bank Transfer,Bit"
        /// </summary>
        [DataMember]
        public string PaymentMethods { get => paymentMethods; set => paymentMethods = value; }

        /// <summary>
        /// Custom lesson price for this student (0 = use teacher's default)
        /// </summary>
        [DataMember]
        public int CustomLessonPrice { get; set; }

        /// <summary>
        /// Discount percentage for this student (0-100)
        /// </summary>
        [DataMember]
        public int DiscountPercent { get; set; }

        public UserInfo()
        {
            LessonPrice = 200; // Default
            PaymentMethods = "Cash,Credit Card,Bank Transfer";
            DiscountPercent = 0;
            CustomLessonPrice = 0;
        }

        /// <summary>
        /// Gets the effective price considering discounts
        /// </summary>
        public int GetEffectivePrice(int basePrice)
        {
            if (CustomLessonPrice > 0)
                return CustomLessonPrice;

            if (DiscountPercent > 0)
                return basePrice - (basePrice * DiscountPercent / 100);

            return basePrice;
        }
    }
}