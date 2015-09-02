using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BahamutFireCommon.Algorithm
{
    public class FireAccessInfo
    {
        public string FileId { get; set; }
        public string AccessFileUserId { get; set; }
    }

    public interface IFireAccesskeyConverter
    {
        string GenerateAccesskey(string accessFileUserId,FireRecord file);
        string GetFireAccessInfoFromAccesskey(string accessKey);
    }

    public interface IFireAccesskeyConverterContainer
    {
        IDictionary<string,IFireAccesskeyConverter> Converters { get; set; }
        void UseConverter<T>(T Converter) where T : IFireAccesskeyConverter;
        IFireAccesskeyConverter GetConverter(string ConverterName);
    }

    public class DefaultFireAccesskeyConverterContainer : IFireAccesskeyConverterContainer
    {
        public DefaultFireAccesskeyConverterContainer()
        {
            Converters = new Dictionary<string, IFireAccesskeyConverter>();
        }

        public IDictionary<string, IFireAccesskeyConverter> Converters { get; set; }

        public IFireAccesskeyConverter GetConverter(string ConverterName)
        {
            return Converters[ConverterName];
        }

        public void UseConverter<T>(T Converter) where T : IFireAccesskeyConverter
        {
            Converters.Add(typeof(T).ToString(), Converter);
        }
    }
}
