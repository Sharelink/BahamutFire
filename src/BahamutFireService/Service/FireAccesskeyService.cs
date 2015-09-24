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

        public string GenerateAccesskey(string accessFileUserId, FireRecord file)
        {
            var fileId = file.Id.ToString();
            var b64 = new Base64();
            var md5 = new MD5_Hsr();
            var spliteOperator = md5.HashString(PRIMARY_KEY);
            var fileb64 = b64.EncodeString(fileId);
            var userb64 = b64.EncodeString(accessFileUserId);
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

        public string GetAccesskey(string accessFileAccountId, FireRecord file)
        {
            IFireAccesskeyConverter converter = ConverterContainer.GetConverter(file.AccessKeyConverter);
            return converter.GenerateAccesskey(accessFileAccountId, file);
        }

        public IEnumerable<string> GetAccesskeys(string accessFileAccountId,ICollection<FireRecord> files)
        {
            return from f in files select GetAccesskey(accessFileAccountId, f);
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
