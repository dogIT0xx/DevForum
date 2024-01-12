using Blog.Data;
using Blog.Services.Firebase;
using Blog.Services.MailService;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Text;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cấu hình biến môi trường firebase để dùng cho authen thư viện firebase.cloud
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"firebase-config.json");

            #region Config db context
            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
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
            //builder.Services
            //    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //    {
            //        // Cấu hình đường dẫn khi chưa truy cập chức năng mà chưa Authen
            //        options.LoginPath = "/Identity/Account/Register";
            //    });
            #endregion

            #region Firebase storage file configs
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
            builder.Services.Configure<MailSetting>(mailSetting); // đăng ký để Inject
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

            //// cấu hình map area
            //app.MapControllerRoute(
            //    name: "Identity",
            //    pattern: "{area:exists}/{controller}/{action}/{id?}");
            //Cấu hình map mặc định
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            // Tự nó map area mặc định đcm nó
            // app.MapRazorPages();
            app.Run();
            #endregion

            #region Test upload file storage firebase
            //// thêm 2 file1.jpq, file2.jpq thì mới hoạt động được
            //var client = StorageClient.Create();

            //// Create a bucket with a globally unique name
            //var bucket = "blogmvc-b2c63.appspot.com";

            //// Upload text file
            //var content = Encoding.UTF8.GetBytes("hello, world");
            //var obj1 = client.UploadObject(bucket, "file1.txt", "text/plain", new MemoryStream(content));
            //// Upload image - Xác định content type "image/jpeg" rất quan trọng để nó hiển thị trên 
            //Stream stream1 = new FileStream("file1.jpg", FileMode.Open);
            //var obj2 = client.UploadObject(bucket, "file1.jpg", "image/jpeg", stream1);

            //var storage = new StorageImage(bucket, client);
            //var s = storage.UploadImage("file2.jpg", "file2.jpg").ToString();

            //// Download file
            //using (var stream = File.OpenWrite("file1.txt"))
            //{
            //    client.DownloadObject(bucket, "file1.txt", stream);
            //}
            #endregion

            #region Test send email
            //app.UseRouting()
            //   .UseEndpoints(endpoints =>
            //   {
            //       endpoints.MapGet("/testmail", async context =>
            //       {

            //           // lấy dịch vụ sendmailservice
            //           var sendMailService = context.RequestServices.GetService<ISendMailService>();
            //           var content = new MailContent
            //           {
            //               To = "nguyenviethoang.2003.personal@gmail.com",
            //               Subject = "kiểm tra thử",
            //               Body = "<p><strong>xin chào việt hoàng</strong></p>"
            //           };

            //           await sendMailService.SendEmailAsync(content);
            //           await context.Response.WriteAsync("send mail");
            //       });
            //   });
            #endregion
        }
    }
}
