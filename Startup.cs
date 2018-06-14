using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommentsDownloader.Data;
using CommentsDownloader.Data.Interfaces;
using CommentsDownloader.Models;
using CommentsDownloader.Services;
using CommentsDownloader.Services.HostedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommentsDownloader {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddMvc (); //.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<CommentsDownloaderDbContext> (options => options.UseSqlite ("Data Source=CommentsDownloader.db"));

            services.AddSingleton<BackgroundReviewWorker> ();
            services.AddSingleton<BackgroundEMailSender> ();
            services.AddSingleton<IHostedService> (serviceProvider => serviceProvider.GetService<BackgroundReviewWorker> ());
            services.AddSingleton<IHostedService> (serviceProvider => serviceProvider.GetService<BackgroundEMailSender> ());

            services.AddSingleton<YoutubeFetcher> ();
            services.AddSingleton<AmazonFetcher> ();

            services.AddTransient<Func<string, ICommentFetcher>> (serviceProvider => key => {
                switch (key) {
                    case AppConstants.Youtube:
                        return serviceProvider.GetService<YoutubeFetcher> ();
                    case AppConstants.Amazon:
                        return serviceProvider.GetService<AmazonFetcher> ();
                    default:
                        throw new KeyNotFoundException (); // or maybe return null, up to you
                }
            });

            services.AddScoped<IUnitOfWork, UnitOfWork> ();
            services.AddScoped (typeof (IRepository<>), typeof (Repository<>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseExceptionHandler ("/Home/Error");
            }

            app.UseStaticFiles ();

            app.UseMvc (routes => {
                routes.MapRoute (
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            #region perform automatic migrations
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory> ().CreateScope ()) {
                serviceScope.ServiceProvider.GetService<CommentsDownloaderDbContext>().Database.Migrate ();
            }
            #endregion
        }
    }
}
