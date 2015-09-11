using BahamutFireCommon;
using BahamutFireCommon.Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BahamutFireService.Service
{
    public class FireAccesskeyService
    {
        public IFireAccesskeyConverterContainer ConverterContainer { get; set; }
        public string DefaultConverterName { get; set; }
        public FireAccesskeyService()
        {
            ConverterContainer = new DefaultFireAccesskeyConverterContainer();
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
