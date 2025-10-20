using SearchPlant.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;
using SearchPlant.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddRazorPages();
builder.Services.AddDbContext<SearchPlantContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SearchPlantDb")));
builder.Services.AddControllers();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/WarningUser";
    });





builder.Services.AddAntiforgery(options =>
{
    // Cấu hình này giúp tương thích với các lệnh gọi AJAX sau này
    options.HeaderName = "XSRF-TOKEN";
});


builder.Services.AddAuthorization();



// Cấu hình SmtpSettings
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Đăng ký EmailService
builder.Services.AddTransient<IEmailService, EmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}





app.UseHttpsRedirection();

// ✅ ĐẶT UseRouting() LÊN TRƯỚC UseStaticFiles() ĐỂ ƯU TIÊN API
app.UseRouting();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".rdlx-json"] = "application/json";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});


app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.MapGet("/", context =>
{
    context.Response.Redirect("/RedirectToPage");
    return Task.CompletedTask;
});

app.Run();