using System;
using System.IO;
using VCBitcoin;

namespace VCBitcoinBlockchainParser {

    public class InputTxn {

        public Hash txnHash { get; set; }
        public UInt32 vout { get; set; }
        private byte firstVarIntByteScriptLength;
        private UInt64 inputScriptLength;
        public byte[] inputScript { get; set; }
        public UInt32 sequenceNum { get; set; }

        public void ReadStream(BinaryReader br) {

            byte[] hexTxnHash = new byte[32];
            hexTxnHash = br.ReadBytes(32);
            txnHash = new Hash(hexTxnHash);

            vout = br.ReadUInt32();

            if (vout == 0xFFFFFFFF) {
                vout = 0x11111111;
            }

            VarInt.Decode(br, out firstVarIntByteScriptLength, out inputScriptLength);

            inputScript = Util.InstantiateArrayOf<byte>(inputScriptLength);

            for (int i = 0; i < inputScript.Length; ++i) {
                inputScript[i] = br.ReadByte();
            }

            sequenceNum = br.ReadUInt32();
        }

        public void WriteStream(BinaryWriter bw) {

            bw.Write(txnHash.hex);
            bw.Write(vout);
            VarInt.WriteStream(bw, firstVarIntByteScriptLength, inputScriptLength);
            bw.Write(inputScript);
            bw.Write(sequenceNum);
        }

    }

}