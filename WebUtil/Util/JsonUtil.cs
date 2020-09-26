using Newtonsoft.Json;

namespace WebUtil.Util
{
    public static class JsonUtil
    {
        public static Target ConvertTo<Target, Source>(this Source source) where Source : class
        {
            var deserialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<Target>(deserialized);
        }
    }
}
