using BahamutFireCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using DataLevelDefines;
using MongoDB.Bson;

namespace BahamutFireService.Service
{
    public class FireService
    {
        private readonly string FireDBName = "BahamutFireDB";

        public IMongoClient Client { get; set; }

        public FireService(IMongoDbServerConfig Config)
        {
            Client = new MongoClient(new MongoUrl(Config.Url));
        }

        public byte[] GetBigFireData(string fileId)
        {
            //TODO: complete the big data
            return null;
        }

        public async Task SaveFireData(string fileId, byte[] data)
        {
            var record = await GetFireRecord(fileId);
            if (record.IsSmallFile)
            {
                var update = new UpdateDefinitionBuilder<FireRecord>().Set(fr => fr.SmallFileData, data).Set(fr => fr.State, (int)FireRecordState.Saved);
                var collection = Client.GetDatabase(FireDBName).GetCollection<FireRecord>("FireRecord");
                var fOid = new ObjectId(fileId);
                await collection.UpdateOneAsync(f => f.Id == fOid, update);
            }
            else
            {
                //TODO: complete the big data
            }
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
