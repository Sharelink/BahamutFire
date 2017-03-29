using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BahamutCommon;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FireServer.Controllers
{
    [Route("[controller]")]
    public class BahamutFiresController : Controller
    {
        [HttpGet("{accessKey}")]
        public async Task<object> Get(string accessKey)
        {
            try
            {
                var fileId = accessKey;
                var fireService = Startup.AppServiceProvider.GetFireService();
                var accountId = Request.Headers["accountId"];
                var fireRecord = await fireService.GetFireRecord(fileId);
                if (null == fireRecord)
                {
                    throw new Exception();
                }
                return new
                {
                    fileId = fireRecord.Id.ToString(),
                    server = fireRecord.UploadServerUrl,
                    accessKey = fireRecord.Id.ToString(),
                    bucket = fireRecord.Bucket,
                    serverType = fireRecord.ServerType,
                    expireAt = DateTimeUtil.ToString(DateTime.UtcNow.AddDays(7))
                };
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetLogger("Warning").Warn(ex, "Access Key Not Found:{0}", accessKey);
                Response.StatusCode = 404;
                return new { msg = "ACCESS_KEY_NOT_FOUND" };
            }
        }
    }
}
