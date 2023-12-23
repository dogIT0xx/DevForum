using Blog.Data;
using Blog.Services.MailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;

                    // SignIn
                    options.SignIn.RequireConfirmedEmail = true;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                });

            // Email config
            builder.Services.AddOptions();  // Kích hoạt Options
            var mailSettings = builder.Configuration.GetSection("MailSettings");  // đọc config
            builder.Services.Configure<MailSettings>(mailSettings); // đăng ký để Inject
            builder.Services.AddTransient<ISendMailService, SendMailService>();



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
                pattern: "{controller=Page}/{action=Home}/{id?}");

            //test send email
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
            // Tự nó map area mặc định đcm nó
            // app.MapRazorPages();
            app.Run();
        }
    }
}
