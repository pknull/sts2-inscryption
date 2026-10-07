using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>The Altar: Block for each creature sacrificed, as Feel No Pain gives Block for each card exhausted.</summary>
public sealed class AltarPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    /// <summary>Called by <see cref="Creatures.Sacrifice.Perform"/> once per creature sacrificed.</summary>
    public async Task AfterSacrifice()
    {
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
