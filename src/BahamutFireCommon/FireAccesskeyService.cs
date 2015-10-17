using BahamutFireCommon;
using BahamutFireCommon.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBTek.Crypto;

namespace BahamutFireService.Service
{
    class AccessKeyConverter : IFireAccesskeyConverter
    {
        public const string PRIMARY_KEY = "SharelinkGreat";

        public string ConverterName
        {
            get
            {
                return "default150923";
            }
        }

        public string GenerateAccesskey(string accessFileAccountId, string fileId)
        {
            var b64 = new Base64();
            var md5 = new MD5_Hsr();
            var spliteOperator = md5.HashString(PRIMARY_KEY);
            var fileb64 = b64.EncodeString(fileId);
            var userb64 = b64.EncodeString(accessFileAccountId);
            return b64.EncodeString(string.Format("{0}{1}{2}", fileb64, spliteOperator,userb64));
        }

        public FireAccessInfo GetFireAccessInfoFromAccesskey(string accessKey)
        {
            var b64 = new Base64();
            var md5 = new MD5_Hsr();
            var spliteOperator = md5.HashString(PRIMARY_KEY);
            var originString = b64.DecodeString(accessKey);
            var v = originString.Split(new string[] { spliteOperator }, StringSplitOptions.RemoveEmptyEntries);
            return new FireAccessInfo()
            {
                AccessFileAccountId = b64.DecodeString(v[1]),
                FileId = b64.DecodeString(v[0])
            };
        }

        public bool IsAccessKeyGenerateByConverter(string accessKey)
        {
            var md5 = new MD5_Hsr();
            var b64 = new Base64();
            var spliteOperator = md5.HashString(PRIMARY_KEY);
            var originString = b64.DecodeString(accessKey);
            return originString.Contains(spliteOperator);
        }
    }

    public class FireAccesskeyService
    {
        public IFireAccesskeyConverterContainer ConverterContainer { get; set; }
        public string DefaultConverterName { get; set; }
        public FireAccesskeyService()
        {
            ConverterContainer = new DefaultFireAccesskeyConverterContainer();
            var defaultConverter = new AccessKeyConverter();
            ConverterContainer.UseConverter(defaultConverter);
            DefaultConverterName = defaultConverter.ConverterName;
        }

        public string GetAccessKeyUseDefaultConverter(string accessFireAccountId,string fileId)
        {
            if (fileId == null)
            {
                return null;
            }
            return GetAccesskey(DefaultConverterName, accessFireAccountId, fileId);
        }

        public string GetAccesskey(string converterName,string accessFileAccountId, string fileId)
        {
            if (fileId == null)
            {
                return null;
            }
            IFireAccesskeyConverter converter = ConverterContainer.GetConverter(converterName);
            return converter.GenerateAccesskey(accessFileAccountId, fileId);
        }

        public IEnumerable<string> GetAccesskeys(string accessFileAccountId,ICollection<FireRecord> files)
        {
            return from f in files select GetAccesskey(f.AccessKeyConverter, accessFileAccountId, f.Id.ToString());
        }

        public FireAccessInfo GetFireAccessInfo(string accessKey)
        {
            try
            {
                return ConverterContainer.GetConverterOfAccessKey(accessKey).GetFireAccessInfoFromAccesskey(accessKey);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
