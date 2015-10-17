using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BahamutFireCommon
{
    public enum FireRecordState: int
    {
        Create = 0,
        Saved = 2,
        Delete = 3
    }

    public class FireRecord
    {
        public ObjectId Id { get; set; }
        public string AccountId { get; set; }
        public int FileSize { get; set; }
        public DateTime CreateTime { get; set; }
        public int State { get; set; }
        public bool IsSmallFile { get; set; }
        public string FileType { get; set; }
        public string AccessKeyConverter { get; set; }
        public byte[] SmallFileData { get; set; }
        public string UploadServerUrl { get; set; }
    }

}
