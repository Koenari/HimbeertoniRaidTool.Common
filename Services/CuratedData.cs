using static HimbeertoniRaidTool.Common.Data.EncounterDifficulty;

namespace HimbeertoniRaidTool.Common.Services;

/// <summary>
/// This class/file encapsulates all data that needs to be update for new Patches of the game.
/// </summary>
internal class CuratedData
{
    internal readonly GameExpansion[] Expansions =
    [
        new GameExpansion(1, MateriaLevel.None, 0),
        new GameExpansion(2, MateriaLevel.II, 50),
        new GameExpansion(3, MateriaLevel.IV, 60),
        new GameExpansion(4, MateriaLevel.VI, 70),
        new GameExpansion(5, MateriaLevel.VIII, 80),
        new GameExpansion(6, MateriaLevel.X, 90)
        {
            NormalRaidTiers =
            [
                new RaidTier(Normal, 590, 580, "Asphodelos", []),
                new RaidTier(Normal, 620, 610, "Abyssos", []),
                new RaidTier(Normal, 650, 640, "Anabaseios", [30126, 30128, 30130, 30132]),
            ],
            SavageRaidTiers =
            [
                new RaidTier(Savage, 605, 600, "Asphodelos Savage", [30112, 30114, 30110, 30108]),
                new RaidTier(Savage, 635, 630, "Abyssos Savage", [30117, 30121, 30119, 30123]),
                new RaidTier(Savage, 665, 660, "Anabaseios Savage", [30127, 30129, 30131, 30133]),
            ],
        },
        new GameExpansion(7, MateriaLevel.XII, 100)
        {
            NormalRaidTiers =
            [
                new RaidTier(Normal, 710, 700, "AAC Light-heavyweight Tier", [30135, 30137, 30139, 30141]),
            ],
            SavageRaidTiers =
            [
                new RaidTier(Savage, 735, 730, "AAC Light-heavyweight Tier Savage", [30136, 30138, 30140, 30142]),
            ],
        },
    ];


    internal readonly Dictionary<uint, ItemIdCollection> ItemContainerDb = new()
    {
        //6.0
        { 35734, 35245..35264 }, //Asphodelos weapon coffer
        { 35735, new ItemIdList(35265, 35270, 35275, 35280, 35285, 35290, 35295) }, //Asphodelos head gear coffer
        { 35736, new ItemIdList(35266, 35271, 35276, 35281, 35286, 35291, 35296) }, //Asphodelos chest gear coffer
        { 35737, new ItemIdList(35267, 35272, 35277, 35282, 35287, 35292, 35297) }, //Asphodelos hand gear coffer
        { 35738, new ItemIdList(35268, 35273, 35278, 35283, 35288, 35293, 35298) }, //Asphodelos leg gear coffer
        { 35739, new ItemIdList(35269, 35274, 35279, 35284, 35289, 35294, 35299) }, //Asphodelos foot gear coffer
        { 35740, 35300..35304 }, //Asphodelos earring coffer
        { 35741, 35305..35309 }, //Asphodelos necklace coffer
        { 35742, 35310..35314 }, //Asphodelos bracelet coffer
        { 35743, 35315..35319 }, //Asphodelos ring coffers
        //6.2
        { 38390, 38081..38099 }, //Abyssos weapon coffer
        { 38391, new ItemIdList(38101, 38106, 38111, 38116, 38121, 38126, 38131) }, //Abyssos head gear coffer
        { 38392, new ItemIdList(38102, 38107, 38112, 38117, 38122, 38127, 38132) }, //Abyssos chest gear coffer
        { 38393, new ItemIdList(38103, 38108, 38113, 38118, 38123, 38128, 38133) }, //Abyssos hand gear coffer
        { 38394, new ItemIdList(38104, 38109, 38114, 38119, 38124, 38129, 38134) }, //Abyssos leg gear coffer
        { 38395, new ItemIdList(38105, 38110, 38115, 38120, 38125, 38130, 38135) }, //Abyssos foot gear coffer
        { 38396, 38136..38140 }, //Abyssos earring coffer
        { 38397, 38141..38145 }, //Abyssos necklace coffer
        { 38398, 38146..38150 }, //Abyssos bracelet coffer
        { 38399, 38151..38155 }, //Abyssos ring coffers
        //6.4
        { 40307, new ItemIdRange(40165, 40183) }, //Ascension weapon coffer
        { 40308, new ItemIdList(40185, 40190, 40195, 40200, 40205, 40210, 40215) }, //Ascension head gear coffer
        { 40309, new ItemIdList(40186, 40191, 40196, 40201, 40206, 40211, 40216) }, //Ascension chest gear coffer
        { 40310, new ItemIdList(40187, 40192, 40197, 40202, 40207, 40212, 40217) }, //Ascension hand gear coffer
        { 40311, new ItemIdList(40188, 40193, 40198, 40203, 40208, 40213, 40218) }, //Ascension leg gear coffer
        { 40312, new ItemIdList(40189, 40194, 40199, 40204, 40209, 40214, 40219) }, //Ascension foot gear coffer
        { 40313, 40220..40224 }, //Ascension earring coffer
        { 40314, 40225..40229 }, //Ascension necklace coffer
        { 40315, 40230..40234 }, //Ascension bracelet coffer
        { 40316, 40235..40239 }, //Ascension ring coffers
        //7.0
        { 43526, 42773..42794 }, //Skyruin weapon coffer
        { 43527, 43101..43121 }, //dark horse champion's weapon coffer (IL 735)
        { 43528, new ItemIdList(43123, 43128, 43133, 43138, 43143, 43148, 43153) }, //dark horse head gear coffer
        { 43529, new ItemIdList(43124, 43129, 43134, 43139, 43144, 43149, 43154) }, //dark horse chest gear coffer
        { 43530, new ItemIdList(43125, 43130, 43135, 43140, 43145, 43150, 43155) }, //dark horse hand gear coffer
        { 43531, new ItemIdList(43126, 43131, 43136, 43141, 43146, 43151, 43156) }, //dark horse leg gear coffer
        { 43532, new ItemIdList(43127, 43132, 43137, 43142, 43147, 43152, 43157) }, //dark horse foot gear coffer
        { 43533, 43158..43162 }, //dark horse earring coffer
        { 43534, 43163..43167 }, //dark horse necklace coffer
        { 43535, 43168..43172 }, //dark horse bracelet coffer
        { 43536, 43173..43177 }, //dark horse ring coffer
    };

