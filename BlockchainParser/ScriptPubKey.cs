using System;
using System.IO;

namespace VCBitcoin.BlockParser {

    public class ScriptPubKey {

        public byte[] scriptPubKey { get; }
        public byte this[int index] => scriptPubKey[index];
        public int length => scriptPubKey.Length;

        public ScriptPubKey(BinaryReader br, UInt64 length) {

            scriptPubKey = Util.InstantiateArrayOf<byte>(length);

            for (int i = 0; i < scriptPubKey.Length; ++i) {
                scriptPubKey[i] = br.ReadByte();
            }
        }

        public override string ToString() {
            return Util.ByteArrayToString(scriptPubKey);
        }

    }

}