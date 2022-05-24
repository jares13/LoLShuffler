using LoLShuffler.DAL;
using LoLShuffler.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace LoLShuffler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ConfigureServices(builder.Services, builder.Configuration);
            var app = builder.Build();
            ConfigureApp(app);
            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddRazorPages().AddRazorPagesOptions(options =>
            {
                options.Conventions.AddPageRoute("/TeamShufflerStart", "/");
            });

            services.AddDbContext<ShufflerDbContext>(option => option.UseNpgsql(configuration.GetConnectionString("ShufflerDb")));
            services.AddScoped<RiotService>();
            services.AddHttpClient();
            services.Configure<Keys>(configuration.GetSection("Keys"));
        }


        private static void ConfigureApp(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            using var dbContext = scope.ServiceProvider.GetService<ShufflerDbContext>();

            dbContext.Database.Migrate();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapRazorPages();
        }
    }
}




