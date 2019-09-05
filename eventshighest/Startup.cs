using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eventshighest.Controllers.Repository;
using eventshighest.Data;
using eventshighest.Interface;
using eventshighest.Repository;
using eventshighest.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rotativa.AspNetCore;

namespace eventshighest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration["Dbconstring:dbConnectionString"]);
            });
            services.AddIdentity<AppUser, ApplicationRole>(
                   option =>
                   {
                       option.Password.RequireDigit = false;
                       option.Password.RequiredLength = 6;
                       option.Password.RequireNonAlphanumeric = false;
                       option.Password.RequireUppercase = false;
                       option.Password.RequireLowercase = false;
                   }
               ).AddEntityFrameworkStores<ApplicationDbContext>()
               .AddDefaultTokenProviders();
            services.AddAuthentication(option => {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = Configuration["Jwt:Audience"],
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SigningKey"]))
                };
            });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHttpClient();
            services.AddScoped<IActivityRepository, ActivityRepository>();
            services.AddScoped<ITicketclass, TicketRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IActivityService,Activityservice>();
            services.AddScoped<IVenueRepository,VenueRepository>();
            services.AddScoped<ISearchRepository,SearchRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<IpaymentsRepository,PaymentsRepository>();
            services.AddScoped<ITransactions, TransactionsRepository>();
            services.AddTransient<ICurrency, CurrencyRepository>();
            services.AddTransient<IJwtTokenService, JwtTokenService>();
            services.AddTransient<IravePayments, RavePaymentsRepository>();
            services.AddTransient<IEmailService, Email>();
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<OrderService>();
            services.AddHostedService<PaymentsService>();
            services.AddHostedService<CurrencyratesService>();
            //services.AddScoped<CustomAuthorizeFilter>();
            services.AddSingleton<IOrderBackgroundQueue, OrderQueueService>();
            services.AddSingleton<IPaymentsBackgroundQueue, PaymentsQueue>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseForwardedHeaders();
            //app.UseHttpsRedirection();
            RotativaConfiguration.Setup(env,"..\\Rotativa\\");
            app.UseMvc();
        }
    }
}
