using System;
using System.IO;

namespace VCBitcoin.BlockParser {

    public class StackItem {

        private byte firstVarIntByteLength;
        private UInt64 itemLength;
        private byte[] item;

        public void ReadStream(BinaryReader br) {

            VarInt.Decode(br, out firstVarIntByteLength, out itemLength);
            item = Util.InstantiateArrayOf<byte>(itemLength);

            for (int i = 0; i < item.Length; ++i) {
                item[i] = br.ReadByte();
            }
        }

    }

}
