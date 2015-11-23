using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Net;
using BahamutService;

namespace FireServer.Authentication
{

    // You may need to install the Microsoft.AspNet.Http.Abstractions package into your project
    public class BasicAuthentication
    {
        private readonly RequestDelegate _next;

        public BasicAuthentication(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var userId = httpContext.Request.Headers["userId"];
            var token = httpContext.Request.Headers["token"];
            var appkey = httpContext.Request.Headers["appkey"];
            var res = Startup.TokenService.ValidateAppToken(appkey, userId, token).Result;
            if (res != null)
            {
                try
                {
                    NLog.LogManager.GetCurrentClassLogger().Info("Request:{0}", httpContext.Request.Path);
                    httpContext.Request.Headers["accountId"] = res.AccountId;
                    return _next(httpContext);
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex, "Invoke Request Error:{0}", ex.Message);
                    return null;
                }
            }
            else
            {
                NLog.LogManager.GetCurrentClassLogger().Info("Validate Failed:{0}", userId);
                return null;
            }
            
        }
    }
}
