using System;
using System.IO;
using System.Text;
using VCBitcoin;

namespace VCBitcoinBlockchainParser {

    public class Hash {

        public byte[] hex { get; }

        public Hash(byte[] hex) {
            this.hex = hex;
        }

        public override string ToString() {
            return Util.ByteArrayToStringRevEndian(hex);
        }

    }

}