using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using SlayTheSpire2.LAN.Multiplayer.Patchs;

// ReSharper disable ClassNeverInstantiated.Global

namespace SlayTheSpire2.LAN.Multiplayer.Helpers
{
    internal class PacketHelper
    {
        private static readonly Action<PacketWriter, int> SetPacketWriterBitPosition;

        private static readonly Action<PacketReader, int> SetPacketReaderBitPosition;

        private static readonly AccessTools.FieldRef<PacketWriter, byte[]> RefPacketWriterTempBuffer;

        private static readonly AccessTools.FieldRef<PacketReader, byte[]> RefPacketReaderTempBuffer;

        static PacketHelper()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;

            SetPacketWriterBitPosition =
                AccessTools.MethodDelegate<Action<PacketWriter, int>>(typeof(PacketWriter)
                    .GetProperty("BitPosition", flags)
                    ?.SetMethod!);
            SetPacketReaderBitPosition =
                AccessTools.MethodDelegate<Action<PacketReader, int>>(typeof(PacketReader)
                    .GetProperty("BitPosition", flags)
                    ?.SetMethod!);

            RefPacketWriterTempBuffer =
                AccessTools.FieldRefAccess<PacketWriter, byte[]>("_tempBuffer");
            RefPacketReaderTempBuffer =
                AccessTools.FieldRefAccess<PacketReader, byte[]>("_tempBuffer");
        }

        public static void WriteList<T>(PacketWriter instance, IReadOnlyList<T> list)
            where T : IPacketSerializable, new()
        {
            WriteVarInt(instance, (uint)list.Count);
            foreach (var item in list)
            {
                instance.Write(item);
            }
        }

        public static void WriteVarInt(PacketWriter writer, uint val)
        {
            var tempBuffer = RefPacketWriterTempBuffer(writer);

            var span = tempBuffer.AsSpan();

            var bytesWritten = 0;

            while (val >= 0x80)
            {
                span[bytesWritten++] = (byte)((val & 0x7F) | 0x80);
                val >>= 7;
            }

            span[bytesWritten++] = (byte)val;

            var totalBits = bytesWritten * 8;
            BitSerializationUtilWriteBytesPatch.WriteBytes(tempBuffer, writer.Buffer, writer.BitPosition, totalBits);
            SetPacketWriterBitPosition(writer, writer.BitPosition + totalBits);
        }

        public static List<T> ReadList<T>(PacketReader reader) where T : IPacketSerializable, new()
        {
            var list = new List<T>();
            var num = ReadVarInt(reader);
            for (var i = 0; i < num; i++)
            {
                list.Add(reader.Read<T>());
            }

            return list;
        }

        public static uint ReadVarInt(PacketReader reader)
        {
            var tempBuffer = RefPacketReaderTempBuffer(reader);

            uint result = 0;
            var shift = 0;

            while (true)
            {
                Array.Clear(tempBuffer);

                //7-bit variant and 1-bit flag
                BitSerializationUtilReadBitsPatch.ReadBits(reader.Buffer, reader.BitPosition, tempBuffer, 8);

                SetPacketReaderBitPosition(reader, reader.BitPosition + 8);

                result |= (uint)(tempBuffer[0] & 0x7F) << shift;

                if ((tempBuffer[0] & 0x80) == 0)
                    break;

                shift += 7;

                if (shift >= 35)
                {
                    throw new Exception("VarInt Invalid");
                }
            }

            return result;
        }
    }
}