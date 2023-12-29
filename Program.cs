using Blog.Data;
using Blog.Services.Firebase.Storage;
using Blog.Services.MailService;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
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

            // Add services to the container.
            var connectionString = builder.Configuration
                .GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<BlogDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // đăng kí dịch vụ Identity
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

            // Authen configs
            //builder.Services
            //    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //    .AddCookie(options =>
            //    {
            //        // Cấu hình đường dẫn khi chưa truy cập chức năng mà chưa Authen
            //        options.LoginPath = "/Identity/Account/Register";
            //    });

            // Email configs
            builder.Services.AddOptions();  // Kích hoạt Options
            var mailSetting = builder.Configuration.GetSection("MailSetting");  // đọc config
            builder.Services.Configure<MailSetting>(mailSetting); // đăng ký để Inject
            builder.Services.AddTransient<ISendMailService, SendMailService>();


            builder.Services.AddTransient<StorageClient>(sc => StorageClient.Create());
            builder.Services.AddTransient<StorageFile>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
            app.MapControllerRoute(
                name: "Identity",
                pattern: "{area:exists}/{controller}/{action}/{id?}");
            //Cấu hình map mặc định
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Post}/{action=Index}/{id?}");

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

            // Tự nó map area mặc định đcm nó
            // app.MapRazorPages();

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

            app.Run();
        }
    }
}
