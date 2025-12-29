using Common.Configs;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using HealthChecks.UI.Client;
using Common.Email;
using Common.Interfaces;
using Common.Extensions;
using TalentaReceiver.Config;
using TalentaReceiver.UseCases;
using TalentaReceiver.Services;
using TalentaReceiver.Repositories.MsSql;
using TalentaReceiver.Repositories;
using Elastic.Apm.Api;
using FluentValidation;
using TalentaReceiver.Validators;
using jpk3service.Repositories;
using TalentaReceiver.Utils;
using TalentaReceiver.Protos;

namespace TalentaReceiver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //Setting for dapper
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, EmailService>();
            services.Configure<SAPUrlSettings>(Configuration.GetSection("SAPUrl"));

            #region Register client api rest / grpc

            var policyConfigs = new HttpClientPolicyConfiguration();
            Configuration.Bind("HttpClientPolicies", policyConfigs);

            #endregion

            //services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            #region Redis Configuration
            //services.AddStackExchangeRedisCache(options =>
            //{
            //    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
            //    {
            //        EndPoints = { Configuration.GetValue<string>("CacheSettings:ConnectionString") },
            //        DefaultDatabase = Configuration.GetValue<int>("CacheSettings:Database"),
            //        User = Configuration.GetValue<string>("CacheSettings:User"),
            //        Password = Configuration.GetValue<string>("CacheSettings:Password"),
            //        Ssl = false
            //    };
            //});
            #endregion

            #region IOC Register
            services.AddScoped<IDbConnectionFactory>(_ => new Config.MySql.DbConnectionFactory(Configuration.GetValue<string>("DatabaseSettings:ConnectionString")!));

            services.AddScoped<IDbConnectionMsSqlFactory>(_ => new Config.MsSql.DbConnectionMsSqlFactory(Configuration.GetValue<string>("DatabaseSettings:JPK3Jkt")!));

            services.AddScoped<IDbConnectionMsSqlFactory2>(_ => new Config.MsSql.DbConnectionMsSqlFactory2(Configuration.GetValue<string>("DatabaseSettings:JPK3Sby")!));

            //ResClient dan GRPC Client IOC tidak perlu ditambahkan
            services.AddScoped<IMTEmployee, MTEmployee>();
            services.AddScoped<IMTPwsDws, MTPwsDws>();
            services.AddScoped<IMTPesertaDb, MTPesertaDb>();
            services.AddScoped<IMTBBGCompanyDb, MTBBGCompanyDb>();
            services.AddScoped<IMTBBGJobLevelDb, MTBBGJobLevelDb>();

            services.AddScoped<IMTEmployeeRepositories, MTEmployeeRepositories>();
            services.AddScoped<IMTBBGJobLevelRepository, MTBBGJobLevelRepository>();
            services.AddScoped<IMTBBGCompanyRepository, MTBBGCompanyRepository>();
            services.AddScoped<IMTPesertaTempRepository, MTPesertaTempRepository>();

            services.AddScoped<ITalentaReceiverUsecase, TalentaReceiverUsecase>();
            services.AddScoped<IPeriodDailyWorkScheduleUsecase, PeriodDailyScheduleUsecase>();
            services.AddScoped<IValidator<Models.MT_Peserta_Temp>, MTPesertaTempValidator>();

            services.AddScoped<IHelperService, HelperService>();
            services.AddScoped<IMTJobLogSAP, MTJobLogSAP>();
            services.AddScoped<IMTJobLogRepository, MTJobLogSAPRepository>();

            services.AddScoped<ISchedulerUsecase, SchedulerUsecase>();

            services.AddScoped<ISapLogDataUsecase, SapLogDataUsecase>();
            services.AddScoped<IUrlSettingProvider, UrlSettingProvider>();
            services.AddScoped<ISchedularHelperService, SchedularHelperService>();
            #endregion

            services.AddHttpClient();

            services.AddAutoMapper(typeof(Startup));

            services.AddGrpc().AddJsonTranscoding();

            services.AddGrpcReflection();

            services.AddControllers();
            services.AddGrpcSwagger();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JPK3 service", Version = "v1" });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Input bearer token here",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            services.AddHealthChecks();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseAllElasticApm(Configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "talenta receiver service v1"));
            app.UseRouting();

            app.UseAuthorization();
            app.UseCustomResponseMiddleware();
            app.UseSSEMiddleware();

            app.UseEndpoints(endpoints =>
            {
                #region Service Register
                //endpoints.MapGrpcService<OrganizationService>();
                endpoints.MapGrpcService<TalentaReceiverService>();
                endpoints.MapGrpcService<PeriodDailyWorkScheduleService>();
                endpoints.MapGrpcService<SapLogDataService>();
                endpoints.MapGrpcService<SchedulerService>();

                #endregion

                endpoints.MapGrpcReflectionService(); //  Focus!!!
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });

                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        }
    }
}