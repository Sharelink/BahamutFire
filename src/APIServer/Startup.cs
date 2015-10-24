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
using DataLevelDefines;
using Microsoft.Framework.Configuration;
using BahamutFire.APIServer.Authentication;
using Microsoft.Dnx.Runtime;
using ServiceStack.Redis;
using ServerControlService.Model;

namespace BahamutFire.APIServer
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            BahamutFireDbConfig = new MongoDbServerConfig()
            {
                Url = Configuration["Data:BahamutFireDBServer:url"]
            };
            AppUrl = Configuration["Data:App:url"];
            Appkey = Configuration["Data:App:appkey"];
        }
        public static IRedisServerConfig TokenServerConfig { private set; get; }
        public static TokenService TokenService { private set; get; }
        public static IMongoDbServerConfig BahamutFireDbConfig { get; private set; }
        public static string Appkey { get; private set; }
        public static string AppUrl { get; private set; }
        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            var TokenServerClientManager = new RedisManagerPool(Configuration["Data:TokenServer:url"]);
            TokenService = new TokenService(TokenServerClientManager);

            var ControlServerServiceClientManager = new RedisManagerPool(Configuration["Data:ControlServiceServer:url"]);
            var serverMgrService = new ServerControlService.Service.ServerControlManagementService(ControlServerServiceClientManager);
            var appInstance = new BahamutAppInstance()
            {
                Appkey = Startup.Appkey,
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
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();
            // Add MVC to the request pipeline.
            app.UseMiddleware<BasicAuthentication>(); //must in front of UseMvc
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
        }
    }
}
