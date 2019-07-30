using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace VCBitcoin {

    public static class Crypto {

        public static List<byte> DoubleHash(byte[] publicKey) {

            using (SHA256 sha256 = SHA256.Create()) {
                using (RIPEMD160 ripemd160 = RIPEMD160.Create()) {
                    return new List<byte>(ripemd160.ComputeHash(sha256.ComputeHash(publicKey)));
                }
            }
        }

        public static byte[] DoubleSha256(byte[] data) {

            using (SHA256 sha256 = SHA256.Create()) {
                return sha256.ComputeHash(sha256.ComputeHash(data));
            }
        }

        public static void AddChecksum(ref List<byte> pubKeyHash) {

            using (SHA256 sha256 = SHA256.Create()) {
                byte[] doubleSha256 = sha256.ComputeHash(sha256.ComputeHash(pubKeyHash.ToArray()));

                byte[] checksum = new byte[4];

                Array.Copy(doubleSha256, 0, checksum, 0, 4);

                foreach (var b in checksum) {
                    pubKeyHash.Add(b);
                }
            }
        }

    }
}
