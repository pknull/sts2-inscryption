using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode;

public class InscryptionKeywords
{
    /// <summary>On every creature card; its hover tip explains lanes and when creatures act.</summary>
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Creature;
}
