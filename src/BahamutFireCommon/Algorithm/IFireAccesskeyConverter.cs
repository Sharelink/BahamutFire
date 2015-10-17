using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BahamutFireCommon.Algorithm
{
    public class FireAccessInfo
    {
        public string FileId { get; set; }
        public string AccessFileAccountId { get; set; }
    }

    public interface IFireAccesskeyConverter
    {
        string ConverterName { get; }
        bool IsAccessKeyGenerateByConverter(string accessKey);
        string GenerateAccesskey(string accessFileAccountId,string fileId);
        FireAccessInfo GetFireAccessInfoFromAccesskey(string accessKey);
    }

    public interface IFireAccesskeyConverterContainer
    {
        IDictionary<string,IFireAccesskeyConverter> Converters { get; set; }
        void UseConverter<T>(T Converter) where T : IFireAccesskeyConverter;
        IFireAccesskeyConverter GetConverter(string ConverterName);
        IFireAccesskeyConverter GetConverterOfAccessKey(string accessKey);
    }

    public class DefaultFireAccesskeyConverterContainer : IFireAccesskeyConverterContainer
    {
        public DefaultFireAccesskeyConverterContainer()
        {
            Converters = new Dictionary<string, IFireAccesskeyConverter>();
        }

        public IDictionary<string, IFireAccesskeyConverter> Converters { get; set; }

        public IFireAccesskeyConverter GetConverterOfAccessKey(string accessKey)
        {
            foreach (var converter in Converters)
            {
                if (converter.Value.IsAccessKeyGenerateByConverter(accessKey))
                {
                    return converter.Value;
                }
            }
            throw new NullReferenceException("No Converter Can Decrypt");
        }

        public IFireAccesskeyConverter GetConverter(string ConverterName)
        {
            return Converters[ConverterName];
        }

        public void UseConverter<T>(T Converter) where T : IFireAccesskeyConverter
        {
            Converters.Add(Converter.ConverterName, Converter);
        }
    }
}
