using Scheduler; // Make sure to include the namespace

var builder = WebApplication.CreateBuilder(args);

// Register the SchedulerService
builder.Services.AddTransient<SchedulerService>();

// Other services...
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
