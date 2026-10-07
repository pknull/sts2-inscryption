using HarmonyLib;
using Inscryption.InscryptionCode.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Patches;

/// <summary>The Fish Hook can only be aimed at an enemy it can hook (<see cref="FishHook.Hookable"/>).</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
internal static class FishHookTargets
{
    [HarmonyPostfix]
    private static void Postfix(CardModel __instance, Creature? target, ref bool __result)
    {
        if (__result && __instance is FishHook && target != null && !FishHook.Hookable(target))
        {
            __result = false;
        }
    }
}
