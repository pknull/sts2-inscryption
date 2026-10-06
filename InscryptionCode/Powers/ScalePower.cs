using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>
/// Inscryption's scale, where health is the teeth that tip with the damage each side deals: Luke heals half of the
/// HP that he and his creatures take from enemies (rounded down per hit; Block and overkill don't count). Given at
/// the start of every combat by the Side Deck.
/// </summary>
public sealed class ScalePower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldPlayVfx => false;

    // Every listener hears every hit: heal only for this Luke's own damage and his creatures'.
    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == null || (dealer != Owner && dealer.PetOwner?.Creature != Owner) || !result.Receiver.IsEnemy
            || Owner.IsDead)
        {
            return;
        }
        int heal = result.UnblockedDamage / 2;
        if (heal <= 0)
        {
            return;
        }
        int before = Owner.CurrentHp;
        await CreatureCmd.Heal(Owner, heal);
        MainFile.Logger.Info($"Scale: {dealer.Monster?.Id.Entry ?? "Luke"} took {result.UnblockedDamage} HP from {result.Receiver.Monster?.Id.Entry}; Luke heals {heal}, HP {before} -> {Owner.CurrentHp}");
    }
}
