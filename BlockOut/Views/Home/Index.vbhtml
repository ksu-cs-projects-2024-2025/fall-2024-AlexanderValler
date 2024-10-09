@Imports BlockOut.Scheduler
@ModelType Blockout.views.home.IndexModel

<div class="jumbotron">
    <h1>Number one in automatic scheduling</h1>
    <p class="lead">Your online scheduler for all your scheduling needs</p>
</div>

<div class="row">
    <div class="col-md-12">
        <h2>Input Scheduling Information</h2>
        <form method="post">
            <div class="form-group">
                <label asp-for="NewSchedulingInfo.Title"></label>
                <input asp-for="NewSchedulingInfo.Title" class="form-control" />
                <span asp-validation-for="NewSchedulingInfo.Title" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="NewSchedulingInfo.Description"></label>
                <textarea asp-for="NewSchedulingInfo.Description" class="form-control"></textarea>
                <span asp-validation-for="NewSchedulingInfo.Description" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="NewSchedulingInfo.Date"></label>
                <input asp-for="NewSchedulingInfo.Date" class="form-control" type="date" />
                <span asp-validation-for="NewSchedulingInfo.Date" class="text-danger"></span>
            </div>
            <div class="form-group">
                <label asp-for="NewSchedulingInfo.Status"></label>
                <select asp-for="NewSchedulingInfo.Status" class="form-control">
                    <option value="">Select Status</option>
                    <option value="Pending">Pending</option>
                    <option value="Confirmed">Confirmed</option>
                    <option value="Canceled">Canceled</option>
                </select>
                <span asp-validation-for="NewSchedulingInfo.Status" class="text-danger"></span>
            </div>
            <button type="submit" class="btn btn-primary">Submit</button>
        </form>
    </div>
</div>

<div class="row">
    <div class="col-md-12">
        <h2>Scheduled Information</h2>
        <table class="table table-bordered">
            <thead>
                <tr>
                    <th>Title</th>
                    <th>Description</th>
                    <th>Date</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                @For Each info In BlockOut.Scheduler.Creator
                    <tr>
                        <td>info.Title</td>
                        <td>info.Description</td>
                        <td>info.Date.ToString("yyyy-MM-dd")</td>
                        <td>info.Status</td>
                    </tr>
                Next
            </tbody>
        </table>
    </div>
</div>
