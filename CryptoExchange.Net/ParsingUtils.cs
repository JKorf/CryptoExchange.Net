using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace CryptoExchange.Net
{
    /// <summary>
    /// Parsing utility methods
    /// </summary>
    public static class ParsingUtils
    {
        /// <summary>
        /// Read the stream as string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetString(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, false, (int)stream.Length, true);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Read the stream and parse to JToken
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static JToken GetJToken(Stream x)
        {
            using var sr = new StreamReader(x, Encoding.UTF8, false, (int)x.Length, true);
            using var jsonTextReader = new JsonTextReader(sr);
            return JToken.Load(jsonTextReader);
        }
    }
}
