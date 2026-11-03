using System.Collections;
using System.Globalization;
using System.Text;

namespace CryptoExchange.Net.Web3
{
    /// <summary>
    /// Message pack converter, not thread safe
    /// </summary>
    public class MessagePackConverter
    {
        private byte[] _tmp0 = new byte[8];
        private Encoding _encoder = Encoding.UTF8;

        /// <summary>
        /// Convert an object to byte array
        /// </summary>
        public byte[] Pack(object o)
        {
            using var ms = new MemoryStream();
            Pack(ms, o);
            return ms.ToArray();
        }

        /// <summary>
        /// Convert an object to byte array
        /// </summary>
        public void Pack(Stream s, object o)
        {
            if (o == null)
                PackNull(s);
            else if (o is string objStr)
                Pack(s, objStr);
            else if (o is IList objList)
                Pack(s, objList);
            else if (o is IDictionary objDict)
                Pack(s, objDict);
            else if (o is IDictionary<string, object> objDict2)
                Pack(s, objDict2);
            else if (o is bool objBool)
                Pack(s, objBool);
            else if (o is sbyte objSbyte)
                Pack(s, objSbyte);
            else if (o is byte objByte)
                Pack(s, objByte);
            else if (o is short objShort)
                Pack(s, objShort);
            else if (o is ushort objUshort)
                Pack(s, objUshort);
            else if (o is int objInt)
                Pack(s, objInt);
            else if (o is uint objUint)
                Pack(s, objUint);
            else if (o is long objLong)
                Pack(s, objLong);
            else if (o is ulong objUlong)
                Pack(s, objUlong);
            else if (o is float objFloat)
                Pack(s, objFloat);
            else if (o is double objDouble)
                Pack(s, objDouble);
            else
                Pack(s, Convert.ToString(o, CultureInfo.InvariantCulture)!);
        }

        private void PackNull(Stream s)
        {
            s.WriteByte(0xc0);
        }

        private void Pack(Stream s, IList list)
        {
            int count = list.Count;
            if (count < 16)
            {
                s.WriteByte((byte)(0x90 + count));
            }
            else if (count < 0x10000)
            {
                s.WriteByte(0xdc);
                Write(s, (ushort)count);
            }
            else
            {
                s.WriteByte(0xdd);
                Write(s, (uint)count);
            }
            foreach (object o in list)
            {
                Pack(s, o);
            }
        }

        private void Pack(Stream s, IDictionary dict)
        {
            int count = dict.Count;
            if (count < 16)
            {
                s.WriteByte((byte)(0x80 + count));
            }
            else if (count < 0x10000)
            {
                s.WriteByte(0xde);
                Write(s, (ushort)count);
            }
            else
            {
                s.WriteByte(0xdf);
                Write(s, (uint)count);
            }
            foreach (object key in dict.Keys)
            {
                Pack(s, key);
                Pack(s, dict[key]!);
            }
        }

        private void Pack(Stream s, IDictionary<string, object> dict)
        {
            int count = dict.Count;
            if (count < 16)
            {
                s.WriteByte((byte)(0x80 + count));
            }
            else if (count < 0x10000)
            {
                s.WriteByte(0xde);
                Write(s, (ushort)count);
            }
            else
            {
                s.WriteByte(0xdf);
                Write(s, (uint)count);
            }
            foreach (var key in dict.Keys)
            {
                Pack(s, key);
                Pack(s, dict[key]!);
            }
        }

        private void Pack(Stream s, bool val)
        {
            s.WriteByte(val ? (byte)0xc3 : (byte)0xc2);
        }

        private void Pack(Stream s, sbyte val)
        {
            unchecked
            {
                if (val >= -32)
                {
                    s.WriteByte((byte)val);
                }
                else
                {
                    _tmp0[0] = 0xd0;
                    _tmp0[1] = (byte)val;
                    s.Write(_tmp0, 0, 2);
                }
            }
        }

        private void Pack(Stream s, byte val)
        {
            if (val <= 0x7f)
            {
                s.WriteByte(val);
            }
            else
            {
                _tmp0[0] = 0xcc;
                _tmp0[1] = val;
                s.Write(_tmp0, 0, 2);
            }
        }

        private void Pack(Stream s, short val)
        {
            unchecked
            {
                if (val >= 0)
                {
                    Pack(s, (ushort)val);
                }
                else if (val >= -128)
                {
                    Pack(s, (sbyte)val);
                }
                else
                {
                    s.WriteByte(0xd1);
                    Write(s, (ushort)val);
                }
            }
        }

        private void Pack(Stream s, ushort val)
        {
            unchecked
            {
                if (val < 0x100)
                {
                    Pack(s, (byte)val);
                }
                else
                {
                    s.WriteByte(0xcd);
                    Write(s, val);
                }
            }
        }

