using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CSharpProjectCore.Core.Entities;
using CSharpProjectCore.Core.Models;
using CSharpProjectCore.Core.Utils;
using CSharpProjectCore.Core.ViewModels;
using CSharpProjectCore.Infrastructure;
using DataService.Repository;
using DataService.Service;
using DataService.UoW;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace CSharpProjectCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private AppSettings appSettings;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            SetupSwagger(services);
            SetupDbContext(services);
            SetupAutoMapper();
            SetupAuthentication(services);
            SetupDI(services);
            SetupFirebaseAuthentication(services);
        }

        private void SetupFirebaseAuthentication(IServiceCollection services)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromStream(File.OpenRead(appSettings.FirebaseConfigurationFile))
            });
        }

        private void SetupDbContext(IServiceCollection services)
        {
            services.AddDbContext<CSharpProjectCoreDBContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
        }

        private void SetupSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Project API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                var security = new Dictionary<string, IEnumerable<string>>
                                                  {
                                                      {"Bearer", new string[] { }},
                                                  };
                c.AddSecurityRequirement(security);
            });
        }

        private void SetupAutoMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<RegisteredUser, User>();
                cfg.CreateMap<User, UserViewModel>();
            });
        }

        private void SetupAuthentication(IServiceCollection services)
        {

            const string scheme = JwtBearerDefaults.AuthenticationScheme;

            // Add authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = scheme;
                options.DefaultChallengeScheme = scheme;
                options.DefaultScheme = scheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.Audience,
                    ValidIssuer = appSettings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(appSettings.SecretKey))
                };
            });



        }

        private void SetupDI(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddSingleton(Configuration);
            services.AddScoped<ExtensionSettings>();
            services.AddScoped<JwtTokenProvider>();
            services.AddScoped<UnitOfWork>();
            services.AddScoped<DbContext, CSharpProjectCoreDBContext>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
        }





        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(options =>
            {
                options.AllowAnyMethod();
                options.AllowAnyOrigin();
                options.AllowAnyHeader();
            });

            loggerFactory.AddLog4Net();

            app.UseStaticFiles();

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project API");
            });
        }
    }
}
