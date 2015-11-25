using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutCommon;
using BahamutFireService.Service;

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
                var fireService = new FireService(Startup.BahamutFireDbUrl);
                var accountId = Request.Headers["accountId"];
                var fireRecord = await fireService.GetFireRecord(fileId);
                var bucket = "";
                if (AliOSSFileInfo.AliOssServerType == fireRecord.ServerType)
                {
                    var aliossInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AliOSSFileInfo>(fireRecord.Extra);
                    bucket = aliossInfo.Bucket;
                }
                return new
                {
                    fileId = fireRecord.Id.ToString(),
                    server = fireRecord.UploadServerUrl,
                    accessKey = fireRecord.Id.ToString(),
                    bucket = bucket,
                    serverType = fireRecord.ServerType,
                    expireAt = DateTimeUtil.ToString(DateTime.UtcNow.AddDays(7))
                };
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "AccessKey Not Found:{0}", accessKey);
                return HttpNotFound();
            }
        }
    }
}
