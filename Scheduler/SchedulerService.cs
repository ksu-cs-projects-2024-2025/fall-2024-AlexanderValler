namespace Scheduler
{
    public class SchedulerService
    {
        public string ScheduleTask(string taskName, DateTime scheduledTime)
        {
            // Logic for scheduling a task
            return $"Task '{taskName}' scheduled at {scheduledTime}.";
        }
    }
}
