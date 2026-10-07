using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>Boneyard: damage to every enemy whenever Luke gains Bones, as Black Hole does for the Regent's Stars.</summary>
public sealed class BoneyardPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not BonesPower || power.Owner != Owner || amount <= 0 || CombatState == null)
        {
            return;
        }
        Flash();
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner, null);
    }
}
