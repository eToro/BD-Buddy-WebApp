using ChatGPTClone.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ChatService>(); // Register ChatService
builder.Services.AddSingleton(new DatabaseRepository("server=stg-masterdb.trad.local;Persist Security Info=True;User ID=ETORO_ADMIN;Password=etoroadmin;database=etoro;Max Pool Size=1000;Connection Timeout=120"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.Run();