        private void Pack(Stream s, int val)
        {
            unchecked
            {
                if (val >= 0)
                {
                    Pack(s, (uint)val);
                }
                else if (val >= -128)
                {
                    Pack(s, (sbyte)val);
                }
                else if (val >= -0x8000)
                {
                    s.WriteByte(0xd1);
                    Write(s, (ushort)val);
                }
                else
                {
                    s.WriteByte(0xd2);
                    Write(s, (uint)val);
                }
            }
        }

        private void Pack(Stream s, uint val)
        {
            unchecked
            {
                if (val < 0x100)
                {
                    Pack(s, (byte)val);
                }
                else if (val < 0x10000)
                {
                    s.WriteByte(0xcd);
                    Write(s, (ushort)val);
                }
                else
                {
                    s.WriteByte(0xce);
                    Write(s, val);
                }
            }
        }

        private void Pack(Stream s, long val)
        {
            unchecked
            {
                if (val >= 0)
                {
                    Pack(s, (ulong)val);
                }
                else if (val >= -128)
                {
                    Pack(s, (sbyte)val);
                }
                else if (val >= -0x8000)
                {
                    s.WriteByte(0xd1);
                    Write(s, (ushort)val);
                }
                else if (val >= -0x80000000)
                {
                    s.WriteByte(0xd2);
                    Write(s, (uint)val);
                }
                else
                {
                    s.WriteByte(0xd3);
                    Write(s, (ulong)val);
                }
            }
        }

        private void Pack(Stream s, ulong val)
        {
            unchecked
            {
                if (val < 0x100)
                {
                    Pack(s, (byte)val);
                }
                else if (val < 0x10000)
                {
                    s.WriteByte(0xcd);
                    Write(s, (ushort)val);
                }
                else if (val < 0x100000000)
                {
                    s.WriteByte(0xce);
                    Write(s, (uint)val);
                }
                else
                {
                    s.WriteByte(0xcf);
                    Write(s, val);
                }
            }
        }

        private void Pack(Stream s, float val)
        {
            var bytes = BitConverter.GetBytes(val);
            s.WriteByte(0xca);
            if (BitConverter.IsLittleEndian)
            {
                _tmp0[0] = bytes[3];
                _tmp0[1] = bytes[2];
                _tmp0[2] = bytes[1];
                _tmp0[3] = bytes[0];
                s.Write(_tmp0, 0, 4);
            }
            else
            {
                s.Write(bytes, 0, 4);
            }
        }

        private void Pack(Stream s, double val)
        {
            var bytes = BitConverter.GetBytes(val);
            s.WriteByte(0xcb);
            if (BitConverter.IsLittleEndian)
            {
                _tmp0[0] = bytes[7];
                _tmp0[1] = bytes[6];
                _tmp0[2] = bytes[5];
                _tmp0[3] = bytes[4];
                _tmp0[4] = bytes[3];
                _tmp0[5] = bytes[2];
                _tmp0[6] = bytes[1];
                _tmp0[7] = bytes[0];
                s.Write(_tmp0, 0, 8);
            }
            else
            {
                s.Write(bytes, 0, 8);
            }
        }

        private void Pack(Stream s, string val)
        {
            var bytes = _encoder.GetBytes(val);
            if (bytes.Length < 0x20)
            {
                s.WriteByte((byte)(0xa0 + bytes.Length));
            }
            else if (bytes.Length < 0x100)
            {
                s.WriteByte(0xd9);
                s.WriteByte((byte)bytes.Length);
            }
            else if (bytes.Length < 0x10000)
            {
                s.WriteByte(0xda);
                Write(s, (ushort)bytes.Length);
            }
            else
            {
                s.WriteByte(0xdb);
                Write(s, (uint)bytes.Length);
            }
            s.Write(bytes, 0, bytes.Length);
        }

        private void Write(Stream s, ushort val)
        {
            unchecked
            {
                _tmp0[0] = (byte)(val >> 8);
                _tmp0[1] = (byte)val;
                s.Write(_tmp0, 0, 2);
            }
        }

        private void Write(Stream s, uint val)
        {
            unchecked
            {
                _tmp0[0] = (byte)(val >> 24);
                _tmp0[1] = (byte)(val >> 16);
                _tmp0[2] = (byte)(val >> 8);
                _tmp0[3] = (byte)val;
                s.Write(_tmp0, 0, 4);
            }
        }

        private void Write(Stream s, ulong val)
        {
            unchecked
            {
                _tmp0[0] = (byte)(val >> 56);
                _tmp0[1] = (byte)(val >> 48);
                _tmp0[2] = (byte)(val >> 40);
                _tmp0[3] = (byte)(val >> 32);
                _tmp0[4] = (byte)(val >> 24);
                _tmp0[5] = (byte)(val >> 16);
                _tmp0[6] = (byte)(val >> 8);
                _tmp0[7] = (byte)val;
                s.Write(_tmp0, 0, 8);
            }
        }
    }
}
