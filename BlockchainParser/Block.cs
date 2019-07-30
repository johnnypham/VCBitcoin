using System;
using System.Dynamic;
using System.IO;
using VCBitcoin;

namespace VCBitcoinBlockchainParser {

    public class Block {

        public UInt32 magicId { get; set; }
        public UInt32 size { get; set; }
        public UInt32 version { get; set; }
        public Hash prevBlockHash { get; set; }
        public Hash merkleRootHash { get; set; }
        private UInt32 timestamp { get; set; }
        public DateTime creationTime;
        public UInt32 targetDifficulty { get; set; }
        public UInt32 nonce { get; set; }
        public byte firstVarIntByteNTxns;
        public UInt64 nTxns;
        public Transaction[] txns { get; set; }
        public Hash blockHash { get; set; }

        public void ReadStream(BinaryReader br) {

            magicId = br.ReadUInt32();
            size = br.ReadUInt32();
            version = br.ReadUInt32();

            byte[] hexPrevBlockHash = br.ReadBytes(32);
            prevBlockHash = new Hash(hexPrevBlockHash);

            byte[] hexMerkleRootHash = br.ReadBytes(32);
            merkleRootHash = new Hash(hexMerkleRootHash);

            timestamp = br.ReadUInt32();
            creationTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).
                AddSeconds(timestamp).ToLocalTime();

            targetDifficulty = br.ReadUInt32();
            nonce = br.ReadUInt32();

            blockHash = GetBlockHash();

            VarInt.Decode(br, out firstVarIntByteNTxns, out nTxns);
            txns = Util.InstantiateArrayOf<Transaction>(nTxns);

            for (UInt64 i = 0; i < nTxns; ++i) {
                txns[i].ReadStream(br);
            }
        }

        public Hash GetBlockHash() {

            using (MemoryStream ms = new MemoryStream()) {
                using (BinaryWriter bw = new BinaryWriter(ms)) {

                    bw.Write(version);
                    bw.Write(prevBlockHash.hex);
                    bw.Write(merkleRootHash.hex);
                    bw.Write(timestamp);
                    bw.Write(targetDifficulty);
                    bw.Write(nonce);

                    return new Hash(Crypto.DoubleSha256(ms.ToArray()));
                }
            }
        }

    }
}
