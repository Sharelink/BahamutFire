using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BahamutFire.APIServer.Controllers
{
    [Route("[controller]")]
    public class GetFileController : Controller
    {

        // GET: /<controller>/
        [HttpGet]
        public async Task<IActionResult> Index(string accessKey)
        {
            var fireService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            try
            {
                var info = akService.GetFireAccessInfo(accessKey);
                var fire = await fireService.GetFireRecord(info.FileId);

                if (fire.IsSmallFile)
                {
                    Response.ContentLength = fire.FileSize;
                    return File(fire.SmallFileData, fire.FileType);
                }
                else
                {
                    Response.ContentLength = fire.FileSize;
                    return File(fireService.GetBigFireData(fire.Id.ToString()), fire.FileType);
                }
            }
            catch (Exception)
            {
                return HttpNotFound();
            }
        }
    }
}
