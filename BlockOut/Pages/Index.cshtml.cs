using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Scheduler;

namespace BlockOut.Pages
{
    

    public class IndexModel : PageModel
    {
        private readonly SchedulerService _schedulerService;

        public IndexModel(SchedulerService schedulerService)
        {
            _schedulerService = schedulerService;
        }

        public string Result { get; private set; }

        public void OnGet()
        {
            Result = _schedulerService.ScheduleTask("My Task", DateTime.Now);
        }
    }

}