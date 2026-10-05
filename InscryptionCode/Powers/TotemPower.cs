using MegaCrit.Sts2.Core.Entities.Powers;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>Marks on the player how many Totems stand this combat; the effect itself is in <see cref="Creatures.Sigils"/>.</summary>
public sealed class TotemPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
