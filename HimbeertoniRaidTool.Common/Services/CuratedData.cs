﻿using System.Collections.Generic;
using HimbeertoniRaidTool.Common.Data;
using static HimbeertoniRaidTool.Common.Data.EncounterDifficulty;

namespace HimbeertoniRaidTool.Common.Services;

/// <summary>
/// This class/file encapsulates all data that needs to be update for new Patches of the game.
/// </summary>
internal class CuratedData
{
    internal CuratedData()
    {
        CurrentExpansion = new GameExpansion(6, MateriaLevel.X, 90, 3);
        CurrentExpansion.SavageRaidTiers[0] = new RaidTier(Savage, 605, 600,
            "Asphodelos Savage", new uint[] { 30112, 30114, 30110, 30108 });
        CurrentExpansion.SavageRaidTiers[1] = new RaidTier(Savage, 635, 630,
            "Abyssos Savage", new uint[] { 30117, 30121, 30119, 30123 });
        CurrentExpansion.SavageRaidTiers[2] = new RaidTier(Savage, 665, 660,
            "Anabaseios Savage", new uint[] { 30127, 30129, 30131, 30133 });
        CurrentExpansion.NormalRaidTiers[0] = new RaidTier(Normal, 590, 580, "Asphodelos", System.Array.Empty<uint>());
        CurrentExpansion.NormalRaidTiers[1] = new RaidTier(Normal, 620, 610, "Abyssos", System.Array.Empty<uint>());
        CurrentExpansion.NormalRaidTiers[2] =
            new RaidTier(Normal, 650, 640, "Anabaseios", new uint[] { 30126, 30128, 30130, 30132 });
    }

    internal readonly GameExpansion CurrentExpansion;

    internal readonly Dictionary<uint, ItemIDCollection> ItemContainerDB = new()
    {
        //6.0
        { 35734, new ItemIDRange(35245, 35264) }, //Asphodelos weapon coffer
        { 35735, new ItemIDList(35265, 35270, 35275, 35280, 35285, 35290, 35295) }, //Asphodelos head gear coffer
        { 35736, new ItemIDList(35266, 35271, 35276, 35281, 35286, 35291, 35296) }, //Asphodelos chest gear coffer
        { 35737, new ItemIDList(35267, 35272, 35277, 35282, 35287, 35292, 35297) }, //Asphodelos hand gear coffer
        { 35738, new ItemIDList(35268, 35273, 35278, 35283, 35288, 35293, 35298) }, //Asphodelos leg gear coffer
        { 35739, new ItemIDList(35269, 35274, 35279, 35284, 35289, 35294, 35299) }, //Asphodelos foot gear coffer
        { 35740, new ItemIDRange(35300, 35304) }, //Asphodelos earring coffer
        { 35741, new ItemIDRange(35305, 35309) }, //Asphodelos necklace coffer
        { 35742, new ItemIDRange(35310, 35314) }, //Asphodelos bracelet coffer
        { 35743, new ItemIDRange(35315, 35319) }, //Asphodelos ring coffers
        //6.2
        { 38390, new ItemIDRange(38081, 38099) }, //Abyssos weapon coffer
        { 38391, new ItemIDList(38101, 38106, 38111, 38116, 38121, 38126, 38131) }, //Abyssos head gear coffer
        { 38392, new ItemIDList(38102, 38107, 38112, 38117, 38122, 38127, 38132) }, //Abyssos chest gear coffer
        { 38393, new ItemIDList(38103, 38108, 38113, 38118, 38123, 38128, 38133) }, //Abyssos hand gear coffer
        { 38394, new ItemIDList(38104, 38109, 38114, 38119, 38124, 38129, 38134) }, //Abyssos leg gear coffer
        { 38395, new ItemIDList(38105, 38110, 38115, 38120, 38125, 38130, 38135) }, //Abyssos foot gear coffer
        { 38396, new ItemIDRange(38136, 38140) }, //Abyssos earring coffer
        { 38397, new ItemIDRange(38141, 38145) }, //Abyssos necklace coffer
        { 38398, new ItemIDRange(38146, 38150) }, //Abyssos bracelet coffer
        { 38399, new ItemIDRange(38151, 38155) }, //Abyssos ring coffers
        //6.4
        { 40307, new ItemIDRange(40165, 40183) }, //Ascension weapon coffer
        { 40308, new ItemIDList(40185, 40190, 40195, 40200, 40205, 40210, 40215) }, //Ascension head gear coffer
        { 40309, new ItemIDList(40186, 40191, 40196, 40201, 40206, 40211, 40216) }, //Ascension chest gear coffer
        { 40310, new ItemIDList(40187, 40192, 40197, 40202, 40207, 40212, 40217) }, //Ascension hand gear coffer
        { 40311, new ItemIDList(40188, 40193, 40198, 40203, 40208, 40213, 40218) }, //Ascension leg gear coffer
        { 40312, new ItemIDList(40189, 40194, 40199, 40204, 40209, 40214, 40219) }, //Ascension foot gear coffer
        { 40313, new ItemIDRange(40220, 40224) }, //Ascension earring coffer
        { 40314, new ItemIDRange(40225, 40229) }, //Ascension necklace coffer
        { 40315, new ItemIDRange(40230, 40234) }, //Ascension bracelet coffer
        { 40316, new ItemIDRange(40235, 40239) }, //Ascension ring coffers
    };

