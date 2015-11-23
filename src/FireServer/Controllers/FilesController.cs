using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using BahamutFireService.Service;
using BahamutFireCommon;
using System.Net;
using System.IO;
using System.Threading;

namespace FireServer.Controllers
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
            NLog.LogManager.GetCurrentClassLogger().Info("File Get");
            try
            {
                var fireService = new FireService(Startup.BahamutFireDbUrl);
                var fire = await fireService.GetFireRecord(accessKey);
                var fireId = fire.Id.ToString();
                var contentType = "application/octet-stream";
                Response.ContentLength = fire.FileSize;
                if (fire.IsSmallFile)
                {
                    return File(fire.SmallFileData, contentType);
                }
                else
                {
                    Response.ContentType = contentType;
                    return await WriteBigFileResponse(fireService, fireId);
                }
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn(ex, "AccessKey Not Found:{0}", accessKey);
                return HttpNotFound();
            }
        }

        private async Task<IActionResult> WriteBigFileResponse(FireService fireService, string fireId)
        {
            try
            {
                var gfstream = await fireService.GetBigFireStream(fireId);
                await Task.Run(async () =>
                {
                    using (var st = gfstream)
                    {

                        var count = gfstream.Length;
                        var buffer = new byte[gfstream.FileInfo.ChunkSizeBytes];

                        while (count > 0)
                        {
                            var partialCount = (int)Math.Min(buffer.Length, count);
                            await gfstream.ReadAsync(buffer, 0, partialCount, default(CancellationToken)).ConfigureAwait(false);
                            Response.StatusCode = (int)HttpStatusCode.OK;
                            await Response.Body.WriteAsync(buffer, 0, partialCount, default(CancellationToken)).ConfigureAwait(false);
                            count -= partialCount;
                        }
                    }
                });
                return Ok();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "Open Big Fire Stream Failed{0}", fireId);
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                throw;
            }
        }

        /*
        POST /Files (fileType,fileSize) : get a new send file key for upload task
        */
        [HttpPost]
        public async Task<object> PostOne(string fileType, int fileSize)
        {
            NLog.LogManager.GetCurrentClassLogger().Info("File Post");
            try
            {
                var fService = new FireService(Startup.BahamutFireDbUrl);
                var accountId = Request.Headers["accountId"];
                var newFire = new FireRecord
                {
                    CreateTime = DateTime.UtcNow,
                    FileSize = fileSize,
                    FileType = fileType,
                    IsSmallFile = fileSize < 1024 * 256,
                    State = (int)FireRecordState.Create,
                    AccountId = accountId,

                    UploadServerUrl = Startup.AppUrl + "/UploadFile",
                    AccessKeyConverter = ""
                };
                var rs = await fService.CreateFireRecord(new FireRecord[] { newFire });
                var r = rs.First();
                var fileId = r.Id.ToString();
                var accessKey = fileId;
                return new
                {
                    server = newFire.UploadServerUrl,
                    accessKey = accessKey,
                    fileId = fileId
                };
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex.Message);
                throw;
            }
        }

        // DELETE Files/
        [HttpDelete("{accessKeyIds}")]
        public async Task<long> Delete(string accessKeyIds)
        {
            var accessKeyArray = accessKeyIds.Split('#');
            var accountId = Request.Headers["accountId"];
            var fService = new FireService(Startup.BahamutFireDbUrl);
            var akService = new FireAccesskeyService();
            var infos = from ak in accessKeyArray select akService.GetFireAccessInfo(ak);
            var fileIds = from fi in infos where fi.AccessFileAccountId == accountId select fi.FileId;
            var count = await fService.DeleteFires(accountId, fileIds);
            return count;
        }
    }
}
