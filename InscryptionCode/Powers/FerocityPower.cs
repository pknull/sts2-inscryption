using Inscryption.InscryptionCode.Creatures;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>
/// Luke's scaling stat for his creatures, as Focus is for the Defect's orbs: each strike by his creatures deals
/// <c>Amount</c> more damage. Creatures with no power of their own still don't strike. Luke's Strength never reaches
/// them (the creature deals the hit), so this is the stat cards, relics and potions feed instead.
/// </summary>
public sealed class FerocityPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Extra damage per strike for <paramref name="player"/>'s creatures: Ferocity plus Hunting Horn.</summary>
    public static int Of(Player? player)
    {
        var luke = player?.Creature;
        return luke == null ? 0 : luke.GetPowerAmount<FerocityPower>() + luke.GetPowerAmount<HuntingHornPower>();
    }

    // The creatures' intents show their damage per strike; show the new amount.
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            Board.RefreshIntents(Owner.Player);
        }
        return Task.CompletedTask;
    }
}
