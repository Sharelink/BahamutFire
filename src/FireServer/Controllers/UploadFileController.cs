using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;
using System.IO;
using MongoDB.Driver.GridFS;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BahamutFire.FireServer.Controllers
{
    [Route("[controller]")]
    public class UploadFileController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Index()
        {
            string accessKey = Request.Headers["accessKey"];
            var fireService = new FireService(Startup.BahamutFireDbUrl);
            var accountId = Request.Headers["accountId"];
            var fireRecord = await fireService.GetFireRecord(accessKey);
            var fileName = fireRecord.Id.ToString();
            if (fireRecord.IsSmallFile)
            {
                var reader = new BinaryReader(Request.Body);
                var data = reader.ReadBytes(fireRecord.FileSize);
                if (data.Length == fireRecord.FileSize)
                {
                    await fireService.SaveSmallFire(fireRecord.Id.ToString(), data);
#if DEBUG
                    Console.WriteLine("Small Fire Save");
#endif
                }
                else
                {
                    return HttpBadRequest();
                }
            }
            else
            {
                var newBigFireId = await fireService.SaveBigFire(fileName, Request.Body);
                await fireService.UpdateBigFireId(fileName, newBigFireId);
#if DEBUG
                Console.WriteLine("Big Fire Save");
#endif
            }
            
            return Json(new { fileId = fireRecord.Id.ToString() });
        }
    }
}
