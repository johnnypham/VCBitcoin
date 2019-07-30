using System;
using System.Collections.Generic;
using System.Text;
using VCBitcoin;
using VCBitcoin.BlockParser;
using System.Security.Cryptography;

namespace VCBitcoinBlockchainParser {

    public class Address {

        private string address;

        public Address() {
            address = string.Empty;
        }

        public Address(ScriptPubKey scriptPubKey, TxnType txnType) {

            byte[] pushData = Script.FromScriptPubKey(scriptPubKey, txnType);

            List<byte> hash;
            byte prefix = 0x00;

            switch (txnType) {

                case TxnType.P2SH:
                    hash = new List<byte>(pushData);
                    prefix = (byte)Prefix.TestnetP2SH;
                    break;

                case TxnType.P2WPKH:
                    hash = new List<byte>(pushData);
                    break;

                case TxnType.P2WSH:
                    hash = new List<byte>(pushData);
                    break;

                case TxnType.P2PKH:
                    hash = new List<byte>(pushData);
                    prefix = (byte)Prefix.TestnetP2PKH;
                    break;

                case TxnType.P2PK:
                    hash = Crypto.DoubleHash(pushData);
                    prefix = (byte)Prefix.TestnetP2PKH;
                    break;

                default:
                    hash = null;
                    break;
            }

            if (hash != null) {
                if (txnType == TxnType.P2WPKH || txnType == TxnType.P2WSH) {
                    address = Bech32.Encode(0, hash.ToArray(), false);
                } else {
                    hash.Insert(0, prefix);
                    Crypto.AddChecksum(ref hash);
                    address = Base58Check.Encode(hash.ToArray());
                }
            }
        }

        public override string ToString() {
            return address;
        }

    }

}