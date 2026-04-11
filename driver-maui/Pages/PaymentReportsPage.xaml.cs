using driver_maui.Services;

namespace driver_maui.Pages
{
    public class PaymentItem
    {
        public string DateText { get; set; } = "";
        public string Method { get; set; } = "";
        public string AmountText { get; set; } = "";
    }

    public partial class PaymentReportsPage : ContentPage
    {
        public PaymentReportsPage() => InitializeComponent();

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var payments = await ServiceHelper.CallAsync(srv => srv.SelectPaymentByTeacherIDAsync(AppState.UserId));
                if (payments != null)
                {
                    int total = payments.Where(p => p.paid).Sum(p => p.Amount);
                    int thisMonth = payments
                        .Where(p => p.paid && p.PaymentDate.Month == DateTime.Now.Month && p.PaymentDate.Year == DateTime.Now.Year)
                        .Sum(p => p.Amount);
                    int pending = payments.Count(p => !p.paid);

                    TotalEarningsLabel.Text = $"Total Earnings: ₪{total}";
                    ThisMonthLabel.Text = $"This Month: ₪{thisMonth}";
                    PendingLabel.Text = $"Pending Lessons: {pending}";

                    RecentList.ItemsSource = payments
                        .Where(p => p.paid)
                        .OrderByDescending(p => p.PaymentDate)
                        .Take(20)
                        .Select(p => new PaymentItem
                        {
                            DateText = p.PaymentDate.ToString("dd/MM/yyyy"),
                            Method = p.PaymentMethod ?? "Cash",
                            AmountText = $"₪{p.Amount}"
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Back_Click(object s, EventArgs e) => await Shell.Current.GoToAsync("//TeacherHome");
    }
}
