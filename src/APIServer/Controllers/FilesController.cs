using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;
using BahamutFireCommon;
using System.Net;

namespace BahamutFire.APIServer.Controllers
{
    [Route("[controller]")]
    public class FilesController : Controller
    {


        /*
        GET /Files/{id} : get a new send file key for upload task
        */
        [HttpGet("{accessKey}")]
        public async Task<IActionResult> Get(string accessKey)
        {
            var fireService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            try
            {
                var info = akService.GetFireAccessInfo(accessKey);
                if (info.AccessFileAccountId != Request.Headers["accountId"])
                {
                    return HttpBadRequest();
                }
                var fire = await fireService.GetFireRecord(info.FileId);
                if (fire.IsSmallFile)
                {
                    Response.ContentLength = fire.FileSize;
                    return File(fire.SmallFileData, "application/octet-stream");
                }
                else
                {
                    var routeValues = new { accessKey = accessKey };
                    return RedirectToAction("Index", "GetFile", routeValues);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return HttpNotFound();
            }
        }

        /*
        POST /Files (fileType,fileSize) : get a new send file key for upload task
        */
        [HttpPost]
        public async Task<object> PostOne(string fileType, int fileSize)
        {
            var akService = new FireAccesskeyService();
            var fService = new FireService(Startup.BahamutFireDbConfig);
            var accountId = Request.Headers["accountId"];
            var newFire = new FireRecord
            {
                CreateTime = DateTime.UtcNow,
                FileSize = fileSize,
                FileType = fileType,
                IsSmallFile = fileSize < 1024 * 1024 * 7,
                State = (int)FireRecordState.Create,
                AccountId = accountId,
                
                UploadServerUrl = Startup.AppUrl + "/UploadFile",
                AccessKeyConverter = akService.DefaultConverterName
            };
            var rs = await fService.CreateFireRecord(new FireRecord[] { newFire });
            var r = rs.First();
            var fileId = r.Id.ToString();
            var accessKey = akService.GetAccesskey(newFire.AccessKeyConverter, accountId, fileId);
            return new
            {
                server = newFire.UploadServerUrl,
                accessKey = accessKey,
                fileId = fileId
            };

        }

        // DELETE Files/
        [HttpDelete("{accessKeyIds}")]
        public async Task<long> Delete(string accessKeyIds)
        {
            var accessKeyArray = accessKeyIds.Split('#');
            var accountId = Request.Headers["accountId"];
            var fService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            var infos = from ak in accessKeyArray select akService.GetFireAccessInfo(ak);
            var fileIds = from fi in infos where fi.AccessFileAccountId == accountId select fi.FileId;
            var count = await fService.DeleteFires(accountId, fileIds);
            return count;
        }
    }
}
