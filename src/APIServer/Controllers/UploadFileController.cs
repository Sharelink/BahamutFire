using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BahamutFire.APIServer.Controllers
{
    public class UploadFileController : Controller
    {
        [HttpPost]       
        public IActionResult Index([FromForm]string accessKey,byte[] fileData)
        {
            var accountId = Context.Request.Headers["accountId"];
            var fireService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            var info = akService.GetFireAccessInfo(accessKey);
            if(info.AccessFileAccountId == accessKey)
            {
                fireService.SaveFireData(info.FileId, fileData);
                return Json(new { Message = "upload success" });
            }
            else
            {
                return HttpUnauthorized();
            }
        }
    }
}
