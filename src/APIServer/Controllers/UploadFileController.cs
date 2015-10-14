﻿using System;
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
        public IActionResult Index()
        {
            string accessKey = Context.Request.Headers["accessKey"];
            var fireService = new FireService(Startup.BahamutFireDbConfig);
            var akService = new FireAccesskeyService();
            var info = akService.GetFireAccessInfo(accessKey);
            
            var accountId = Context.Request.Headers["accountId"];
            if (info.AccessFileAccountId != accountId)
            {
                return HttpUnauthorized();
            }

            var result = Task.Run(async () =>
            {
                var fireRecord = await fireService.GetFireRecord(info.FileId);
                var reader = new BinaryReader(Context.Request.Body);
                var data = reader.ReadBytes(fireRecord.FileSize);
                if (data.Length == fireRecord.FileSize)
                {
                    await fireService.SaveFireData(info.FileId, data);
                    Console.WriteLine("Fire Save");
                    return true;
                }
                else
                {
                    return false;
                }
            }).Result;
            if (result)
            {
                return Json("upload success");
            }
            else
            {
                return HttpBadRequest();
            }
        }
    }
}
