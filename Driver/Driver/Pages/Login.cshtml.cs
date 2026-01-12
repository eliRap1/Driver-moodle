using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

public class LoginModel : PageModel
{
    private Service1Client srv = new Service1Client();

    [BindProperty]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Please fill all fields";
            return Page();
        }

        bool exists = srv.CheckUserExist(Username);
        bool valid = srv.CheckUserPassword(Username, Password);

        if (!exists || !valid)
        {
            ErrorMessage = "Invalid username or password";
            return Page();
        }

        bool isAdmin = srv.CheckUserAdmin(Username);
        int userId = srv.GetUserID(Username, isAdmin ? "Teacher" : "Student");

        // simple session storage
        HttpContext.Session.SetInt32("UserId", userId);
        HttpContext.Session.SetString("Username", Username);
        HttpContext.Session.SetString("Role", isAdmin ? "Teacher" : "Student");

        return isAdmin
            ? RedirectToPage("/Teacher/TeacherHome")
            : RedirectToPage("/Student/StudentHome");
    }
}
