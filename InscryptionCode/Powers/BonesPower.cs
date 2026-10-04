using MegaCrit.Sts2.Core.Entities.Powers;

namespace Inscryption.InscryptionCode.Powers;

/// <summary>Inscryption's Bones: one for each of your creatures that dies this combat.</summary>
public sealed class BonesPower : InscryptionPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
