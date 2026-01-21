using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace driver_client
{
    public partial class StudentPayment : Page
    {
        private List<Lessons> unpaidLessons = new List<Lessons>();
        private List<Lessons> selectedLessons = new List<Lessons>();
        private int lessonPrice = 200; // Default, will be fetched from teacher

        public StudentPayment()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var srv = new Service1Client();

                // Get lesson price from teacher
                int teacherId = srv.GetTeacherId(LogIn.sign.Id);
                var teacher = srv.GetUserById(teacherId, "Teacher");
                if (teacher != null && teacher.LessonPrice > 0)
                {
                    lessonPrice = teacher.LessonPrice;
                }

                // Get all student lessons
                var allLessons = srv.GetAllStudentLessons(LogIn.sign.Id).ToList();

                // Filter unpaid lessons
                unpaidLessons = allLessons
                    .Where(l => !l.paid && l.Canceled != 1)
                    .OrderBy(l => l.Date)
                    .ToList();

                LoadUnpaidLessons();
                LoadPaymentHistory();
                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUnpaidLessons()
        {
            UnpaidLessonsPanel.Children.Clear();

            if (unpaidLessons.Count == 0)
            {
                UnpaidLessonsPanel.Children.Add(new TextBlock
                {
                    Text = "🎉 No unpaid lessons!\nYou're all caught up.",
                    Foreground = Brushes.LightGreen,
                    FontSize = 16,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                });
                return;
            }

            foreach (var lesson in unpaidLessons)
            {
                UnpaidLessonsPanel.Children.Add(CreateLessonCard(lesson));
            }
        }

        private Border CreateLessonCard(Lessons lesson)
        {
            var checkBox = new CheckBox
            {
                Tag = lesson,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            checkBox.Checked += LessonCheckbox_Changed;
            checkBox.Unchecked += LessonCheckbox_Changed;

            // Check if already selected
            if (selectedLessons.Any(l => l.LessonId == lesson.LessonId))
            {
                checkBox.IsChecked = true;
            }

            var infoStack = new StackPanel();

            // Date and time
            DateTime lessonDate;
            string dateDisplay = lesson.Date;
            if (DateTime.TryParse(lesson.Date, out lessonDate))
            {
                dateDisplay = lessonDate.ToString("dd/MM/yyyy");
            }

            infoStack.Children.Add(new TextBlock
            {
                Text = $"📅 {dateDisplay} at {lesson.Time}",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            });

            // Lesson ID
            infoStack.Children.Add(new TextBlock
            {
                Text = $"Lesson #{lesson.LessonId}",
                FontSize = 11,
                Foreground = Brushes.Gray
            });

            // Price
            var priceText = new TextBlock
            {
                Text = $"{lessonPrice} ₪",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var mainStack = new StackPanel { Orientation = Orientation.Horizontal };
            mainStack.Children.Add(checkBox);
            mainStack.Children.Add(infoStack);
            mainStack.Children.Add(priceText);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 36, 53)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 8),
                Child = mainStack,
                Cursor = System.Windows.Input.Cursors.Hand
            };
        }

        private void LessonCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var lesson = checkBox?.Tag as Lessons;

            if (lesson == null) return;

            if (checkBox.IsChecked == true)
            {
                if (!selectedLessons.Any(l => l.LessonId == lesson.LessonId))
                {
                    selectedLessons.Add(lesson);
                }
            }
            else
            {
                selectedLessons.RemoveAll(l => l.LessonId == lesson.LessonId);
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            // Total due
            int totalDue = unpaidLessons.Count * lessonPrice;
            TotalDueText.Text = $"{totalDue} ₪";

            // Selected amount
            int selectedAmount = selectedLessons.Count * lessonPrice;
            SelectedAmountText.Text = $"{selectedAmount} ₪";
            SelectedCountText.Text = $"{selectedLessons.Count} lesson(s) selected";

            // Update installment info
            Installments_Changed(null, null);
        }

        private void Installments_Changed(object sender, SelectionChangedEventArgs e)
        {
            int selectedAmount = selectedLessons.Count * lessonPrice;
            int installments = InstallmentsCombo.SelectedIndex + 1;

            if (installments > 1 && selectedAmount > 0)
            {
                int perPayment = selectedAmount / installments;
                InstallmentInfoText.Text = $"You will pay {perPayment} ₪ per month for {installments} months";
                InstallmentInfoText.Visibility = Visibility.Visible;
            }
            else
            {
                InstallmentInfoText.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            selectedLessons.Clear();
            selectedLessons.AddRange(unpaidLessons);
            LoadUnpaidLessons(); // Refresh to check all boxes
            UpdateSummary();
        }

        private void ProcessPayment_Click(object sender, RoutedEventArgs e)
        {
            if (selectedLessons.Count == 0)
            {
                MessageBox.Show("Please select at least one lesson to pay for.", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int totalAmount = selectedLessons.Count * lessonPrice;
            string paymentMethod = (PaymentMethodCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            int installments = InstallmentsCombo.SelectedIndex + 1;

            var result = MessageBox.Show(
                $"Confirm Payment:\n\n" +
                $"Amount: {totalAmount} ₪\n" +
                $"Lessons: {selectedLessons.Count}\n" +
                $"Method: {paymentMethod}\n" +
                $"Installments: {installments}\n\n" +
                "Proceed with payment?",
                "Confirm Payment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var srv = new Service1Client();
                int teacherId = srv.GetTeacherId(LogIn.sign.Id);

                foreach (var lesson in selectedLessons)
                {
                    var payment = new Payment
                    {
                        StudentID = LogIn.sign.Id,
                        TeacherID = teacherId,
                        PaymentID = lesson.LessonId,
                        LessonId = lesson.LessonId,
                        Amount = lessonPrice,
                        PaymentDate = DateTime.Now,
                        PaymentMethod = paymentMethod,
                        NumberOfPayments = installments,
                        paid = true,
                        Status = "Paid",
                        Notes = $"Payment for lesson on {lesson.Date}"
                    };

                    srv.Pay(payment);
                }

                MessageBox.Show($"Payment successful!\n\n{selectedLessons.Count} lesson(s) paid.\nTotal: {totalAmount} ₪",
                    "Payment Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Refresh
                selectedLessons.Clear();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPaymentHistory()
        {
            PaymentHistoryPanel.Children.Clear();

            try
            {
                var srv = new Service1Client();
                var payments = srv.SelectPaymentByStudentID(LogIn.sign.Id)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(10)
                    .ToList();

                if (payments.Count == 0)
                {
                    PaymentHistoryPanel.Children.Add(new TextBlock
                    {
                        Text = "No payment history yet.",
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic
                    });
                    return;
                }

                foreach (var payment in payments)
                {
                    var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };

                    stack.Children.Add(new TextBlock
                    {
                        Text = payment.paid ? "✓" : "⏳",
                        Foreground = payment.paid ? Brushes.LightGreen : Brushes.Orange,
                        Width = 25
                    });

                    stack.Children.Add(new TextBlock
                    {
                        Text = payment.PaymentDate.ToString("dd/MM/yyyy"),
                        Foreground = Brushes.White,
                        Width = 90
                    });

                    stack.Children.Add(new TextBlock
                    {
                        Text = $"{payment.Amount} ₪",
                        Foreground = new SolidColorBrush(Color.FromRgb(0, 224, 255)),
                        FontWeight = FontWeights.SemiBold,
                        Width = 80
                    });

                    stack.Children.Add(new TextBlock
                    {
                        Text = payment.PaymentMethod ?? "N/A",
                        Foreground = Brushes.Gray,
                        FontSize = 12
                    });

                    PaymentHistoryPanel.Children.Add(stack);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading payment history: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            page.Navigate(new StudentUI());
        }
    }
}