    //I only record Gear and items used to get gear
    internal readonly HashSet<InstanceWithLoot> InstanceDb =
    [
        //6.0
        new InstanceWithLoot(78, Normal, 34155..34229),
        new InstanceWithLoot(79, Normal, 34830..34849),
        new InstanceWithLoot(80, Normal, 34305..34379),
        new InstanceWithLoot(81, Normal, 34810..34829),
        new InstanceWithLoot(82, Normal, 34605..34679),
        new InstanceWithLoot(83, Normal, 34455..34529),
        new InstanceWithLoot(84, Normal, 34830..34849),
        new InstanceWithLoot(85, Normal, 34830..34849),
        new InstanceWithLoot(20077, Normal, 36283),
        new InstanceWithLoot(20078, Extreme, 34925..34944),
        new InstanceWithLoot(20079, Normal, 36275),
        new InstanceWithLoot(20080, Normal, 36282),
        new InstanceWithLoot(20081, Extreme, 34945..34964),
        new InstanceWithLoot(30107, Normal, 35817..35822),
        //Raids
        new InstanceWithLoot(30108, Savage, new ItemIdList(35245..35264, 35734, 35736), 35826),
        new InstanceWithLoot(30109, Normal, (35817, 35822)),
        new InstanceWithLoot(30110, Savage, new ItemIdList(35735, 35737, 35738, 35739, 35828, 35829), 35825),
        new InstanceWithLoot(30111, Normal, (35817, 35822)),
        new InstanceWithLoot(30112, Savage, 35740..35743, 35823),
        new InstanceWithLoot(30113, Normal, (35817, 35822)),
        new InstanceWithLoot(30114, Savage, new ItemIdList(35735, 35737, 35739, 35830, 35831), 35824),
        //6.1
        new InstanceWithLoot(87, Normal, (37166, 37227)),
        new InstanceWithLoot(20083, Extreme, (36923, 36942), 36809),
        new InstanceWithLoot(30106, Ultimate, 36810),
        new InstanceWithLoot(30115, Normal, (37131, 37165), 36820),
        //6.2
        new InstanceWithLoot(88, Normal, 38156..38210),
        new InstanceWithLoot(20084, Normal, 38437),
        new InstanceWithLoot(20085, Extreme, 37856..37875, 38374),
        new InstanceWithLoot(30116, Normal, 38375..38380),
        new InstanceWithLoot(30117, Savage, 38396..38399, 38381),
        new InstanceWithLoot(30118, Normal, 38375..38380),
        new InstanceWithLoot(30119, Savage, new ItemIdList(38391, 38393, 38394, 38395, 38386, 38387), 38383),
        new InstanceWithLoot(30120, Normal, 38375..38380),
        new InstanceWithLoot(30121, Savage, new ItemIdList(38391, 38393, 38395, 38388, 38389), 38382),
        new InstanceWithLoot(30122, Normal, 38375..38380, 38385),
        new InstanceWithLoot(30123, Savage, new ItemIdList(38081..38099, 38390, 38392), 38384),
        //6.3
        //Lapis Manalis
        new InstanceWithLoot(89, Normal, (38957, 39011)),
        //Euphrosyne
        new InstanceWithLoot(30125, Normal, new ItemIdList(39089..39123, 39373, 39481, 39600, 39601, 39602), 38950),
        //6.4
        //Dungeon
        new InstanceWithLoot(90, Normal, 40240..40294),
        //Trial
        new InstanceWithLoot(20089),
        new InstanceWithLoot(20090, Extreme, new ItemIdList(39940..39959, 40296), 40295),
        //Raids
        new InstanceWithLoot(30126, Normal, 40297..40302),
        new InstanceWithLoot(30127, Savage, 40313..40316, 40303),
        new InstanceWithLoot(30128, Normal, 40297..40302),
        new InstanceWithLoot(30129, Savage, new ItemIdList(40308, 40310, 40312, 40320, 40321), 40304),
        new InstanceWithLoot(30130, Normal, 40297..40302),
        new InstanceWithLoot(30131, Savage, new ItemIdList(40308, 40310, 40309, 40311, 40312, 40318, 40319), 40305),
        new InstanceWithLoot(30132, Normal, 40297..40302, 40317),
        new InstanceWithLoot(30133, Savage, new ItemIdList(40165..40183, 40307), 40306),
        //6.5
        //Dungeon (The Lunar Subterrane)
        new InstanceWithLoot(91, Normal, (40765, 40819)),
        //Trial (The Abyssal Fracture)
        new InstanceWithLoot(20091, Normal),
        new InstanceWithLoot(20092, Extreme, new ItemIdList(41033..41051, 41054), 41053),
        //Alliance Raid (Thaleia)
        new InstanceWithLoot(30134, Normal, 40897..40931),
        //7.0
        //Dungeons
        //Worqor Zormor
        new InstanceWithLoot(92, Normal, 42067..42143),
        //Origenics
        new InstanceWithLoot(93, Normal, 42529..42568),
        //Ihuykatumu
        new InstanceWithLoot(94, Normal, 41913..41989),
        //Alexandria
        new InstanceWithLoot(95),
        //The Skydeep Cenote
        new InstanceWithLoot(96, Normal, 42221..42297),
        //Vanguard
        new InstanceWithLoot(97, Normal, 42375..42451),
        //Tender Valley
        new InstanceWithLoot(98),
        //The Strayborough Deadwalk
        new InstanceWithLoot(99),
        //Trials
        //Worqor Lar Dor
        new InstanceWithLoot(20093),
        new InstanceWithLoot(20094, Extreme, new ItemIdList(42773..42794, 43526), 43539),
        //The Interphos
        new InstanceWithLoot(20095),
        //Everkeep
        new InstanceWithLoot(20096, Normal),
        new InstanceWithLoot(20097, Extreme, 42795..42814, 43540),
        //Raids
        new InstanceWithLoot(30135, Normal, 43541..43546), //M1
        new InstanceWithLoot(30136, Savage, 43533..43536, 43549), //M1S
        new InstanceWithLoot(30137, Normal, 43541..43546), //M2
        new InstanceWithLoot(30138, Savage, new ItemIdList(43528, 43530, 43532, 43553, 43548), 43550), //M2S
        new InstanceWithLoot(30139, Normal, new ItemIdList(43541..43544, 43546)), //M3
        new InstanceWithLoot(30140, Savage, new ItemIdList(43529, 43531, 43553, 43554), 43551), //M3S
        new InstanceWithLoot(30141, Normal, 43542..43546, 43547), //M4
        new InstanceWithLoot(30142, Savage, new ItemIdList(43101..43121, 43527), 43550), //M4S

    ];
}