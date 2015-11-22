using BahamutFireCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using System.IO;
using MongoDB.Driver.GridFS;

namespace BahamutFireService.Service
{
    public class FireService
    {
        private readonly string FireDBName = "BahamutFireDB";
        private readonly string BigDataFireDBName = "BahamutBigFireDB";

        public IMongoClient Client { get; set; }

        public FireService(string mongoDbUrl)
        {
            Client = new MongoClient(new MongoUrl(mongoDbUrl));
        }

        public async Task<GridFSFileInfo> GetBigFire(string fileId)
        {
            var bucket = new GridFSBucket(Client.GetDatabase(BigDataFireDBName));
            var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Filename, fileId);
            var fires = await (await bucket.FindAsync(filter)).ToListAsync();
            return fires.First();
        }

        public async Task<GridFSDownloadStream> GetBigFireStream(string fileId)
        {
            var bucket = new GridFSBucket(Client.GetDatabase(BigDataFireDBName));
            var fireStream = await bucket.OpenDownloadStreamByNameAsync(fileId);
            return fireStream;
        }

        public async Task<ObjectId> SaveBigFire(string fileName, Stream fireStream)
        {
            var bucket = new GridFSBucket(Client.GetDatabase(BigDataFireDBName));
            return await bucket.UploadFromStreamAsync(fileName, fireStream);
        }

        public async Task UpdateBigFireId(string fileId, ObjectId bigFireId)
        {
            var update = Builders<FireRecord>.Update.Set(fr => fr.BigFireId, bigFireId);
            var collection = Client.GetDatabase(FireDBName).GetCollection<FireRecord>("FireRecord");
            var fOid = new ObjectId(fileId);
            await collection.UpdateOneAsync(f => f.Id == fOid, update);
        }

        public async Task SaveSmallFire(string fileId, byte[] data)
        {
            var update = new UpdateDefinitionBuilder<FireRecord>().Set(fr => fr.SmallFileData, data).Set(fr => fr.State, (int)FireRecordState.Saved);
            var collection = Client.GetDatabase(FireDBName).GetCollection<FireRecord>("FireRecord");
            var fOid = new ObjectId(fileId);
            await collection.UpdateOneAsync(f => f.Id == fOid, update);
        }

        public async Task<FireRecord> GetFireRecord(string fileId)
        {
            var collection = Client.GetDatabase(FireDBName).GetCollection<FireRecord>("FireRecord");
            var oId = new ObjectId(fileId);
            return await collection.Find(fr => fr.Id == oId).SingleAsync();
        }

        public async Task<IEnumerable<FireRecord>> CreateFireRecord(IEnumerable<FireRecord> newFireRecords)
        {
            await Client.GetDatabase(FireDBName).GetCollection<FireRecord>("FireRecord").InsertManyAsync(newFireRecords);
            return newFireRecords;
        }

        public async Task<long> DeleteFires(string accountId, IEnumerable<string> fileIds)
        {
            var update = new UpdateDefinitionBuilder<FireRecord>().Set(fr => fr.State, (int)FireRecordState.Delete);
            var result = await Client.GetDatabase(FireDBName).GetCollection<FireRecord>("FireRecord").UpdateManyAsync(fr => fr.AccountId == accountId && fileIds.Contains(fr.Id.ToString()), update);
            return result.ModifiedCount;
        }
    }
}
