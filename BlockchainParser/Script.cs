using System;
using System.Diagnostics.Eventing.Reader;
using VCBitcoin.BlockParser;
using VCBitcoinBlockchainParser;

namespace VCBitcoin {

    public static class Script {

        public static Address ExtractDestination(OutputTxn output) {
            TxnType txnType = TxnType.Nonstandard;

            if (IsPayToScriptHash(output.scriptPubKey)) {
                txnType = TxnType.P2SH;
            }

            int witnessVersion = 0;
            byte[] witnessProgram = null;

            if (IsWitnessProgram(output.scriptPubKey, ref witnessVersion, ref witnessProgram)) {
                if (witnessVersion == 0 &&
                    witnessProgram.Length == (int)Witness.Ver_0_KeyHash_Size) {
                    txnType = TxnType.P2WPKH;
                } else if (witnessVersion == 0 &&
                           witnessProgram.Length == (int)Witness.Ver_0_ScriptHash_Size) {
                    txnType = TxnType.P2WSH;
                } else if (witnessVersion != 0) {
                    txnType = TxnType.Nonstandard;
                } else {
                    txnType = TxnType.Nonstandard;
                }

            }

            if (MatchPayToPubKeyHash(output.scriptPubKey)) {
                txnType = TxnType.P2PKH;
            }

            if (IsUnspendable(output.scriptPubKey)) {
                txnType = TxnType.NullPushData;
            }

            if (MatchPayToPubKey(output.scriptPubKey)) {
                txnType = TxnType.P2PK;
            }

            if (MatchRawMultiSig(output.scriptPubKey)) {
                txnType = TxnType.RawMultisig;
            }

            if (txnType == TxnType.Nonstandard ||
                txnType == TxnType.NullPushData ||
                txnType == TxnType.RawMultisig) {
                return new Address();
            }

            return new Address(output.scriptPubKey, txnType);
        }

        private static bool IsPayToScriptHash(ScriptPubKey scriptPubKey) {

            if (scriptPubKey.length == (int)PubKeySize.Hash + 3 &&
                scriptPubKey[0] == (byte)OpCode.OP_HASH160 &&
                scriptPubKey[1] == 0X14 &&
                scriptPubKey[22] == (byte)OpCode.OP_EQUAL) {
                return true;
            }

            return false;
        }

        private static bool IsWitnessProgram(ScriptPubKey scriptPubKey,
            ref int witnessVersion, ref byte[] witnessProgram) {

            if (scriptPubKey.length < 4 || scriptPubKey.length > 42) {
                return false;
            }

            if (scriptPubKey[0] != (byte)OpCode.OP_0 &&
                (scriptPubKey[0] < (byte)OpCode.OP_1) || scriptPubKey[0] > (byte)OpCode.OP_16) {
                return false;
            }

            if (scriptPubKey.length == scriptPubKey[1] + 2) {
                witnessVersion = DecodeOpN(scriptPubKey[0]);
                witnessProgram = new byte[scriptPubKey[1]];
                Array.Copy(scriptPubKey.scriptPubKey, 2, witnessProgram, 0, scriptPubKey[1]);
                return true;
            }

            return false;
        }

        private static bool MatchPayToPubKeyHash(ScriptPubKey scriptPubKey) {

            if (scriptPubKey.length == (int)PubKeySize.Hash + 5 &&
                scriptPubKey[0] == (byte)OpCode.OP_DUP &&
                scriptPubKey[1] == (byte)OpCode.OP_HASH160 &&
                scriptPubKey[2] == (int)PubKeySize.Hash &&
                scriptPubKey[23] == (byte)OpCode.OP_EQUALVERIFY &&
                scriptPubKey[24] == (byte)OpCode.OP_CHECKSIG) {
                return true;
            }

            return false;
        }

