using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace CryptoExchange.Net.Authentication.Signing
{
    /// <summary>
    /// ABI encoding
    /// </summary>
    public static class CeAbiEncoder
    {
        /// <summary>
        /// ABI encode string as Sha3Keccack hashed byte value
        /// </summary>
        public static byte[] AbiValueEncodeString(string value)
        {
            var abiValueEncoded = CeSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(value));
            return abiValueEncoded;
        }

        /// <summary>
        /// ABI encode bool value as uint256 with 1 for true and 0 for false, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeBool(bool value)
            => AbiValueEncodeInt((byte)(value ? 1 : 0));

        /// <summary>
        /// ABI encode byte value as uint256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeInt(byte value)
            => AbiValueEncodeBigInteger(false, new BigInteger(value));

        /// <summary>
        /// ABI encode short value as int256, as per ABI specification  
        /// </summary>
        public static byte[] AbiValueEncodeInt(short value)
            => AbiValueEncodeBigInteger(true, new BigInteger(value));

        /// <summary>
        /// ABI encode int value as int256, as per ABI specification
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] AbiValueEncodeInt(int value)
            => AbiValueEncodeBigInteger(true, new BigInteger(value));

        /// <summary>
        /// ABI encode long value as int256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeInt(long value)
            => AbiValueEncodeBigInteger(true, new BigInteger(value));

        /// <summary>
        /// ABI encode ushort value as uint256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeInt(ushort value)
            => AbiValueEncodeBigInteger(false, new BigInteger(value));

        /// <summary>
        /// ABI encode uint value as uint256, as per ABI specification
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] AbiValueEncodeInt(uint value)
            => AbiValueEncodeBigInteger(false, new BigInteger(value));

        /// <summary>
        /// ABI encode ulong value as uint256, as per ABI specification
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] AbiValueEncodeInt(ulong value)
            => AbiValueEncodeBigInteger(false, new BigInteger(value));

        /// <summary>
        /// ABI encode big integer value as int256 or uint256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeBigInteger(bool signed, BigInteger value)
        {
            var result = new byte[32];
            if (signed && value < 0)
            {
                // Pad with FF
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = 0xFF;
                }
            }

            var t = value.ToByteArray();
            if (t.Length == 33)
            {
                // Strip last byte
                var strip1 = new byte[32];
                Array.Copy(t, 0, strip1, 0, 32);
                t = strip1;
            }

            if (BitConverter.IsLittleEndian)
                t = t.AsEnumerable().Reverse().ToArray();

            t.CopyTo(result, result.Length - t.Length);
            return result;
        }

        /// <summary>
        /// ABI encode address value as uint256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeAddress(string value)
        {
            var result = new byte[32];
            var h = value.HexStringToBytes();
            h.CopyTo(result, result.Length - h.Length);
            return result;
        }

        /// <summary>
        /// ABI encode hex string value as bytes32, as per ABI specification. The hex string is expected to be a 0x prefixed string, and the resulting bytes will be right aligned in the 32 bytes result, with leading zeros if the hex string is shorter than 32 bytes. If the hex string is longer than 32 bytes, an exception will be thrown.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] AbiValueEncodeHexBytes(int length, string value)
            => AbiValueEncodeBytes(value.Length, value.HexStringToBytes());

        /// <summary>
        /// ABI encode byte array value as bytes32, as per ABI specification. The resulting bytes will be right aligned in the 32 bytes result, with leading zeros if the byte array is shorter than 32 bytes. If the byte array is longer than 32 bytes, an exception will be thrown.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static byte[] AbiValueEncodeBytes(int length, byte[] value)
        {
            if (length != 32)
                throw new Exception("Only 32 bytes size supported");

            if (value.Length == 32)
                return value;

            var result = new byte[32];
            value.CopyTo(result, result.Length - value.Length);
            return result;
        }
    }
}
