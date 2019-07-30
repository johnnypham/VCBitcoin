using System;
using System.IO;
using System.Text;
using VCBitcoin;
using VCBitcoin.BlockParser;

namespace VCBitcoinBlockchainParser {

    public class OutputTxn {

        public UInt64 value { get; set; }
        private byte firstVarIntByteScriptLength;
        private UInt64 outputScriptLength;
        public ScriptPubKey scriptPubKey { get; set; }
        public Address destinationAddress { get; set; }

        public void ReadStream(BinaryReader br) {

            value = br.ReadUInt64();
            VarInt.Decode(br, out firstVarIntByteScriptLength, out outputScriptLength);
            scriptPubKey = new ScriptPubKey(br, outputScriptLength);
        }

        public void WriteStream(BinaryWriter bw) {

            bw.Write(value);
            VarInt.WriteStream(bw, firstVarIntByteScriptLength, outputScriptLength);
            bw.Write(scriptPubKey.scriptPubKey);
        }

    }

}