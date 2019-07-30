using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VCBitcoin;

namespace VCBitcoinBlockchainParser {

    public class Transaction {

        public bool isSegwit { get; set; }
        public UInt32 versionNum;
        private byte firstVarIntByteNInputs;
        public UInt64 nInputs;
        public InputTxn[] inputs { get; set; }
        public UInt64 nOutputs;
        private byte firstVarIntByteNOutputs;
        public OutputTxn[] outputs { get; set; }
        public UInt32 lockTime;
        public Witness[] witnesses;
        public Hash txId { get; set; }

        public void ReadStream(BinaryReader br) {

            versionNum = br.ReadUInt32();

            isSegwit = CheckForSegwitFlag(br);

            VarInt.Decode(br, out firstVarIntByteNInputs, out nInputs);
            inputs = Util.InstantiateArrayOf<InputTxn>(nInputs);

            for (int i = 0; i < inputs.Length; ++i) {
                inputs[i].ReadStream(br);
            }

            VarInt.Decode(br, out firstVarIntByteNOutputs, out nOutputs);
            outputs = Util.InstantiateArrayOf<OutputTxn>(nOutputs);

            for (int i = 0; i < outputs.Length; ++i) {
                outputs[i].ReadStream(br);
            }

            if (isSegwit) {
                witnesses = Util.InstantiateArrayOf<Witness>(nInputs);

                for (int i = 0; i < witnesses.Length; ++i) {
                    witnesses[i].ReadStream(br);
                }
            }

            lockTime = br.ReadUInt32();

            foreach (var output in outputs) {
                output.destinationAddress = Script.ExtractDestination(output);
            }

            txId = GetTxId();
        }

        private Hash GetTxId() {

            using (MemoryStream ms = new MemoryStream()) {
                using (BinaryWriter bw = new BinaryWriter(ms)) {

                    bw.Write(versionNum);

                    VarInt.WriteStream(bw, firstVarIntByteNInputs, nInputs);
                    foreach (var input in inputs) {
                        input.WriteStream(bw);
                    }

                    VarInt.WriteStream(bw, firstVarIntByteNOutputs, nOutputs);
                    foreach (var output in outputs) {
                        output.WriteStream(bw);
                    }

                    bw.Write(lockTime);

                    return new Hash(Crypto.DoubleSha256(ms.ToArray()));
                }
            }
        }

        private bool CheckForSegwitFlag(BinaryReader br) {

            long initialPosition = br.BaseStream.Position;

            if (br.ReadByte() == 0x00 && br.ReadByte() == 0x01) {
                return true;
            } else {
                br.BaseStream.Position = initialPosition;
                return false;
            }
        }

    }

}