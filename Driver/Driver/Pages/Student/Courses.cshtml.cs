using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using webTOsrv;

namespace Driver.Pages.Student
{
    public class CoursesModel : PageModel
    {
        private Service1Client srv = new Service1Client();

        public List<CourseViewModel> Courses { get; set; } = new();
        public int TotalModules { get; set; }
        public int TotalCompletedModules { get; set; }
        public int OverallProgress => TotalModules > 0 ? (TotalCompletedModules * 100 / TotalModules) : 0;
        public string Message { get; set; } = "";
        public bool IsSuccess { get; set; }

        public class CourseViewModel
        {
            public int CourseId { get; set; }
            public string CourseName { get; set; } = "";
            public string Description { get; set; } = "";
            public List<ModuleViewModel> Modules { get; set; } = new();
            public int TotalModules => Modules.Count;
            public int CompletedModules => Modules.Count(m => m.IsCompleted);
            public int ProgressPercent => TotalModules > 0 ? (CompletedModules * 100 / TotalModules) : 0;
        }

        public class ModuleViewModel
        {
            public int ModuleId { get; set; }
            public string ModuleName { get; set; } = "";
            public string Description { get; set; } = "";
            public int ModuleOrder { get; set; }
            public bool IsCompleted { get; set; }
        }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            LoadCourses(userId.Value);
            return Page();
        }

        public IActionResult OnPostMarkComplete(int moduleId, int courseId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
            {
                return RedirectToPage("/Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                bool result = srv.MarkModuleComplete(userId.Value, moduleId);
                if (result)
                {
                    TempData["Message"] = "Module marked as complete!";
                    TempData["IsSuccess"] = true;
                }
                else
                {
                    TempData["Message"] = "Failed to mark module as complete.";
                    TempData["IsSuccess"] = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking module complete: {ex.Message}");
                TempData["Message"] = "Failed to mark module as complete. Please try again.";
                TempData["IsSuccess"] = false;
            }

            return RedirectToPage();
        }

        private void LoadCourses(int studentId)
        {
            try
            {
                // Try to load from service first
                var courseProgress = srv.GetStudentCourseProgress(studentId);
                if (courseProgress != null && courseProgress.Length > 0)
                {
                    foreach (var cp in courseProgress)
                    {
                        var courseVm = new CourseViewModel
                        {
                            CourseId = cp.CourseId,
                            CourseName = cp.CourseName ?? "Unknown Course",
                            //Description = cp.CourseDescription ?? "",
                            Modules = new List<ModuleViewModel>()
                        };

                        // Get modules for this course
                        var modules = srv.GetCourseModules(cp.CourseId);
                        if (modules != null)
                        {
                            int order = 1;
                            foreach (var mod in modules)
                            {
                                var isCompleted = cp.ModuleProgress?.Any(mp => mp.ModuleId == mod.ModuleId && mp.IsCompleted) ?? false;
                                courseVm.Modules.Add(new ModuleViewModel
                                {
                                    ModuleId = mod.ModuleId,
                                    ModuleName = mod.ModuleName ?? "",
                                    Description = mod.Description ?? "",
                                    ModuleOrder = order++,
                                    IsCompleted = isCompleted
                                });
                            }
                        }

                        Courses.Add(courseVm);
                    }

                    // Calculate totals
                    foreach (var course in Courses)
                    {
                        TotalModules += course.TotalModules;
                        TotalCompletedModules += course.CompletedModules;
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading from service: {ex.Message}");
            }

            // No courses available from service - show empty state
            Courses = new List<CourseViewModel>();
        }
    }
}
