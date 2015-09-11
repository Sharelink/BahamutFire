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
        public IActionResult Get(string accessKey)
        {
            var fireService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            try
            {
                var info = akService.GetFireAccessInfo(accessKey);
                var fire = fireService.GetFireRecord(info.FileId);
                if (fire.IsSmallFile)
                {
                    return File(fire.SmallFileData, fire.FileType);
                }
                else
                {
                    var routeValues = new { accessKey = accessKey };
                    return RedirectToAction("Index", "GetFile",routeValues);
                }
            }
            catch (Exception)
            {
                return HttpNotFound();   
            }
        }

        /*
        POST /Files (fileType,fileSize) : get a new send file key for upload task
        */
        [HttpPost]
        public SendFileTask PostOne([FromBody]string fileType, int fileSize)
        {
            var akService = new FireAccesskeyService();
            var fService = new FireService(Startup.BahamutFireDbConfig);
            var accountId = Context.Request.Headers["accountId"];
            var newFireRecords = new FireRecord[] 
            {
                new FireRecord
                {
                    CreateTime = DateTime.Now,
                    FileSize = fileSize,
                    FileType = fileType,
                    IsSmallFile = fileSize < 1024 * 1027 * 7,
                    State = (int)FireRecordState.Create,
                    AccountId = accountId
                }
            };

            var r = fService.CreateFireRecord(newFireRecords).ElementAt(0);
            var result = new SendFileTask()
            {
                acceptServerUrl = r.UploadServerUrl,
                accessKey = akService.GetAccesskey(accountId, r)
            };
            return result;

        }

        /*
        POST /Files (fileType,fileSize) : get a new send file key for upload task
        */
        [HttpPost("More")]
        public SendFileTask[] PostMore([FromBody]string fileTypes,string fileSizes)
        {
            var akService = new FireAccesskeyService();
            var fService = new FireService(Startup.BahamutFireDbConfig);
            var accountId = Context.Request.Headers["accountId"];
            var sizes = from fs in fileSizes.Split('#') select int.Parse(fs);
            var types = fileTypes.Split('#');
            if (types.Count() != sizes.Count())
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
            var length = fileTypes.Count();
            var newFireRecords = new FireRecord[length];
            for (int i = 0; i < length; i++)
            {
                var fileSize = sizes.ElementAt(i);
                newFireRecords[i] = new FireRecord
                {
                    CreateTime = DateTime.Now,
                    FileSize = fileSize,
                    FileType = types.ElementAt(i),
                    IsSmallFile = fileSize < 1024 * 1027 * 7,
                    State = (int)FireRecordState.Create,
                    AccountId = accountId
                };
            }

            var records = fService.CreateFireRecord(newFireRecords);
            var tasks = from r in records
                        select new SendFileTask()
                        {
                            acceptServerUrl = r.UploadServerUrl,
                            accessKey = akService.GetAccesskey(accountId, r)
                        };
            return tasks.ToArray();

        }

        // DELETE Files/
        [HttpDelete("{accessKeyIds}")]
        public long Delete(string accessKeyIds)
        {
            var accessKeyArray = accessKeyIds.Split('#');
            var accountId = Context.Request.Headers["accountId"];
            var fService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            var infos = from ak in accessKeyArray select akService.GetFireAccessInfo(ak);
            var fileIds = from fi in infos where fi.AccessFileAccountId == accountId select fi.FileId;
            var count = fService.DeleteFires(accountId, fileIds);
            return count;
        }
    }
}
