using driver_maui.Services;
using DriverService;

namespace driver_maui.Pages
{
    public class UnpaidLessonItem
    {
        public int LessonId { get; set; }
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
        public int Price { get; set; }
    }

    public partial class PaymentsPage : ContentPage
    {
        private int _teacherId;
        private int _lessonPrice = 200;

        public PaymentsPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ResultLabel.IsVisible = false;
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                _teacherId = await ServiceHelper.CallAsync(srv => srv.GetTeacherIdAsync(AppState.UserId));
                _lessonPrice = await ServiceHelper.CallAsync(srv => srv.GetEffectiveLessonPriceAsync(AppState.UserId));
                PriceLabel.Text = $"Lesson price: ₪{_lessonPrice}";

                var lessons = await ServiceHelper.CallAsync(srv => srv.GetAllStudentLessonsAsync(AppState.UserId));
                UnpaidList.ItemsSource = lessons?
                    .Where(l => !l.paid && l.Canceled != 1)
                    .Select(l => new UnpaidLessonItem
                    {
                        LessonId = l.LessonId,
                        Date = l.Date,
                        Time = l.Time,
                        Price = _lessonPrice
                    }).ToList();
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"Error: {ex.Message}";
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.IsVisible = true;
            }
        }

        private async void Pay_Click(object sender, EventArgs e)
        {
            var selected = UnpaidList.SelectedItems?.OfType<UnpaidLessonItem>().ToList();
            if (selected == null || selected.Count == 0)
            {
                await DisplayAlert("No Selection", "Please select at least one lesson.", "OK");
                return;
            }

            string method = PaymentMethodPicker.SelectedItem?.ToString() ?? "Cash";

            try
            {
                foreach (var lesson in selected)
                {
                    var payment = new Payment
                    {
                        StudentID = AppState.UserId,
                        TeacherID = _teacherId,
                        Amount = _lessonPrice,
                        PaymentDate = DateTime.Now,
                        PaymentMethod = method,
                        NumberOfPayments = 1,
                        ParcialAmount = _lessonPrice,
                        paid = true,
                        LessonId = lesson.LessonId,
                        Status = "Paid"
                    };
                    await ServiceHelper.CallAsync(srv => srv.PayAsync(payment));
                }

                ResultLabel.Text = $"Successfully paid {selected.Count} lesson(s)!";
                ResultLabel.TextColor = Colors.Green;
                ResultLabel.IsVisible = true;
                await LoadData();
            }
            catch (Exception ex)
            {
                ResultLabel.Text = $"Payment failed: {ex.Message}";
                ResultLabel.TextColor = Colors.Red;
                ResultLabel.IsVisible = true;
            }
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//StudentHome");
    }
}
