// File: Pages/Creations/Template.cshtml.cs
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlockOut.Pages.Creations
{
    public class TemplateModel : PageModel
    {
        public string PageTitle { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        public void OnGet()
        {
            // Placeholder values for testing; replace with dynamic data in generated pages
            PageTitle = "Sample Schedule";
            Description = "This is a template page. Customize it with dynamic data.";
            Date = DateTime.Today;
            Time = new TimeSpan(14, 0, 0);
        }
    }
}
