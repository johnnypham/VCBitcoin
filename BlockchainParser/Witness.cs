using System;
using System.Collections.Generic;
using System.IO;
using VCBitcoin;
using VCBitcoin.BlockParser;

namespace VCBitcoinBlockchainParser {

    public class Witness {

        private byte firstVarIntByteNItems;
        public UInt64 nStackItems;
        public StackItem[] stackItems;

        public void ReadStream(BinaryReader br) {

            VarInt.Decode(br, out firstVarIntByteNItems, out nStackItems);
            stackItems = Util.InstantiateArrayOf<StackItem>(nStackItems);

            for (int i = 0; i < stackItems.Length; ++i) {
                stackItems[i].ReadStream(br);
            }
        }

    }

}