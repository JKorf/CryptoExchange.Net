using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace CryptoExchange.Net.Authentication.Signing
{
    /// <summary>
    /// EIP712 Typed Data Encoder
    /// </summary>
    public static class CeEip712TypedDataEncoder
    {
        /// <summary>
        /// Encode EIP712 typed data according to the specification, with the provided primary type, domain fields and message fields.
        /// The resulting byte array is the 0x19 0x01 prefix followed by the hash of the domain and the hash of the message, which can be signed with ECDSA secp256k1 to produce a signature that can be verified on chain with EIP712.
        /// Note that this implementation does not support all possible EIP712 types, but it should cover most common use cases
        /// </summary>
        public static byte[] EncodeEip721(
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
        /// The resulting byte array is the 0x19 0x01 prefix followed by the hash of the domain and the hash of the message, which can be signed with ECDSA secp256k1 to produce a signature that can be verified on chain with EIP712.
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
                        else if (memberValue.TypeName.StartsWith("int") || memberValue.TypeName.StartsWith("uint"))
                        {
                            if (memberValue.Value is string v)
                            {
                                if (!BigInteger.TryParse(v, out BigInteger parsedOutput))
                                    throw new Exception("");

                                writer.Write(CeAbiEncoder.AbiValueEncodeBigInteger(memberValue.TypeName[0] != 'u', parsedOutput));
                            }
                            else if (memberValue.Value is byte b)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(b));
                            }
                            else if (memberValue.Value is short s)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(s));
                            }
                            else if (memberValue.Value is int i)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(i));
                            }
                            else if (memberValue.Value is long l)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(l));
                            }
                            else if (memberValue.Value is ushort us)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(us));
                            }
                            else if (memberValue.Value is uint ui)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(ui));
                            }
                            else if (memberValue.Value is ulong ul)
                            {
                                writer.Write(CeAbiEncoder.AbiValueEncodeInt(ul));
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else if (memberValue.TypeName.StartsWith("bytes"))
                        {
                            // Applicable?
                            //if (memberValue.Value is string v)
                            //    writer.Write(AbiEncoder.AbiValueEncodeHexBytes(v));
                            //else if (memberValue.Value is byte[] b)
                            //    writer.Write(AbiEncoder.AbiValueEncodeBytes(b));
                            //else
                            //    throw new Exception("Unknown byte value type");

                            var length = memberValue.TypeName.Length == 5 ? 32 : int.Parse(memberValue.TypeName.Substring(5));
                            writer.Write(CeAbiEncoder.AbiValueEncodeBytes(length, (byte[])memberValue.Value));
                        }
                        break;

                }
            }
        }

        private static void EncodeType(BinaryWriter writer, IDictionary<string, CeMemberDescription[]> types, string typeName)
        {
            var encodedTypes = EncodeTypes(types, typeName);
            var encodedPrimaryType = encodedTypes.Single(x => x.Key == typeName);
            var encodedReferenceTypes = encodedTypes.Where(x => x.Key != typeName).OrderBy(x => x.Key).Select(x => x.Value);
            var fullyEncodedType = encodedPrimaryType.Value + string.Join(string.Empty, encodedReferenceTypes.ToArray());

            writer.Write(CeSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(fullyEncodedType)));
        }

        /// <summary>
        /// Create a list of type => type(parameters), for example:<br />
        /// { IP712Domain, EIP712Domain(string name,string version,uint256 chainId,address verifyingContract) }
        /// </summary>
        private static IList<KeyValuePair<string, string>> EncodeTypes(IDictionary<string, CeMemberDescription[]> types, string currentTypeName)
        {
            var currentTypeMembers = types[currentTypeName];
            var currentTypeMembersEncoded = currentTypeMembers.Select(x => x.Type + " " + x.Name);
            var result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(currentTypeName, currentTypeName + "(" + string.Join(",", currentTypeMembersEncoded.ToArray()) + ")")
            };

            result.AddRange(currentTypeMembers.Select(x => x.Type.Contains("[") ? x.Type.Substring(0, x.Type.IndexOf("[")) : x.Type)
                                              .Distinct()
                                              .Where(IsReferenceType)
                                              .SelectMany(x => EncodeTypes(types, x)));
            return result;
        }

        internal static bool IsReferenceType(string typeName)
        {
            switch (typeName)
            {
                case var bytes when new Regex("bytes\\d+").IsMatch(bytes):
                case var @uint when new Regex("uint\\d+").IsMatch(@uint):
                case var @int when new Regex("int\\d+").IsMatch(@int):
                case "bytes":
                case "string":
                case "bool":
                case "address":
                case var array when array.Contains("["):
                    return false;
                default:
                    return true;
            }
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