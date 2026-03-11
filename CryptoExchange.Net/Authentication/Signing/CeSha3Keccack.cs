using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CryptoExchange.Net.Authentication.Signing
{
    /// <summary>
    /// Sha3 Keccack hashing, as per Ethereum specification, with 256 bit output
    /// </summary>
    public class CeSha3Keccack
    {
        /// <summary>
        /// Calculate the Keccack256 hash of the provided data, as per Ethereum specification
        /// </summary>
        public static byte[] CalculateHash(byte[] data)
        {
            var digest = new CeKeccakDigest256();
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, data.Length);
            digest.DoFinal(output, 0);
            return output;
        }
    }

    internal class CeKeccakDigest256
    {
        private static readonly ulong[] _keccakRoundConstants = KeccakInitializeRoundConstants();
        private static readonly int[] _keccakRhoOffsets = KeccakInitializeRhoOffsets();

        private readonly int _rate;
        private const int _stateLength = 1600 / 8;
        private readonly ulong[] _state = new ulong[_stateLength / 8];
        private readonly byte[] _dataQueue = new byte[1536 / 8];
        private int _bitsInQueue;
        private int _fixedOutputLength;
        private bool _squeezing;
        private int _bitsAvailableForSqueezing;

        public CeKeccakDigest256()
        {
            _rate = 1600 - (256 << 1);
            _bitsInQueue = 0;
            _squeezing = false;
            _bitsAvailableForSqueezing = 0;
            _fixedOutputLength = 1600 - _rate >> 1;
        }

        internal void BlockUpdate(byte[] data, int length)
        {
            int bytesInQueue = _bitsInQueue >> 3;
            int rateBytes = _rate >> 3;

            int count = 0;
            while (count < length)
            {
                if (bytesInQueue == 0 && count <= length - rateBytes)
                {
                    do
                    {
                        KeccakAbsorb(data, count);
                        count += rateBytes;
                    } while (count <= length - rateBytes);
                }
                else
                {
                    int partialBlock = Math.Min(rateBytes - bytesInQueue, length - count);
                    Array.Copy(data, count, _dataQueue, bytesInQueue, partialBlock);

                    bytesInQueue += partialBlock;
                    count += partialBlock;

                    if (bytesInQueue == rateBytes)
                    {
                        KeccakAbsorb(_dataQueue, 0);
                        bytesInQueue = 0;
                    }
                }
            }

            _bitsInQueue = bytesInQueue << 3;
        }

        internal void DoFinal(byte[] output, int outOff)
        {
            Squeeze(output, outOff, _fixedOutputLength >> 3);
        }

        internal int GetDigestSize() => _fixedOutputLength >> 3;

        protected void Squeeze(byte[] output, int off, int len)
        {
            if (!_squeezing)
                PadAndSwitchToSqueezingPhase();

            long outputLength = (long)len << 3;
            long i = 0;
            while (i < outputLength)
            {
                if (_bitsAvailableForSqueezing == 0)
                {
                    KeccakPermutation();
                    KeccakExtract();
                    _bitsAvailableForSqueezing = _rate;
                }

                int partialBlock = (int)Math.Min(_bitsAvailableForSqueezing, outputLength - i);
                Array.Copy(_dataQueue, _rate - _bitsAvailableForSqueezing >> 3, output, off + (int)(i >> 3),
                    partialBlock >> 3);
                _bitsAvailableForSqueezing -= partialBlock;
                i += partialBlock;
            }
        }

        private static ulong[] KeccakInitializeRoundConstants()
        {
            ulong[] keccakRoundConstants = new ulong[24];
            byte LFSRState = 0x01;

            for (int i = 0; i < 24; i++)
            {
                keccakRoundConstants[i] = 0;
                for (int j = 0; j < 7; j++)
                {
                    int bitPosition = (1 << j) - 1;

                    // LFSR86540

                    bool loBit = (LFSRState & 0x01) != 0;
                    if (loBit)
                        keccakRoundConstants[i] ^= 1UL << bitPosition;

                    bool hiBit = (LFSRState & 0x80) != 0;
                    LFSRState <<= 1;
                    if (hiBit)
                        LFSRState ^= 0x71;
                }
            }

            return keccakRoundConstants;
        }
        private static int[] KeccakInitializeRhoOffsets()
        {
            int[] keccakRhoOffsets = new int[25];
            int x, y, t, newX, newY;

            int rhoOffset = 0;
            keccakRhoOffsets[0] = rhoOffset;
            x = 1;
            y = 0;
            for (t = 1; t < 25; t++)
            {
                rhoOffset = rhoOffset + t & 63;
                keccakRhoOffsets[x % 5 + 5 * (y % 5)] = rhoOffset;
                newX = (0 * x + 1 * y) % 5;
                newY = (2 * x + 3 * y) % 5;
                x = newX;
                y = newY;
            }

            return keccakRhoOffsets;
        }

        private void KeccakAbsorb(byte[] data, int off)
        {
            int count = _rate >> 6;
            for (int i = 0; i < count; ++i)
            {
                _state[i] ^= Pack.LeToUInt64(data, off);
                off += 8;
            }

            KeccakPermutation();
        }

        private void KeccakPermutation()
        {
            for (int i = 0; i < 24; i++)
            {
                Theta(_state);
                Rho(_state);
                Pi(_state);
                Chi(_state);
                Iota(_state, i);
            }
        }
        private static ulong LeftRotate(ulong v, int r)
        {
            return v << r | v >> -r;
        }

        private static void Theta(ulong[] A)
        {
            ulong C0 = A[0 + 0] ^ A[0 + 5] ^ A[0 + 10] ^ A[0 + 15] ^ A[0 + 20];
            ulong C1 = A[1 + 0] ^ A[1 + 5] ^ A[1 + 10] ^ A[1 + 15] ^ A[1 + 20];
            ulong C2 = A[2 + 0] ^ A[2 + 5] ^ A[2 + 10] ^ A[2 + 15] ^ A[2 + 20];
            ulong C3 = A[3 + 0] ^ A[3 + 5] ^ A[3 + 10] ^ A[3 + 15] ^ A[3 + 20];
            ulong C4 = A[4 + 0] ^ A[4 + 5] ^ A[4 + 10] ^ A[4 + 15] ^ A[4 + 20];

            ulong dX = LeftRotate(C1, 1) ^ C4;

            A[0] ^= dX;
            A[5] ^= dX;
            A[10] ^= dX;
            A[15] ^= dX;
            A[20] ^= dX;

            dX = LeftRotate(C2, 1) ^ C0;

            A[1] ^= dX;
            A[6] ^= dX;
            A[11] ^= dX;
            A[16] ^= dX;
            A[21] ^= dX;

            dX = LeftRotate(C3, 1) ^ C1;

            A[2] ^= dX;
            A[7] ^= dX;
            A[12] ^= dX;
            A[17] ^= dX;
            A[22] ^= dX;

            dX = LeftRotate(C4, 1) ^ C2;

            A[3] ^= dX;
            A[8] ^= dX;
            A[13] ^= dX;
            A[18] ^= dX;
            A[23] ^= dX;

            dX = LeftRotate(C0, 1) ^ C3;

            A[4] ^= dX;
            A[9] ^= dX;
            A[14] ^= dX;
            A[19] ^= dX;
            A[24] ^= dX;
        }

        private static void Rho(ulong[] A)
        {
            // KeccakRhoOffsets[0] == 0
            for (int x = 1; x < 25; x++)
            {
                A[x] = LeftRotate(A[x], _keccakRhoOffsets[x]);
            }
        }

        private static void Pi(ulong[] A)
        {
            ulong a1 = A[1];
            A[1] = A[6];
            A[6] = A[9];
            A[9] = A[22];
            A[22] = A[14];
            A[14] = A[20];
            A[20] = A[2];
            A[2] = A[12];
            A[12] = A[13];
            A[13] = A[19];
            A[19] = A[23];
            A[23] = A[15];
            A[15] = A[4];
            A[4] = A[24];
            A[24] = A[21];
            A[21] = A[8];
            A[8] = A[16];
            A[16] = A[5];
            A[5] = A[3];
            A[3] = A[18];
            A[18] = A[17];
            A[17] = A[11];
            A[11] = A[7];
            A[7] = A[10];
            A[10] = a1;
        }

        private static void Chi(ulong[] A)
        {
            ulong chiC0, chiC1, chiC2, chiC3, chiC4;

            for (int yBy5 = 0; yBy5 < 25; yBy5 += 5)
            {
                chiC0 = A[0 + yBy5] ^ ~A[(0 + 1) % 5 + yBy5] & A[(0 + 2) % 5 + yBy5];
                chiC1 = A[1 + yBy5] ^ ~A[(1 + 1) % 5 + yBy5] & A[(1 + 2) % 5 + yBy5];
                chiC2 = A[2 + yBy5] ^ ~A[(2 + 1) % 5 + yBy5] & A[(2 + 2) % 5 + yBy5];
                chiC3 = A[3 + yBy5] ^ ~A[(3 + 1) % 5 + yBy5] & A[(3 + 2) % 5 + yBy5];
                chiC4 = A[4 + yBy5] ^ ~A[(4 + 1) % 5 + yBy5] & A[(4 + 2) % 5 + yBy5];

                A[0 + yBy5] = chiC0;
                A[1 + yBy5] = chiC1;
                A[2 + yBy5] = chiC2;
                A[3 + yBy5] = chiC3;
                A[4 + yBy5] = chiC4;
            }
        }

        private static void Iota(ulong[] A, int indexRound)
        {
            A[0] ^= _keccakRoundConstants[indexRound];
        }

        private void PadAndSwitchToSqueezingPhase()
        {
            Debug.Assert(_bitsInQueue < _rate);

            _dataQueue[_bitsInQueue >> 3] |= (byte)(1U << (_bitsInQueue & 7));

            if (++_bitsInQueue == _rate)
            {
                KeccakAbsorb(_dataQueue, 0);
                _bitsInQueue = 0;
            }

            {
                int full = _bitsInQueue >> 6, partial = _bitsInQueue & 63;
                int off = 0;
                for (int i = 0; i < full; ++i)
                {
                    _state[i] ^= Pack.LeToUInt64(_dataQueue, off);
                    off += 8;
                }

                if (partial > 0)
                {
                    ulong mask = (1UL << partial) - 1UL;
                    _state[full] ^= Pack.LeToUInt64(_dataQueue, off) & mask;
                }

                _state[_rate - 1 >> 6] ^= 1UL << 63;
            }

            KeccakPermutation();
            KeccakExtract();
            _bitsAvailableForSqueezing = _rate;

            _bitsInQueue = 0;
            _squeezing = true;
        }
        private void KeccakExtract()
        {
            Pack.UInt64ToLe(_state, 0, _rate >> 6, _dataQueue, 0);
        }

        static class Pack
        {
            internal static ulong LeToUInt64(byte[] bs, int off)
            {
                uint lo = LeToUInt32(bs, off);
                uint hi = LeToUInt32(bs, off + 4);
                return (ulong)hi << 32 | lo;
            }
            internal static uint LeToUInt32(byte[] bs, int off)
            {
                return bs[off]
                       | (uint)bs[off + 1] << 8
                       | (uint)bs[off + 2] << 16
                       | (uint)bs[off + 3] << 24;
            }

            internal static void UInt64ToLe(ulong[] ns, int nsOff, int nsLen, byte[] bs, int bsOff)
            {
                for (int i = 0; i < nsLen; ++i)
                {
                    UInt64ToLe(ns[nsOff + i], bs, bsOff);
                    bsOff += 8;
                }
            }
            internal static void UInt64ToLe(ulong n, byte[] bs, int off)
            {
                UInt32ToLe((uint)n, bs, off);
                UInt32ToLe((uint)(n >> 32), bs, off + 4);
            }

            internal static void UInt32ToLe(uint n, byte[] bs, int off)
            {
                bs[off] = (byte)n;
                bs[off + 1] = (byte)(n >> 8);
                bs[off + 2] = (byte)(n >> 16);
                bs[off + 3] = (byte)(n >> 24);
            }
        }
    }
}
