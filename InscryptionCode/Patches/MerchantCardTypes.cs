using HarmonyLib;
using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Patches;

/// <summary>
/// The shop stocks two Attacks, two Skills and a Power from the character's pool, and throws when the pool has no
/// card of a slot's type. Luke's pool is mostly creatures (its only Powers are the Totems), so for him a missing type
/// falls back to one the pool does have. Other characters are untouched.
/// </summary>
[HarmonyPatch(typeof(CardFactory), nameof(CardFactory.CreateForMerchant),
    [typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardType)])]
internal static class MerchantCardTypes
{
    [HarmonyPrefix]
    private static void Prefix(Player player, IEnumerable<CardModel> options, ref CardType type)
    {
        if (!Board.UsesLanes(player))
        {
            return;
        }
        var stocked = options.Where(c => c.Rarity != CardRarity.Basic).Select(c => c.Type).ToHashSet();
        if (stocked.Count == 0 || stocked.Contains(type))
        {
            return;
        }
        type = stocked.Contains(CardType.Skill) ? CardType.Skill : stocked.First();
    }
}
