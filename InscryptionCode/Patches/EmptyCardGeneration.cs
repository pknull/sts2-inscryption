using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace Inscryption.InscryptionCode.Patches;

/// <summary>
/// <c>CardFactory.GetForCombat</c> picks from the cards that can be generated in combat and creates a null card when
/// none can, which throws mid card play and leaves the played card stuck on screen (#12: Calamity and Metamorphosis
/// ask Luke's pool for a non-Basic Attack). With no candidates it now makes no cards, as
/// <c>GetDistinctForCombat</c> already does. Pools that have candidates are untouched.
/// </summary>
[HarmonyPatch(typeof(CardFactory), nameof(CardFactory.GetForCombat),
    [typeof(Player), typeof(IEnumerable<CardModel>), typeof(int), typeof(Rng)])]
internal static class EmptyCardGeneration
{
    [HarmonyPrefix]
    private static bool Prefix(Player player, IEnumerable<CardModel> cards, ref IEnumerable<CardModel> __result)
    {
        bool multiplayer = player.RunState.Players.Count > 1;
        bool any = CardFactory.FilterForCombat(cards).Any(c => c.MultiplayerConstraint != (multiplayer
            ? CardMultiplayerConstraint.SingleplayerOnly
            : CardMultiplayerConstraint.MultiplayerOnly));
        if (any)
        {
            return true;
        }
        MainFile.Logger.Info("No cards can be generated from this pool; making none.");
        __result = [];
        return false;
    }
}
