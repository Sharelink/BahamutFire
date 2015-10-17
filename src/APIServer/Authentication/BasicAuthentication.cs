using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Net;
using BahamutService;

namespace BahamutFire.APIServer.Authentication
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
            Console.WriteLine(httpContext.Request.Path);
            var res = Startup.TokenService.ValidateAppToken(appkey, userId, token).Result;
            if(res != null)
            {
                httpContext.Request.Headers["accountId"] = res.AccountId;
                return _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
            
        }
    }
}
