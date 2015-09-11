using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BahamutFire.APIServer.Controllers
{
    public class GetFileController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index(string accessKey)
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