    //I only record Gear and items used to get gear
    internal readonly HashSet<InstanceWithLoot> InstanceDB = new()
    {
        //6.0
        new InstanceWithLoot(78, Normal, (34155, 34229)),
        new InstanceWithLoot(79, Normal, (34830, 34849)),
        new InstanceWithLoot(80, Normal, (34305, 34379)),
        new InstanceWithLoot(81, Normal, (34810, 34829)),
        new InstanceWithLoot(82, Normal, (34605, 34679)),
        new InstanceWithLoot(83, Normal, (34455, 34529)),
        new InstanceWithLoot(84, Normal, (34830, 34849)),
        new InstanceWithLoot(85, Normal, (34830, 34849)),
        new InstanceWithLoot(20077, Normal, 36283),
        new InstanceWithLoot(20078, Extreme, (34925, 34944)),
        new InstanceWithLoot(20079, Normal, 36275),
        new InstanceWithLoot(20080, Normal, 36282),
        new InstanceWithLoot(20081, Extreme, (34945, 34964)),
        new InstanceWithLoot(30107, Normal, (35817, 35822)),
        new InstanceWithLoot(30108, Savage, new ItemIDList((35245, 35264), 35734, 35736), 35826),
        new InstanceWithLoot(30109, Normal, (35817, 35822)),
        new InstanceWithLoot(30110, Savage, new ItemIDList(35735, 35737, 35738, 35739, 35828, 35829), 35825),
        new InstanceWithLoot(30111, Normal, (35817, 35822)),
        new InstanceWithLoot(30112, Savage, new ItemIDRange(35740, 35743), 35823),
        new InstanceWithLoot(30113, Normal, (35817, 35822)),
        new InstanceWithLoot(30114, Savage, new ItemIDList(35735, 35737, 35739, 35830, 35831), 35824),
        //6.1
        new InstanceWithLoot(87, Normal, (37166, 37227)),
        new InstanceWithLoot(20083, Extreme, (36923, 36942), 36809),
        new InstanceWithLoot(30106, Ultimate, 36810),
        new InstanceWithLoot(30115, Normal, (37131, 37165), 36820),
        //6.2
        new InstanceWithLoot(88, Normal, (38156, 38210)),
        new InstanceWithLoot(20084, Normal, 38437),
        new InstanceWithLoot(20085, Extreme, (37856, 37875), 38374),
        new InstanceWithLoot(30116, Normal, (38375, 38380)),
        new InstanceWithLoot(30117, Savage, new ItemIDRange(38396, 38399), 38381),
        new InstanceWithLoot(30118, Normal, (38375, 38380)),
        new InstanceWithLoot(30119, Savage, new ItemIDList(38391, 38393, 38394, 38395, 38386, 38387), 38383),
        new InstanceWithLoot(30120, Normal, (38375, 38380)),
        new InstanceWithLoot(30121, Savage, new ItemIDList(38391, 38393, 38395, 38388, 38389), 38382),
        new InstanceWithLoot(30122, Normal, (38375, 38380), 38385),
        new InstanceWithLoot(30123, Savage, new ItemIDList((38081, 38099), 38390, 38392), 38384),
        //6.3
        //Lapis Manalis
        new InstanceWithLoot(89, Normal, (38957, 39011)),
        //Euphrosyne
        new InstanceWithLoot(30125, Normal, new ItemIDList((39089, 39123), 39373, 39481, 39600, 39601, 39602), 38950),
        //6.4
        //Dungeon
        new InstanceWithLoot(90, Normal, new ItemIDList((40240, 40294))),
        //Trial
        new InstanceWithLoot(20089, Normal, new ItemIDList()),
        new InstanceWithLoot(20090, Extreme, new ItemIDList((39940, 39959), 40296), 40295),
        //Raids
        new InstanceWithLoot(30126, Normal, (40297, 40302)),
        new InstanceWithLoot(30127, Savage, new ItemIDRange(40313, 40316), 40303),
        new InstanceWithLoot(30128, Normal, (40297, 40302)),
        new InstanceWithLoot(30129, Savage, new ItemIDList(40308, 40310, 40312, 40320, 40321), 40304),
        new InstanceWithLoot(30130, Normal, (40297, 40302)),
        new InstanceWithLoot(30131, Savage, new ItemIDList(40308, 40310, 40309, 40311, 40312, 40318, 40319), 40305),
        new InstanceWithLoot(30132, Normal, (40297, 40302), 40317),
        new InstanceWithLoot(30133, Savage, new ItemIDList((40165, 40183), 40307), 40306),
        //6.5
        //Dungeon (The Lunar Subterrane)
        new InstanceWithLoot(91, Normal, (40765, 40819)),
        //Trial (The Abyssal Fracture)
        new InstanceWithLoot(20091, Normal),
        new InstanceWithLoot(20092, Extreme, new ItemIDList((41033, 41051), 41054), 41053),
        //Alliance Raid (Thaleia)
        new InstanceWithLoot(30134, Normal, (40897, 40931)),
    };
}