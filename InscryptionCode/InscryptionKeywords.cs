using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Inscryption.InscryptionCode;

public class InscryptionKeywords
{
    /// <summary>On every creature card; its hover tip explains lanes and when creatures act.</summary>
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Creature;

    // Tribes (Totems grant sigils by tribe).
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword CanineTribe;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword HoovedTribe;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword ReptileTribe;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword AvianTribe;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword InsectTribe;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword SquirrelTribe;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword EveryTribe;

    // Sigils.
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Airborne;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword MightyLeap;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword BifurcatedStrike;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword TrifurcatedStrike;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Leader;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Stinky;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword SharpQuills;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword TouchOfDeath;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Waterborne;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Burrower;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Guardian;
    [CustomEnum] [KeywordProperties(AutoKeywordPosition.Before)] public static CardKeyword Sprinter;
}
