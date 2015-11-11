using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using BahamutService;
using Microsoft.Framework.Configuration;
using BahamutFire.APIServer.Authentication;
using Microsoft.Dnx.Runtime;
using ServiceStack.Redis;
using ServerControlService.Model;
using NLog;

namespace BahamutFire.APIServer
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath);
            if (env.IsDevelopment())
            {
                builder.AddJsonFile("config_debug.json");
            }
            else
            {
                builder.AddJsonFile("config.json");
            }
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            BahamutFireDbUrl = Configuration["Data:BahamutFireDBServer:url"];
            AppUrl = Configuration["Data:App:url"];
            Appkey = Configuration["Data:App:appkey"];
        }
        public static TokenService TokenService { private set; get; }
        public static string BahamutFireDbUrl { get; private set; }
        public static string Appkey { get; private set; }
        public static string AppUrl { get; private set; }
        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var tokenServerUrl = Configuration["Data:TokenServer:url"].Replace("redis://", "");
            IRedisClientsManager TokenServerClientManager = new PooledRedisClientManager(tokenServerUrl);
            TokenService = new TokenService(TokenServerClientManager);

            var serverControlUrl = Configuration["Data:ControlServiceServer:url"].Replace("redis://", "");
            IRedisClientsManager ControlServerServiceClientManager = new PooledRedisClientManager(serverControlUrl);
            var serverMgrService = new ServerControlService.Service.ServerControlManagementService(ControlServerServiceClientManager);
            var appInstance = new BahamutAppInstance()
            {
                Appkey = Appkey,
                InstanceServiceUrl = Configuration["Data:App:url"]
            };
            appInstance = serverMgrService.RegistAppInstance(appInstance);
            serverMgrService.StartKeepAlive(appInstance.Id);

            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //Log
            var logConfig = new NLog.Config.LoggingConfiguration();
            var fileTarget = new NLog.Targets.FileTarget();
            fileTarget.FileName = Configuration["Data:Log:logFile"];
            fileTarget.Name = "FileLogger";
            fileTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger}:${message}";
            logConfig.AddTarget(fileTarget);
            logConfig.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, fileTarget));
            LogManager.Configuration = logConfig;

            if (env.IsDevelopment())
            {
                var consoleLogger = new NLog.Targets.ColoredConsoleTarget();
                consoleLogger.Name = "ConsoleLogger";
                consoleLogger.Layout = @"${date:format=HH\:mm\:ss} ${logger}:${message}";
                logConfig.AddTarget(consoleLogger);
                logConfig.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, consoleLogger));
            }

            // Configure the HTTP request pipeline.
            app.UseStaticFiles();
            // Add MVC to the request pipeline.
            app.UseMiddleware<BasicAuthentication>(); //must in front of UseMvc
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            LogManager.GetCurrentClassLogger().Info("Toronto Started!");
        }
    }
}
