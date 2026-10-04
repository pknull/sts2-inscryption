using BaseLib.Abstracts;
using Inscryption.InscryptionCode.Extensions;
using Godot;

namespace Inscryption.InscryptionCode.Character;

public class InscryptionRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Inscryption.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}