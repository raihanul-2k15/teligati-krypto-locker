using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;


namespace TeligatiKrypto
{
    public static class MyRSA
    {
        public static void KeyGen(int key_size, out BigInteger oe, out BigInteger od, out BigInteger on)
        {
            RandomBigInteger rand = new RandomBigInteger();
            BigInteger p = rand.NextBigInteger(key_size / 2);
            p = p.GetNextPrime();

            int qBitLen = key_size - p.BitLength();
            BigInteger q, n;
            do
            {
                q = rand.NextBigInteger(qBitLen);
                q = q.GetNextPrime();
                n = p * q;
                qBitLen++;
            } while (n.BitLength() < key_size + 1);

            BigInteger phi = (p - 1) * (q - 1);
            BigInteger e = 3;
            while (BigInteger.GreatestCommonDivisor(e, phi) != 1)
                e += 2;

            BigInteger d = (e.ModInverse(phi));

            oe = e;
            od = d;
            on = n;
        }

        public static BigInteger Encrypt(BigInteger m, BigInteger e, BigInteger n)
        {
            return BigInteger.ModPow(m, e, n);
        }

        public static BigInteger Decrypt(ref BigInteger c, ref BigInteger d, ref BigInteger n)
        {
            return BigInteger.ModPow(c, d, n);
        }

        private static BigInteger myModPow(BigInteger a, BigInteger b, BigInteger n)
        {
            if (b.IsOne)
                return a % n;
            BigInteger r = myModPow(a, b / 2, n);
            r = r * r % n;
            if (!b.IsEven)
                r = r * a % n;
            return r;
        }

        private static BigInteger myModPow2(BigInteger a, BigInteger b, BigInteger n)
        {
            BigInteger r = BigInteger.One;
            while (b > BigInteger.Zero) {
                if (!b.IsEven)
                    r = r * a % n;
                b /= 2;
                a = a * a % n;
            }
            return r;
        }
    }
}
