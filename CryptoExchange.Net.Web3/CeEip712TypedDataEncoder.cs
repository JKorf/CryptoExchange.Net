using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptoExchange.Net.Web3
{
    /// <summary>
    /// EIP712 Typed Data Encoder
    /// </summary>
    public static class CeEip712TypedDataEncoder
    {
        /// <summary>
        /// Encode EIP712 typed data according to the specification, with the provided primary type, domain fields and message fields.
        /// The resulting byte array is the 0x19 0x01 prefix followed by the hash of the domain and the hash of the message, which can be signed with ECDsa secp256k1 to produce a signature that can be verified on chain with EIP712.
        /// Note that this implementation does not support all possible EIP712 types, but it should cover most common use cases
        /// </summary>
        public static byte[] EncodeEip712(
            string primaryType,
            IEnumerable<(string Name, string Type, object Value)> domainFields,
            IEnumerable<(string Name, string Type, object Value)> messageFields)
        {
            var data = new CeTypedDataRaw()
            {
                PrimaryType = primaryType,
                DomainRawValues = domainFields.Select(x => new CeMemberValue
                {
                    TypeName = x.Type,
                    Value = x.Value,
                }).ToArray(),

                Message = messageFields.Select(x => new CeMemberValue
                {
                    TypeName = x.Type,
                    Value = x.Value,
                }).ToArray(),
                Types = new Dictionary<string, CeMemberDescription[]>
                {
                    {
                        "EIP712Domain",
                        domainFields.Select(x => new CeMemberDescription
                        {
                            Name = x.Name,
                            Type = x.Type
                        }).ToArray()
                    },
                    {
                        primaryType,
                        messageFields.Select(x => new CeMemberDescription
                        {
                            Name = x.Name,
                            Type = x.Type
                        }).ToArray()
                    }
                }
            };

            return EncodeTypedDataRaw(data);
        }

        /// <summary>
        /// Encode EIP712 typed data according to the specification, with the provided primary type, domain fields and message fields.
        /// The resulting byte array is the 0x19 0x01 prefix followed by the hash of the domain and the hash of the message, which can be signed with ECDsa secp256k1 to produce a signature that can be verified on chain with EIP712.
        /// Note that this implementation does not support all possible EIP712 types, but it should cover most common use cases
        /// </summary>
        public static byte[] EncodeTypedDataRaw(CeTypedDataRaw typedData)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            // Write 0x19 0x01 prefix
            writer.Write((byte)0x19);
            writer.Write((byte)0x01);

            // Write domain
            writer.Write(HashStruct(typedData.Types, "EIP712Domain", typedData.DomainRawValues));

            // Write message
            writer.Write(HashStruct(typedData.Types, typedData.PrimaryType, typedData.Message));

            writer.Flush();
            var result = memoryStream.ToArray();
            return result;

        }

        private static byte[] HashStruct(IDictionary<string, CeMemberDescription[]> types, string primaryType, IEnumerable<CeMemberValue> message)
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);

            // Encode the type header
            EncodeType(writer, types, primaryType);

            // Encode the data
            EncodeData(writer, types, message);

            writer.Flush();
            return CeSha3Keccack.CalculateHash(memoryStream.ToArray());

        }

        private static void EncodeData(BinaryWriter writer, IDictionary<string, CeMemberDescription[]> types, IEnumerable<CeMemberValue> memberValues)
        {
            foreach (var memberValue in memberValues)
            {
                switch (memberValue.TypeName)
                {
                    case var refType when IsReferenceType(refType):
                        writer.Write(HashStruct(types, memberValue.TypeName, (IEnumerable<CeMemberValue>)memberValue.Value));
                        break;

                    case "string":
                        writer.Write(CeAbiEncoder.AbiValueEncodeString((string)memberValue.Value));
                        break;

                    case "bool":
                        writer.Write(CeAbiEncoder.AbiValueEncodeBool((bool)memberValue.Value));
                        break;

                    case "address":
                        writer.Write(CeAbiEncoder.AbiValueEncodeAddress((string)memberValue.Value));
                        break;

                    default:
                        if (memberValue.TypeName.Contains("["))
                        {
                            var items = (IList)memberValue.Value;
                            var itemsMemberValues = new List<CeMemberValue>();
                            foreach (var item in items)
                            {
                                itemsMemberValues.Add(new CeMemberValue()
                                {
                                    TypeName = memberValue.TypeName.Substring(0, memberValue.TypeName.LastIndexOf("[")),
                                    Value = item
                                });
                            }

                            var memoryStream = new MemoryStream();
                            var writerItem = new BinaryWriter(memoryStream);

                            EncodeData(writerItem, types, itemsMemberValues);
                            writerItem.Flush();
                            writer.Write(CeSha3Keccack.CalculateHash(memoryStream.ToArray()));
                        }
                        else if (memberValue.TypeName.StartsWith("int", StringComparison.Ordinal) ||
                                 memberValue.TypeName.StartsWith("uint", StringComparison.Ordinal))
                        {
                            var signed = memberValue.TypeName[0] != 'u';
                            var widthText = memberValue.TypeName.Substring(signed ? 3 : 4);

                            if (!int.TryParse( widthText, NumberStyles.None, CultureInfo.InvariantCulture, out var bitWidth))
                                throw new ArgumentException($"Invalid integer type: {memberValue.TypeName}");

                            var value = memberValue.Value switch
                            {
                                BigInteger n => n,
                                byte n => new BigInteger(n),
                                sbyte n => new BigInteger(n),
                                short n => new BigInteger(n),
                                ushort n => new BigInteger(n),
                                int n => new BigInteger(n),
                                uint n => new BigInteger(n),
                                long n => new BigInteger(n),
                                ulong n => new BigInteger(n),

                                string text => BigInteger.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture),

                                _ => throw new ArgumentException($"Unsupported value type for {memberValue.TypeName}")
                            };

                            writer.Write(CeAbiEncoder.AbiValueEncodeBigInteger(signed, value, bitWidth));
                        }
                        else if (memberValue.TypeName.StartsWith("bytes", StringComparison.Ordinal))
                        {
                            if (memberValue.Value is not byte[] value)
                                throw new ArgumentException($"Value for {memberValue.TypeName} must be byte[].");

                            if (memberValue.TypeName == "bytes")
                            {
                                // EIP-712 dynamic bytes: hash the entire contents,
                                // including empty arrays, without padding first.
                                writer.Write(CeSha3Keccack.CalculateHash(value));
                            }
                            else
                            {
                                var lengthText = memberValue.TypeName.Substring(5);
                                if (!int.TryParse(lengthText, NumberStyles.None, CultureInfo.InvariantCulture, out var length))
                                    throw new ArgumentException($"Invalid bytes type: {memberValue.TypeName}");

                                writer.Write(CeAbiEncoder.AbiValueEncodeBytes(length, value));
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Unsupported EIP-712 type: {memberValue.TypeName}");
                        }
                        break;

                }
            }
        }

        private static void EncodeType(BinaryWriter writer, IDictionary<string, CeMemberDescription[]> types, string typeName)
        {
            var encodedTypes = EncodeTypes(types, typeName);
            var encodedPrimaryType = encodedTypes.SingleOrDefault(x => x.Key == typeName);
            if (encodedPrimaryType.Equals(default(KeyValuePair<string, string>)))
                throw new Exception($"Type {typeName} not found in types dictionary");

            var fullyEncodedType = string.Concat(encodedTypes.Select(x => x.Value));
            writer.Write(CeSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(fullyEncodedType)));
        }

        private static IList<KeyValuePair<string, string>> EncodeTypes(
            IDictionary<string, CeMemberDescription[]> types,
            string currentTypeName)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal);

            void Visit(string typeName)
            {
                // Prevent duplicate dependencies and recursive traversal loops.
                if (!visited.Add(typeName))
                    return;

                if (!types.TryGetValue(typeName, out var members))
                    throw new ArgumentException($"Type {typeName} not found in types dictionary", nameof(types));

                foreach (var member in members)
                {
                    var memberType = member.Type;

                    // Strip all array dimensions to find the underlying type.
                    var arrayIndex = memberType.IndexOf('[');
                    if (arrayIndex >= 0)
                        memberType = memberType.Substring(0, arrayIndex);

                    if (IsReferenceType(memberType))
                        Visit(memberType);
                }
            }

            KeyValuePair<string, string> Encode(string typeName)
            {
                var members = types[typeName]
                    .Select(x => $"{x.Type} {x.Name}");

                return new KeyValuePair<string, string>(
                    typeName,
                    $"{typeName}({string.Join(",", members)})");
            }

            Visit(currentTypeName);

            var result = new List<KeyValuePair<string, string>>
            {
                Encode(currentTypeName)
            };

            result.AddRange(
                visited
                    .Where(x => !StringComparer.Ordinal.Equals(x, currentTypeName))
                    .OrderBy(x => x, StringComparer.Ordinal)
                    .Select(Encode));

            return result;
        }

        internal static bool IsReferenceType(string typeName)
        {
            if (typeName == "bytes" ||
                typeName == "string" ||
                typeName == "bool" ||
                typeName == "address" ||
                typeName.Contains("["))
            {
                return false;
            }

            var isPrimitive = Regex.IsMatch(
                typeName,
                @"\A(?:bytes[0-9]+|u?int[0-9]+)\z");

            return !isPrimitive;
        }
    }

    /// <summary>
    /// Member description
    /// </summary>
    public class CeMemberDescription
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Member value
    /// </summary>
    public class CeMemberValue
    {
        /// <summary>
        /// Type name
        /// </summary>
        public string TypeName { get; set; } = string.Empty;
        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Typed data raw, used for encoding EIP712 typed data with the provided primary type, domain fields and message fields.
    /// </summary>
    public class CeTypedDataRaw
    {
        /// <summary>
        /// Type dictionary
        /// </summary>
        public IDictionary<string, CeMemberDescription[]> Types { get; set; } = new Dictionary<string, CeMemberDescription[]>();
        /// <summary>
        /// Primary type
        /// </summary>
        public string PrimaryType { get; set; } = string.Empty;
        /// <summary>
        /// Message values
        /// </summary>
        public CeMemberValue[] Message { get; set; } = [];
        /// <summary>
        /// Domain values
        /// </summary>
        public CeMemberValue[] DomainRawValues { get; set; } = [];
    }
}