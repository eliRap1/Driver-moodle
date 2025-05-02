using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace driver_client
{
    public class AgeRangeRule : ValidationRule
    {

        // validation check
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int age = 0;
            int min = 1, max = 99;
            try
            {
                if (((string)value).Length > 0)
                    age = Int32.Parse((String)value);
            }
            // check if all digits
            catch (Exception e)
            {
                return new ValidationResult(false, "Illegal characters or " + e.Message);
            }

            //  check age range
            if ((age < min) || (age > max))
            {
                return new ValidationResult(false,
                  "Please enter an age in the range: " + min + " - " + max + ".");
            }
            else
            {
                return ValidationResult.ValidResult;
            }
        }
    }





    public class EmailRule : ValidationRule
    {

        // validation check
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Regular Expressions Quick Start
            // https://en.wikipedia.org/wiki/Regular_expression
            // https://www.regular-expressions.info/quickstart.html
            //  Exampels:   https://www.regular-expressions.info/examples.html
            //              https://code.tutsplus.com/tutorials/8-regular-expressions-you-should-know--net-6149
            string email = (string)value;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (match.Success)
                return ValidationResult.ValidResult;  // correct
            else
                return new ValidationResult(false,
                          "Please enter a legal Email.");  // is incorrect
        }
    }
    public class PhoneRule : ValidationRule
    {

        // validation check
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string phone = (string)value;
            //
            if (Regex.Match(phone, "\\^?[1-9][0-9]{8}$").Success)
                return ValidationResult.ValidResult;  // correct
            else
                return new ValidationResult(false,
                          "Please enter a legal phone.");  // is incorrect
        }
    }
    public class isAdminRule : ValidationRule
    {

        // validation check
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string admin = (string)value;
            //
            if (admin == "Student" || admin == "Teacher")
                return ValidationResult.ValidResult;  // correct
            else
                return new ValidationResult(false,
                          "Please select a role");  // is incorrect
        }
    }
    public class MinLenth : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = (string)value;
            if (str.Length >= 4)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                //username_border.BorderThickness = new Thickness(2);
                //pass_border.BorderThickness = new Thickness(2);
                //MessageBox.Show("Password and Username must be at least 4 characters long", "Incorrect password/username length", MessageBoxButton.OK, MessageBoxImage.Error);
                return new ValidationResult(false,
                          "Password and Username must be at least 4 characters long\", \"Incorrect password/username length");
            }
        }
    }


}
