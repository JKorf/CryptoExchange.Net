using System.Numerics;
using System.Text;

namespace CryptoExchange.Net.Web3
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
        public static byte[] AbiValueEncodeInt(uint value)
            => AbiValueEncodeBigInteger(false, new BigInteger(value));

        /// <summary>
        /// ABI encode ulong value as uint256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeInt(ulong value)
            => AbiValueEncodeBigInteger(false, new BigInteger(value));

        /// <summary>
        /// ABI encode big integer value as int256 or uint256, as per ABI specification
        /// </summary>
        public static byte[] AbiValueEncodeBigInteger(
            bool signed,
            BigInteger value,
            int bitWidth = 256)
        {
            if (bitWidth < 8 || bitWidth > 256 || bitWidth % 8 != 0)
                throw new ArgumentOutOfRangeException(nameof(bitWidth));

            var min = signed
                ? -(BigInteger.One << (bitWidth - 1))
                : BigInteger.Zero;

            var max = signed
                ? (BigInteger.One << (bitWidth - 1)) - 1
                : (BigInteger.One << bitWidth) - 1;

            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(nameof(value), $"Value does not fit {(signed ? "int" : "uint")}{bitWidth}.");

            var result = new byte[32];

            // Sign-extend negative values.
            if (value.Sign < 0)
            {
                for (var i = 0; i < result.Length; i++)
                    result[i] = 0xff;
            }

            // BigInteger.ToByteArray() always returns little-endian
            // two's-complement bytes, regardless of platform endianness.
            var bytes = value.ToByteArray();

            // A valid uint256 may have a 33rd, zero-valued sign byte.
            for (var i = 0; i < Math.Min(bytes.Length, result.Length); i++)
                result[result.Length - 1 - i] = bytes[i];

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
        public static byte[] AbiValueEncodeHexBytes(int length, string value)
            => AbiValueEncodeBytes(length, value.HexStringToBytes());

        /// <summary>
        /// ABI encode byte array value as bytes32, as per ABI specification. <br />
        /// The resulting bytes will be left aligned in the 32 bytes result, with trailing zeros if the byte array is shorter than 32 bytes. <br />
        /// If the byte array is longer than 32 bytes, an exception will be thrown.
        /// </summary>
        public static byte[] AbiValueEncodeBytes(int length, byte[] value)
        {
            if (length < 1 || length > 32)
                throw new ArgumentOutOfRangeException(nameof(length));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value.Length > length)
                throw new ArgumentException($"Value exceeds bytes{length}.", nameof(value));

            // Fixed bytes are left-aligned, with trailing zero padding.
            var result = new byte[32];
            Array.Copy(value, 0, result, 0, value.Length);
            return result;
        }
    }
}