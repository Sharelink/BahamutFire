using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;
using BahamutFireCommon;
using BahamutCommon;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FireServer.Controllers
{
    public class AliOSSFileInfo
    {
        public const string AliOssServerType = "alioss";
        public string Bucket { get; set; }
    }

    [Route("[controller]")]
    public class AliOSSFilesController : Controller
    {

        [HttpPost]
        public async Task<object> Post(string fileType, int fileSize)
        {
            try
            {
                var fireService = new FireService(Startup.BahamutFireDbUrl);
                var accountId = Request.Headers["accountId"];
                var aliOssInfo = new AliOSSFileInfo()
                {
                    Bucket = Startup.Configuration["Data:AliOSS:bucket"]
                };
                var newFire = new FireRecord()
                {
                    CreateTime = DateTime.UtcNow,
                    FileSize = fileSize,
                    FileType = fileType,
                    IsSmallFile = fileSize < 1024 * 256,
                    State = (int)FireRecordState.Create,
                    AccountId = accountId,
                    AccessKeyConverter = "",
                    UploadServerUrl = Startup.Configuration["Data:AliOSS:url"],
                    ServerType = AliOSSFileInfo.AliOssServerType,
                    Extra = Newtonsoft.Json.JsonConvert.SerializeObject(aliOssInfo)
                };

                newFire = await fireService.CreateFireRecord(newFire);
                return new
                {
                    fileId = newFire.Id.ToString(),
                    server = newFire.UploadServerUrl,
                    accessKey = newFire.Id.ToString(),
                    bucket = aliOssInfo.Bucket,
                    serverType = AliOSSFileInfo.AliOssServerType,
                    expireAt = DateTimeUtil.ToString(DateTime.UtcNow.AddDays(7))
                };
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Add New Fire Error:{0}", ex.Message);
                return HttpNotFound();
            }
        }
    }
}
