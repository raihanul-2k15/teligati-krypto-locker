using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Numerics;
using System.Threading.Tasks;

namespace TeligatiKrypto
{
    public static class Util
    {
        public static string Hash(string s)
        {
            SHA1 hasher = new SHA1Managed();
            byte[] bArr = Encoding.UTF8.GetBytes(s);
            bArr = hasher.ComputeHash(bArr);
            StringBuilder hex = new StringBuilder(bArr.Length * 2);
            foreach (byte b in bArr)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] XOREncDec(byte[] ba)
        {
            byte[] o = new byte[ba.Length];
            for (int i = 0; i < ba.Length; i++)
                o[i] = (byte)(ba[i] ^ Config.XORKey);
            return o;
        }

        public static int BitLength(this BigInteger b)
        {
            int bitLength = 0;
            do
            {
                bitLength++;
                b /= 2;
            } while (b != 0);
            return bitLength;
        }

        public static BigInteger ModInverse(this BigInteger a, BigInteger m)
        {
            BigInteger x, y;
            BigInteger g = gcdExtended(a, m, out x, out y);
            if (g != 1)
                throw new ArithmeticException("This integer doesn't have an inverse modulo given integer!");
            else
            {
                BigInteger res = (x % m + m) % m;
                return res;
            }
        }
        
        private static BigInteger gcdExtended(BigInteger a, BigInteger b, out BigInteger x, out BigInteger y)
        {
            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }

            BigInteger x1, y1;
            BigInteger gcd = gcdExtended(b % a, a, out x1, out y1);
            
            x = y1 - (b / a) * x1;
            y = x1;

            return gcd;
        }

        public static BigInteger ArrToUnsignedBigInt(ref byte[] b)
        {
            BigInteger bi;
            if ((b[b.Length - 1] & 128) > 0)
            {
                List<byte> l = new List<byte>(b);
                l.Add(0);
                bi = new BigInteger(l.ToArray());
            }
            else
                bi = new BigInteger(b);
            return bi;
        }

        public static byte[] BigIntToFixedLenArr(ref BigInteger bi, int len)
        {
            byte[] b = bi.ToByteArray();
            if (b.Length > len + 1) // + 1 bcz 1 zero byte maybe added to hold sign bit
                throw new ArgumentException("Byte array of length " + len + " too short to hold given big integer. Required length: " + bi.BitLength());
            List<byte> l = new List<byte>(b);
            if (b.Length == len + 1 && b[len] == 0) // if extra byte containing sign bit
                l.RemoveAt(l.Count - 1); // remove sign bit
            //for (int i = 0, n = len - l.Count; i < n; i++)
            //    l.Add(0);
            l.AddRange(new byte[len - l.Count]);
            return l.ToArray();
        }
    }
}
