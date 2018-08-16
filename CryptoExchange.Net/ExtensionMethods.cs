using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace CryptoExchange.Net
{
    public static class ExtensionMethods
    {
        public static void AddParameter(this Dictionary<string, object> parameters, string key, string value)
        {
            parameters.Add(key, value);
        }

        public static void AddParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            parameters.Add(key, value);
        }

        public static void AddOptionalParameter(this Dictionary<string, object> parameters, string key, object value)
        {
            if(value != null)
                parameters.Add(key, value);
        }

        public static void AddOptionalParameter(this Dictionary<string, string> parameters, string key, string value)
        {
            if (value != null)
                parameters.Add(key, value);
        }

        public static string GetString(this SecureString source)
        {
            lock (source)
            {
                string result = null;
                int length = source.Length;
                IntPtr pointer = IntPtr.Zero;
                char[] chars = new char[length];

                try
                {
                    pointer = Marshal.SecureStringToBSTR(source);
                    Marshal.Copy(pointer, chars, 0, length);

                    result = string.Join("", chars);
                }
                finally
                {
                    if (pointer != IntPtr.Zero)
                    {
                        Marshal.ZeroFreeBSTR(pointer);
                    }
                }

                return result;
            }
        }
    }
}
