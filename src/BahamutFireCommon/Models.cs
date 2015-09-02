using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BahamutFireCommon
{
    public class FireRecord
    {
        public string FileId { get; set; }
        public int FileSize { get; set; }
        public DateTime CreateTime { get; set; }
        public string State { get; set; }
        public bool IsSmallFile { get; set; }
        public string FileType { get; set; }
        public string AccessKeyConverter { get; set; }
        public byte[] SmallFileData { get; set; }
    }
}
