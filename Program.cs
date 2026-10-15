using System.Globalization;
using GameLauncher.Models;
using GameLauncher.Services;
using Microsoft.AspNetCore.Localization;           
using Microsoft.AspNetCore.Mvc.Razor;              
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Add services to the container.

builder.Services.AddSingleton<DomainService>();
builder.Services.AddSingleton<EmployeePhotoService>();
builder.Services.AddLocalization(options =>              
	options.ResourcesPath = "Resources");                 

builder.Services.AddControllersWithViews()
	.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)  
	.AddDataAnnotationsLocalization();

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddDbContext<InternsDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

var supportedCultures = new[]                                     
{                                                                  
    new CultureInfo("en"),                                         
    new CultureInfo("es"),                                        
};                                                                
																 
app.UseRequestLocalization(new RequestLocalizationOptions         
{                                                                  
	DefaultRequestCulture = new RequestCulture("en"),             
	SupportedCultures = supportedCultures,                        
	SupportedUICultures = supportedCultures,                     
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Store/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Store}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
