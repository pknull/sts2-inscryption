using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Inscryption.InscryptionCode.Character;
using Inscryption.InscryptionCode.Extensions;

namespace Inscryption.InscryptionCode.Potions;

[Pool(typeof(InscryptionPotionPool))]
public abstract class InscryptionPotion : CustomPotionModel
{
	public override string? CustomPackedImagePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
	public override string? CustomPackedOutlinePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}