using System;
using FCDBApp.Models;
using FCDBApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FCDBApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices((context, services) =>
                    {
                        // Add Razor Pages
                        services.AddRazorPages();

                        // Add DbContext with SQL Server
                        services.AddDbContext<InspectionContext>(options =>
                            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));

                        // Add Authentication with Cookie Scheme
                        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                            .AddCookie(options =>
                            {
                                options.LoginPath = "/Auth/Login"; // Path to redirect for login
                                options.AccessDeniedPath = "/Auth/AccessDenied"; // Path for access denied
                            });

                        // Add Authorization
                        services.AddAuthorization();

                        // Add custom services
                        services.AddScoped<InspectionSheetService>();
                        services.AddTransient<ExportHandler>();

                        // Configure CORS to allow all origins
                        services.AddCors(options =>
                        {
                            options.AddPolicy("AllowAllOrigins", builder =>
                            {
                                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                            });
                        });

                        // Configure Antiforgery token
                        services.AddAntiforgery(options =>
                        {
                            options.HeaderName = "X-CSRF-TOKEN";
                        });

                        // Configure logging
                        services.AddLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.AddConsole();
                            logging.AddDebug();
                        });

                        // Add MVC
                        services.AddMvc();
                    })
                    .Configure((context, app) =>
                    {
                        var env = context.HostingEnvironment;

                        // Configure error handling
                        if (env.IsDevelopment())
                        {
                            app.UseDeveloperExceptionPage();
                        }
                        else
                        {
                            app.UseExceptionHandler("/Error");
                            app.UseHsts();
                        }

                        // Middleware configuration
                        app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
                        app.UseStaticFiles(); // Serve static files
                        app.UseRouting(); // Enable routing
                        app.UseCors("AllowAllOrigins"); // Enable CORS
                        app.UseAuthentication(); // Enable authentication
                        app.UseAuthorization(); // Enable authorization

                        // Endpoint configuration
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapRazorPages();
                            endpoints.MapControllers();
                        });
                    });
                });
    }
}
