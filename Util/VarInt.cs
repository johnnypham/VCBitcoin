using System;
using System.IO;

namespace VCBitcoin {

    public class VarInt {

        public static void Decode(BinaryReader br, out byte firstByte, out UInt64 value) {

            firstByte = br.ReadByte();
            value = 0;

            if (firstByte < 0xFD) {
                // less than 253
                value = firstByte;
            } else if (firstByte == 0xFD) {
                // equal to 253
                value = br.ReadUInt16();
            } else if (firstByte == 0xFE) {
                // equal to 254
                value = br.ReadUInt32();
            } else if (firstByte == 0xFF) {
                // equal to 255
                value = br.ReadUInt64();
            }
        }

        public static void WriteStream(BinaryWriter bw, byte firstByte, UInt64 decodedValue) {

            bw.Write(firstByte);

            if (firstByte == 0xFD) {
                bw.Write((UInt16)decodedValue);
            } else if (firstByte == 0xFE) {
                bw.Write((UInt32)decodedValue);
            } else if (firstByte == 0xFF) {
                bw.Write(decodedValue);
            }
        }

    }

}