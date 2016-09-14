using BahamutFireCommon;
using BahamutFireCommon.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var spliteOperator = BahamutCommon.Encryption.MD5.ComputeMD5Hash(PRIMARY_KEY);
            var fileb64 = BahamutCommon.Encryption.Base64.EncodeString(fileId);
            var userb64 = BahamutCommon.Encryption.Base64.EncodeString(accessFileAccountId);
            return BahamutCommon.Encryption.Base64.EncodeString(string.Format("{0}{1}{2}", fileb64, spliteOperator,userb64));
        }

        public FireAccessInfo GetFireAccessInfoFromAccesskey(string accessKey)
        {
            var spliteOperator = BahamutCommon.Encryption.MD5.ComputeMD5Hash(PRIMARY_KEY);
            var originString = BahamutCommon.Encryption.Base64.DecodeString(accessKey);
            var v = originString.Split(new string[] { spliteOperator }, StringSplitOptions.RemoveEmptyEntries);
            return new FireAccessInfo()
            {
                AccessFileAccountId = BahamutCommon.Encryption.Base64.DecodeString(v[1]),
                FileId = BahamutCommon.Encryption.Base64.DecodeString(v[0])
            };
        }

        public bool IsAccessKeyGenerateByConverter(string accessKey)
        {
            var spliteOperator = BahamutCommon.Encryption.MD5.ComputeMD5Hash(PRIMARY_KEY);
            var originString = BahamutCommon.Encryption.Base64.DecodeString(accessKey);
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
