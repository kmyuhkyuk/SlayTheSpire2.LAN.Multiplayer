using System.Reflection;
using HarmonyLib;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch]
    internal class BitSerializationUtilWriteBytesPatch
    {
        private static MethodInfo TargetMethod()
        {
            return AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Serialization.BitSerializationUtil")
                .GetMethod("WriteBytes", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)!;
        }

        [HarmonyReversePatch]
        public static void WriteBytes(byte[] originBuffer, byte[] destinationBuffer, int destinationBitPosition,
            int totalBitsToWrite)
        {
            throw new NotImplementedException();
        }
    }

    [HarmonyPatch]
    internal class BitSerializationUtilReadBitsPatch
    {
        private static MethodInfo TargetMethod()
        {
            return AccessTools.TypeByName("MegaCrit.Sts2.Core.Multiplayer.Serialization.BitSerializationUtil")
                .GetMethod("ReadBits", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public)!;
        }

        [HarmonyReversePatch]
        public static void ReadBits(byte[] originBuffer, int originBitPosition, byte[] destinationBuffer,
            int totalBitsToRead)
        {
            throw new NotImplementedException();
        }
    }
}