        private static bool IsUnspendable(ScriptPubKey scriptPubKey) {

            if (scriptPubKey.length >= 1 &&
            scriptPubKey[0] == (byte)OpCode.OP_RETURN) {

                if (scriptPubKey.length == 1) {
                    return true;
                }

                byte pushValue = scriptPubKey[1];
                if (scriptPubKey.length == pushValue + 2) {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchRawMultiSig(ScriptPubKey scriptPubKey) {

            if (scriptPubKey.length < 1
                || scriptPubKey[scriptPubKey.length - 1] != (byte)OpCode.OP_CHECKMULTISIG) {
                return false;
            }

            if (scriptPubKey[0] < (byte)OpCode.OP_1 || scriptPubKey[0] > (byte)OpCode.OP_16) {
                return false;
            }

            byte nRequiredKeys = DecodeOpN((byte)scriptPubKey[0]);
            byte nTotalKeys = DecodeOpN((byte)scriptPubKey[scriptPubKey.length - 1]);

            if (nRequiredKeys > nTotalKeys) {
                return false;
            }

            int pushByteIndex = 1;
            int pushSize;

            for (int i = 0; i < nTotalKeys; ++i) {
                pushSize = scriptPubKey[pushByteIndex];
                if (pushSize != (int)PubKeySize.Compressed ||
                    pushSize != (int)PubKeySize.Uncompressed) {
                    return false;
                }
                pushByteIndex += pushSize + 1;
            }

            return true;
        }

        private static bool MatchPayToPubKey(ScriptPubKey scriptPubKey) {

            if (scriptPubKey.length == (int)PubKeySize.Uncompressed + 2 &&
                scriptPubKey[0] == (int)PubKeySize.Uncompressed &&
                scriptPubKey[scriptPubKey.length - 1] == (byte)OpCode.OP_CHECKSIG) {
                return true;
            }

            if (scriptPubKey.length == (int)PubKeySize.Compressed + 2 &&
                scriptPubKey[0] == (int)PubKeySize.Compressed &&
                scriptPubKey[scriptPubKey.length - 1] == (byte)OpCode.OP_CHECKSIG) {
                return true;
            }

            return false;
        }

        public static byte[] FromScriptPubKey(ScriptPubKey scriptPubKey, TxnType txnType) {

            byte[] ret;

            switch (txnType) {

                case TxnType.P2SH:
                    ret = new byte[scriptPubKey[1]];
                    Array.Copy(scriptPubKey.scriptPubKey, 2, ret, 0, scriptPubKey[1]);
                    break;

                case TxnType.P2WPKH:
                    ret = new byte[scriptPubKey[1]];
                    Array.Copy(scriptPubKey.scriptPubKey, 2, ret, 0, scriptPubKey[1]);
                    break;

                case TxnType.P2WSH:
                    ret = new byte[scriptPubKey[1]];
                    Array.Copy(scriptPubKey.scriptPubKey, 2, ret, 0, scriptPubKey[1]);
                    break;

                case TxnType.P2PKH:
                    ret = new byte[scriptPubKey[2]];
                    Array.Copy(scriptPubKey.scriptPubKey, 3, ret, 0, scriptPubKey[2]);
                    break;

                case TxnType.P2PK:
                    ret = new byte[scriptPubKey[0]];
                    Array.Copy(scriptPubKey.scriptPubKey, 1, ret, 0, scriptPubKey[0]);
                    break;

                default:
                    ret = null;
                    break;
            }

            return ret;
        }

        private static byte DecodeOpN(byte opcode) {
            byte ret = 0;
            switch (opcode) {
                case 0x51:
                    ret = 1;
                    break;
                case 0x52:
                    ret = 2;
                    break;
                case 0x53:
                    ret = 3;
                    break;
                case 0x54:
                    ret = 4;
                    break;
                case 0x55:
                    ret = 5;
                    break;
                case 0x56:
                    ret = 6;
                    break;
                case 0x57:
                    ret = 7;
                    break;
                case 0x58:
                    ret = 8;
                    break;
                case 0x59:
                    ret = 9;
                    break;
                case 0x5a:
                    ret = 10;
                    break;
                case 0x5b:
                    ret = 11;
                    break;
                case 0x5c:
                    ret = 12;
                    break;
                case 0x5d:
                    ret = 13;
                    break;
                case 0x5e:
                    ret = 14;
                    break;
                case 0x5f:
                    ret = 15;
                    break;
                case 0x60:
                    ret = 16;
                    break;
            }

            return ret;
        }

    }

}