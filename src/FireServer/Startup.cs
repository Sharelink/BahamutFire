using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BahamutService;
using Microsoft.Extensions.Configuration;
using ServerControlService.Model;
using NLog;
using ServerControlService.Service;
using NLog.Config;
using BahamutCommon;
using BahamutAspNetCommon;
using System.IO;
using Newtonsoft.Json.Serialization;
using DataLevelDefines;
using BahamutFireService.Service;
using Newtonsoft.Json;
using ServerControlService;

namespace FireServer
{
    public class Program
    {
        public static IConfiguration ArgsConfig { get; private set; }
        public static void Main(string[] args)
        {
            ArgsConfig = new ConfigurationBuilder().AddCommandLine(args).Build();
            var configFile = ArgsConfig["config"];
            if (string.IsNullOrEmpty(configFile))
            {
                Console.WriteLine("No Config File");
            }
            else
            {
                var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(ArgsConfig)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();

                var appConfig = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configFile).Build();
                var urls = appConfig["Data:App:urls"].Split(new char[] { ';', ',', ' ' });
                hostBuilder.UseUrls(urls);
                hostBuilder.Build().Run();
            }
        }
    }

    public class Startup
    {
        public static IConfiguration Configuration { private set; get; }
        public static string Appkey { get { return Configuration["Data:App:appkey"]; } }
        public static string AppChannelId { get { return Configuration[string.Format("AppChannel:{0}:channel", Appkey)]; } }
        public static string Appname { get { return Configuration["Data:App:appname"]; } }
        public string AppRegion { get { return Configuration["Data:App:region"]; } }

        public static string AppUrl { get { return Configuration["Data:ServiceApiUrl"]; } }
        public static IServiceProvider AppServiceProvider { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            ReadConfig(env);
        }

        private static void ReadConfig(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);
            var configFile = Program.ArgsConfig["config"];
            var baseConfig = builder.AddJsonFile(configFile, true, true).Build();
            var logConfig = baseConfig["Data:LogConfig"];
            builder.AddJsonFile(configFile, true, true);
            builder.AddJsonFile(logConfig, true, true);
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        
        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config => {
                config.Filters.Add(new LogExceptionFilter());
            }).AddJsonOptions(x =>
            {
                x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddScoped<LogExceptionFilter>();

            services.AddSingleton(new FireService(DBClientManagerBuilder.GeneratePoolMongodbClient(Configuration.GetSection("Data:BahamutFireDBServer"))));

            var TokenServerClientManager = DBClientManagerBuilder.GenerateRedisConnectionMultiplexer(Configuration.GetSection("Data:TokenServer"));
            var tokenService = new TokenService(TokenServerClientManager);
            services.AddSingleton(tokenService);

            var ControlServerServiceClientManager = DBClientManagerBuilder.GenerateRedisConnectionMultiplexer(Configuration.GetSection("Data:ControlServiceServer"));
            var serverControlMgrService = new ServerControlManagementService(ControlServerServiceClientManager);
            services.AddSingleton(serverControlMgrService);

        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            AppServiceProvider = app.ApplicationServices;
            //Log
            var logConfig = new LoggingConfiguration();
            LoggerLoaderHelper.LoadLoggerToLoggingConfig(logConfig, Configuration, "Logger:fileLoggers");

            if (env.IsDevelopment())
            {
                LoggerLoaderHelper.AddConsoleLoggerToLogginConfig(logConfig);
            }
            LogManager.Configuration = logConfig;

            // Configure the HTTP request pipeline.

            //Authentication
            app.UseMiddleware<TokenAuthentication>(Appkey, AppServiceProvider.GetService<TokenService>());

            // Add MVC to the request pipeline.
            app.UseMvc();

            //Regist App Instance
            BahamutAppInstanceRegister.RegistAppInstance(AppServiceProvider.GetServerControlManagementService(), new BahamutAppInstance()
            {
                Appkey = Appkey,
                Region = AppRegion,
                Channel = AppChannelId,
                InfoForClient = JsonConvert.SerializeObject(new
                {
                    apiUrl = AppUrl
                })
            });

            LogManager.GetLogger("Main").Info("Fire Started!");
        }
    }

    public static class IGetAppService
    {
        public static ServerControlManagementService GetServerControlManagementService(this IServiceProvider provider)
        {
            return provider.GetService<ServerControlManagementService>();
        }

        public static TokenService GetTokenService(this IServiceProvider provider)
        {
            return provider.GetService<TokenService>();
        }

        public static FireService GetFireService(this IServiceProvider provider)
        {
            return provider.GetService<FireService>();
        }
    }
}
