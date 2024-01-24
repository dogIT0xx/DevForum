using Blog.Data;
using Blog.Services.Firebase;
using Blog.Services.MailService;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Config db context
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            #endregion

            #region Config Identity
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
                options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<BlogDbContext>();

            builder.Services.Configure<IdentityOptions>(options =>
                {
                    // User
                    options.User.AllowedUserNameCharacters =
                            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                    options.User.RequireUniqueEmail = true;

                    // Password
                    //options.Password.RequireDigit = true;
                    //options.Password.RequireLowercase = true;
                    //options.Password.RequireNonAlphanumeric = true;
                    //options.Password.RequireUppercase = true;
                    //options.Password.RequiredLength = 6;
                    //options.Password.RequiredUniqueChars = 1;

                    // SignIn
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                });
            #endregion

            #region Authen configs
            
            builder.Services
                .AddAuthentication(options =>
                {
                    // Cấu hình default
                    //options.DefaultAuthenticateScheme =  CookieAuthenticationDefaults.AuthenticationScheme;
                    //options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddGoogle(options =>
                {
                    var section = builder.Configuration.GetSection("google-login");
                    options.ClientId = section["client_id"];
                    options.ClientSecret = section["client_secret"];
                    options.CallbackPath = "/signin-google";
                });
            #endregion

            #region Firebase storage file configs
            // Cấu hình biến môi trường firebase để dùng cho authen thư viện firebase.cloud
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"firebase-config.json");
            // Get bucket in appsetting.json
            var bucket = builder.Configuration.GetSection("Firebase").GetSection("StorageFile")["Bucket"];
            builder.Services.Configure<StorageFileSetting>(options =>
            {
                options.Bucket = bucket!;
                options.StorageClient = StorageClient.Create();
            });
            builder.Services.AddTransient<IStorageFile, StorageFile>();
            #endregion

            #region Email configs
            builder.Services.AddOptions();  // Kích hoạt Options
            var mailSetting = builder.Configuration.GetSection("MailSetting");  // đọc config
            builder.Services.Configure<MailSetting>(mailSetting); // map mailSetting vào MailSetting class
            builder.Services.AddTransient<ISendMailService, SendMailService>();
            #endregion

            var app = builder.Build();

            #region Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
            #endregion
        }
    }
}
