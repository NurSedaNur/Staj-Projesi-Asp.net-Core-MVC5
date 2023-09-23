using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Staj.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staj
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:SqlCon"]));

            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddSingleton(_webHostEnvironment);


            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)//GÝRÝÞ ÝÇÝN-------------------------------------
            .AddCookie(opt =>
            {
                opt.LoginPath = "/Login/Giris";//BU GÝRÝÞ YAPMAYAN KULLANICILAR ÜRÜNLER SAYFASINDA TIKLADIGINDA ÝZÝNLERÝ OLMADIGINDAN URUNLER YERÝNE GELECEK OLAN SAYFA
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Rol", "A"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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

            app.UseAuthentication();// Kimlik doðrulama middleware'i-----------SONRADAN EKLEDÝM.---------------------
            app.UseAuthorization();// Yetkilendirme middleware'i---------------VARDI

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Login}/{action=Giris}/{id?}");
            });
        }
    }
}
