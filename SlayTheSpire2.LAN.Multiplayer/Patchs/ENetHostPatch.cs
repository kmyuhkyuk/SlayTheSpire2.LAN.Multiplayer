using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Transport.ENet;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace SlayTheSpire2.LAN.Multiplayer.Patchs
{
    [HarmonyPatch]
    internal class ENetHostDoClientHandshakePatch
    {
        private static MethodInfo TargetMethod()
        {
            return AccessTools.AsyncMoveNext(typeof(ENetHost).GetMethod("DoClientHandshake",
                BindingFlags.Instance | BindingFlags.NonPublic));
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Fixed IdCollision HandshakeResponse will not send

            var peerDisconnectMethod = AccessTools.Method(typeof(ENetPacketPeer), "PeerDisconnect");
            var peerDisconnectLaterMethod = AccessTools.Method(typeof(ENetPacketPeer), "PeerDisconnectLater");

            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand as MethodInfo == peerDisconnectMethod)
                {
                    yield return new CodeInstruction(OpCodes.Callvirt, peerDisconnectLaterMethod);
                    continue;
                }

                yield return instruction;
            }
        }
    }
}