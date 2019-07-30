using System;
using System.Collections.Generic;
using System.Linq;

namespace VCBitcoin.BlockchainParser {

    public class StaleBlockRemover {

        public static List<string> Clean(List<Dictionary<string, string>> blockchain) {

            List<string> blocksToRemove = new List<string>();
            Dictionary<string, string> currBlock;
            Dictionary<string, string> nextBlock;

            string currBlockPrevHash = string.Empty;
            string nextBlockHash = string.Empty;

            for (int i = blockchain.Count - 1; i > 0; --i) {
                currBlock = blockchain.ElementAt(i);
                nextBlock = blockchain.ElementAt(i - 1);

                foreach (KeyValuePair<string, string> e in currBlock) {
                    currBlockPrevHash = e.Value;
                    foreach (KeyValuePair<string, string> f in nextBlock) {
                        nextBlockHash = f.Key;
                    }
                }

                if (currBlockPrevHash != nextBlockHash) {
                    blocksToRemove.Add(nextBlockHash);
                }
            }

            return blocksToRemove;
        }

    }

}