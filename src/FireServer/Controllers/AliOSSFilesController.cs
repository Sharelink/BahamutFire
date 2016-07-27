using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BahamutFireService.Service;
using BahamutFireCommon;
using BahamutCommon;
using System.Net;

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
        private object FireRecordToObject(FireRecord newFire,string bucket)
        {
            var fireObj = new
            {
                fileId = newFire.Id.ToString(),
                server = newFire.UploadServerUrl,
                accessKey = newFire.Id.ToString(),
                bucket = bucket,
                serverType = AliOSSFileInfo.AliOssServerType,
                expireAt = DateTimeUtil.ToString(DateTime.UtcNow.AddDays(7))
            };
            return fireObj;
        }

        private FireRecord GenerateNewFireRecord(string fileType,int fileSize,string accountId,string uploadUrl,string bucket)
        {
            var aliOssInfo = new AliOSSFileInfo()
            {
                Bucket = bucket
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
                UploadServerUrl = uploadUrl,
                ServerType = AliOSSFileInfo.AliOssServerType,
                Extra = Newtonsoft.Json.JsonConvert.SerializeObject(aliOssInfo)
            };
            return newFire;
        }

        [HttpPost("List")]
        public async Task<object> PostFileList(string fileTypes, string fileSizes)
        {
            try
            {
                var appkey = HttpContext.Request.Headers["appkey"];
                var bucketKey = string.Format("Data:AliOSS:{0}:bucket", appkey);
                var uploadUrlKey = string.Format("Data:AliOSS:{0}:url", appkey);
                var bucket = Startup.Configuration[bucketKey];
                var uploadUrl = Startup.Configuration[uploadUrlKey];

                var fireService = new FireService(Startup.BahamutFireDbUrl);
                var accountId = Request.Headers["accountId"];

                var fileTypeList = fileTypes.Split('#');
                var fileSizeList = fileSizes.Split('#');
                if (fileTypeList.Count() != fileSizeList.Count())
                {
                    throw new Exception("New Fire Paramenters Not Match");
                }
                var newFires = new List<FireRecord>();
                for (int i = 0; i < fileTypeList.Count(); i++)
                {
                    var fire = GenerateNewFireRecord(fileTypeList[i], int.Parse(fileSizeList[i]), accountId, uploadUrl, bucket);
                    newFires.Add(fire);
                }
                var records = await fireService.CreateFireRecords(newFires);
                var result = new
                {
                    files = from r in records select FireRecordToObject(r, bucket)
                };
                return result;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetLogger("Warn").Warn(ex, "Add New Fires Error:{0}", ex.Message);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "Bad Request";
            }
        }

        [HttpPost]
        public async Task<object> Post(string fileType, int fileSize)
        {
            try
            {
                var appkey = HttpContext.Request.Headers["appkey"];
                var bucketKey = string.Format("Data:AliOSS:{0}:bucket", appkey);
                var uploadUrlKey = string.Format("Data:AliOSS:{0}:url", appkey);
                var bucket = Startup.Configuration[bucketKey];
                var uploadUrl = Startup.Configuration[uploadUrlKey];
                var fireService = new FireService(Startup.BahamutFireDbUrl);
                var accountId = Request.Headers["accountId"];
                var newFire = GenerateNewFireRecord(fileType, fileSize, accountId, uploadUrl, bucket);
                newFire = await fireService.CreateFireRecord(newFire);
                return FireRecordToObject(newFire, bucket);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetLogger("Warn").Warn(ex, "Add New Fire Error:{0}", ex.Message);
                Response.StatusCode = 400;
                return new { msg = "CREATE_FILE_ERROR" };
            }
        }
    }
}
