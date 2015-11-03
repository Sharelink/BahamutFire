using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;
using System.IO;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BahamutFire.APIServer.Controllers
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
            var reader = new BinaryReader(Request.Body);
            var data = reader.ReadBytes(fireRecord.FileSize);
            if (data.Length == fireRecord.FileSize)
            {
                await fireService.SaveFireData(fireRecord.Id.ToString(), data);
                Console.WriteLine("Fire Save");
                return Json(new { fileId = fireRecord.Id.ToString() });
            }
            else
            {
                return HttpBadRequest();
            }
        }
    }
}
