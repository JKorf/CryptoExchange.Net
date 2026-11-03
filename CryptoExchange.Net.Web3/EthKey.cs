using Secp256k1Net;
using System.Text.Json.Serialization;

namespace CryptoExchange.Net.Web3
{
    /// <summary>
    /// Eth public/private key pair, used for signing messages and verifying signatures.
    /// </summary>
    public class EthKey
    {
        private byte[]? _pkBytes;

        /// <summary>
        /// Private key
        /// </summary>
        [JsonIgnore]
        public string? PrivateKey { get; }
        /// <summary>
        /// Public address
        /// </summary>
        public string PublicAddress { get; }

        /// <summary>
        /// Create a new EthKey instance
        /// </summary>
        /// <param name="publicAddress">Public address, can not sign messages without private key</param>
        /// <param name="privateKey">Private key, required for signing messages</param>
        /// <exception cref="ArgumentException"></exception>
        public EthKey(string? publicAddress = null, string? privateKey = null)
        {
            if (privateKey == null && publicAddress == null)
            {
                throw new ArgumentException(
                    "Either a private key or public address must be provided.");
            }

            if (privateKey != null)
            {
                if (!privateKey.StartsWith("0x") || privateKey.Length != 66)
                {
                    throw new ArgumentException(
                        "Private key must be a valid ETH private key, starting with 0x and 66 characters long.");
                }

                PrivateKey = privateKey;
                _pkBytes = HexToBytesString(privateKey);
            }

            if (publicAddress != null)
            {
                if (!publicAddress.StartsWith("0x") || publicAddress.Length != 42)
                {
                    throw new ArgumentException(
                        "Public address must be a valid ETH address, starting with 0x and 42 characters long.");
                }

                if (privateKey != null && !publicAddress.Equals(GetPublicAddress(privateKey), StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(
                        "Private key does not match the public address.");
                }

                PublicAddress = publicAddress;
            }
            else
            {
                PublicAddress = GetPublicAddress(privateKey!);
            }
        }

        /// <summary>
        /// Create new Eth credentials with only a public key. This is generally only sufficient for verifying signatures and read operations. <br />
        /// Request signing will not be possible without a private key.
        /// </summary>
        /// <param name="publicAddress">The public wallet address</param>
        public static EthKey FromPublicAddress(string publicAddress)
        {
            return new EthKey(publicAddress, null);
        }

        /// <summary>
        /// Create new Eth credentials from a private key. The public key will be derived from the private key. Allows signing of messages.
        /// </summary>
        /// <param name="privateKey">The private key for the wallet</param>
        public static EthKey FromPrivateKey(string privateKey)
        {
            return new EthKey(null, privateKey);
        }

        /// <summary>
        /// Sign a hash with the private key, returning the signature in rsv format
        /// </summary>
        public Dictionary<string, object> SignHashRsv(byte[] request)
        {
            if (_pkBytes == null)
                throw new InvalidOperationException("Private key not provided, cannot sign hash");

            (var signature, var recover) = Secp256k1.SignRecoverable(request, _pkBytes);
            var hexCompactR = BytesToHexString(new ArraySegment<byte>(signature, 0, 32));
            var hexCompactS = BytesToHexString(new ArraySegment<byte>(signature, 32, 32));
            var hexCompactV = recover + 27;

            return new Dictionary<string, object>
            {
                { "r", "0x" + hexCompactR },
                { "s", "0x" + hexCompactS },
                { "v", hexCompactV },
            };
        }

        /// <summary>
        /// Sign a hash with the private key, returning the signature in signature format
        /// </summary>
        public string SignHash(byte[] request)
        {
            if (_pkBytes == null)
                throw new InvalidOperationException("Private key not provided, cannot sign hash");

            (var signature, var recover) = Secp256k1.SignRecoverable(request, _pkBytes);
            var hexCompactR = BytesToHexString(new ArraySegment<byte>(signature, 0, 32));
            var hexCompactS = BytesToHexString(new ArraySegment<byte>(signature, 32, 32));
            var hexCompactV = BytesToHexString([(byte)(recover + 27)]);

            var result = "0x" + hexCompactR.PadLeft(64, '0') +
                   hexCompactS.PadLeft(64, '0') +
                   hexCompactV;
            return result;
        }

        /// <inheritdoc />
        public override string ToString() => $"EthKey(PublicAddress={PublicAddress})";
        
        private static string GetPublicAddress(string privateKey)
        {
            var publicKeyBytes = Secp256k1.CreatePublicKey(ExchangeHelpers.HexToBytesString(privateKey), false);

            var withoutPrefix = new byte[64];
            Array.Copy(publicKeyBytes, 1, withoutPrefix, 0, 64);

            var hash = CeSha3Keccack.CalculateHash(withoutPrefix);
            var pubAddress = new byte[20];
            Array.Copy(hash, hash.Length - 20, pubAddress, 0, 20);

            return "0x" + ExchangeHelpers.BytesToHexString(pubAddress).ToLower();
        }


        private static byte[] HexToBytesString(string hexString)
        {
            if (hexString.StartsWith("0x"))
                hexString = hexString.Substring(2);

            byte[] array = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                string value = hexString.Substring(i, 2);
                array[i / 2] = Convert.ToByte(value, 16);
            }

            return array;
        }

        private static string BytesToHexString(IEnumerable<byte> buff)
        {
            string text = string.Empty;
            foreach (byte item in buff)
                text += item.ToString("X2");
            return text;
        }
    }
}
