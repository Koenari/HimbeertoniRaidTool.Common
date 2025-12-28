using System.Diagnostics.CodeAnalysis;
using static HimbeertoniRaidTool.Common.Data.EncounterDifficulty;

namespace HimbeertoniRaidTool.Common;

/// <summary>
/// This class/file encapsulates all data that needs to be updated for new Patches of the game.
/// </summary>
[SuppressMessage("ReSharper", "CommentTypo")]
internal class CuratedData
{
    internal readonly GameExpansion[] Expansions =
    [
        new(1, MateriaLevel.None, 0),
        new(1, MateriaLevel.None, 0),
        new(2, MateriaLevel.II, 50),
        new(3, MateriaLevel.IV, 60),
        new(4, MateriaLevel.VI, 70),
        new(5, MateriaLevel.VIII, 80),
        new(6, MateriaLevel.X, 90)
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
        new(7, MateriaLevel.XII, 100)
        {
            NormalRaidTiers =
            [
                new RaidTier(Normal, 710, 700, "AAC Light-heavyweight Tier", [30135, 30137, 30139, 30141]),
                new RaidTier(Normal, 750, 740, "AAC Cruiserweight Tier", [30145, 30147, 30149, 30151]),
                new RaidTier(Normal, 790, 780, "AAC Heavyweight Tier", [30154, 30156, 30158, 30160]),
            ],
            SavageRaidTiers =
            [
                new RaidTier(Savage, 735, 730, "AAC Light-heavyweight Tier Savage", [30136, 30138, 30140, 30142]),
                new RaidTier(Savage, 765, 760, "AAC Cruiserweight Savage", [30146, 30148, 30150, 30152]),
                new RaidTier(Savage, 795, 790, "AAC Heavyweight Savage", [30155, 30157, 30159, 30161]),
            ],
        },
    ];


    internal readonly Dictionary<uint, ItemIdCollection> ItemContainerDb = new()
    {
        //6.0
        { 35734, 35245..35264 }, //Asphodelos weapon coffer
        { 35735, [35265, 35270, 35275, 35280, 35285, 35290, 35295] }, //Asphodelos headgear coffer
        { 35736, [35266, 35271, 35276, 35281, 35286, 35291, 35296] }, //Asphodelos chest gear coffer
        { 35737, [35267, 35272, 35277, 35282, 35287, 35292, 35297] }, //Asphodelos hand gear coffer
        { 35738, [35268, 35273, 35278, 35283, 35288, 35293, 35298] }, //Asphodelos leg gear coffer
        { 35739, [35269, 35274, 35279, 35284, 35289, 35294, 35299] }, //Asphodelos foot gear coffer
        { 35740, 35300..35304 }, //Asphodelos earring coffer
        { 35741, 35305..35309 }, //Asphodelos necklace coffer
        { 35742, 35310..35314 }, //Asphodelos bracelet coffer
        { 35743, 35315..35319 }, //Asphodelos ring coffers
        //6.2
        { 38390, 38081..38099 }, //Abyssos weapon coffer
        { 38391, [38101, 38106, 38111, 38116, 38121, 38126, 38131] }, //Abyssos headgear coffer
        { 38392, [38102, 38107, 38112, 38117, 38122, 38127, 38132] }, //Abyssos chest gear coffer
        { 38393, [38103, 38108, 38113, 38118, 38123, 38128, 38133] }, //Abyssos hand gear coffer
        { 38394, [38104, 38109, 38114, 38119, 38124, 38129, 38134] }, //Abyssos leg gear coffer
        { 38395, [38105, 38110, 38115, 38120, 38125, 38130, 38135] }, //Abyssos foot gear coffer
        { 38396, 38136..38140 }, //Abyssos earring coffer
        { 38397, 38141..38145 }, //Abyssos necklace coffer
        { 38398, 38146..38150 }, //Abyssos bracelet coffer
        { 38399, 38151..38155 }, //Abyssos ring coffers
        //6.4
        { 40307, 40165..40183 }, //Ascension weapon coffer
        { 40308, [40185, 40190, 40195, 40200, 40205, 40210, 40215] }, //Ascension headgear coffer
        { 40309, [40186, 40191, 40196, 40201, 40206, 40211, 40216] }, //Ascension chest gear coffer
        { 40310, [40187, 40192, 40197, 40202, 40207, 40212, 40217] }, //Ascension hand gear coffer
        { 40311, [40188, 40193, 40198, 40203, 40208, 40213, 40218] }, //Ascension leg gear coffer
        { 40312, [40189, 40194, 40199, 40204, 40209, 40214, 40219] }, //Ascension foot gear coffer
        { 40313, 40220..40224 }, //Ascension earring coffer
        { 40314, 40225..40229 }, //Ascension necklace coffer
        { 40315, 40230..40234 }, //Ascension bracelet coffer
        { 40316, 40235..40239 }, //Ascension ring coffers
        //7.0
        { 43526, 42773..42794 }, //Skyruin weapon coffer
        { 43527, 43101..43121 }, //dark horse champion's weapon coffer (IL 735)
        { 43528, [43123, 43128, 43133, 43138, 43143, 43148, 43153] }, //dark horse headgear coffer
        { 43529, [43124, 43129, 43134, 43139, 43144, 43149, 43154] }, //dark horse chest gear coffer
        { 43530, [43125, 43130, 43135, 43140, 43145, 43150, 43155] }, //dark horse hand gear coffer
        { 43531, [43126, 43131, 43136, 43141, 43146, 43151, 43156] }, //dark horse leg gear coffer
        { 43532, [43127, 43132, 43137, 43142, 43147, 43152, 43157] }, //dark horse foot gear coffer
        { 43533, 43158..43162 }, //dark horse earring coffer
        { 43534, 43163..43167 }, //dark horse necklace coffer
        { 43535, 43168..43172 }, //dark horse bracelet coffer
        { 43536, 43173..43177 }, //dark horse ring coffer
        //7.2
        { 46710, 46630..46650 }, //babyface champion's weapon coffer (IL 765)
        { 46711, [46652, 46657, 46662, 46667, 46672, 46677, 46682] }, //babyface champion's head gear coffer (IL 760)
        { 46712, [46653, 46658, 46663, 46668, 46673, 46678, 46683] }, //babyface champion's chest gear coffer (IL 760)
        { 46713, [46654, 46659, 46664, 46669, 46674, 46679, 46684] }, //babyface champion's hand gear coffer (IL 760)
        { 46714, [46655, 46660, 46665, 46670, 46675, 46680, 46685] }, //babyface champion's leg gear coffer (IL 760)
        { 46715, [46656, 46661, 46666, 46671, 46676, 46681, 46686] }, //babyface champion's foot gear coffer (IL 760)
        { 46716, 46687..46691 }, //babyface champion's earring gear coffer (IL 760)
        { 46717, 46692..46696 }, //babyface champion's necklace gear coffer (IL 760)
        { 46718, 46697..46701 }, //babyface champion's bracelet gear coffer (IL 760)
        { 46719, 46702..46706 }, //babyface champion's ring gear coffer (IL 760)
        //7.3
        { 46981, 46984..47005 }, //ageless weapon coffers (IL 755)
        //7.4
        { 49738, 49658..49678 }, //Grand Champion's Weapon Coffer (IL 795)
        { 49739, [49680, 49685, 49690, 49695, 49700, 49705, 49710] }, //Grand Champion's Head Gear Coffer (IL 790)
        { 49740, [49681, 49686, 49691, 49696, 49701, 49706, 49711] }, //Grand Champion's Chest Gear Coffer (IL 790)
        { 49741, [49682, 49687, 49692, 49697, 49702, 49707, 49712] }, //Grand Champion's Hand Gear Coffer (IL 790)
        { 49742, [49683, 49688, 49693, 49698, 49703, 49708, 49713] }, //Grand Champion's Leg Gear Coffer (IL 790)
        { 49743, [49684, 49689, 49694, 49699, 49704, 49709, 49714] }, //Grand Champion's Foot Gear Coffer (IL 790)
        { 49744, 49715..49719 }, //Grand Champion's Earring Coffer (IL 790)
        { 49745, 49720..49724 }, //Grand Champion's Necklace Coffer (IL 790)
        { 49746, 49725..49729 }, //Grand Champion's Bracelet Coffer (IL 790)
        { 49747, 49730..49735 }, //Grand Champion's Ring Coffer (IL 790)

    };

    //I only record Gear and items used to get gear
    internal readonly HashSet<InstanceWithLoot> InstanceData =
    [
        //6.0
        new(78, Normal, 34155..34229),
        new(79, Normal, 34830..34849),
        new(80, Normal, 34305..34379),
        new(81, Normal, 34810..34829),
        new(82, Normal, 34605..34679),
        new(83, Normal, 34455..34529),
        new(84, Normal, 34830..34849),
        new(85, Normal, 34830..34849),
        new(20077, Normal, 36283),
        new(20078, Extreme, 34925..34944),
        new(20079, Normal, 36275),
        new(20080, Normal, 36282),
        new(20081, Extreme, 34945..34964),
        new(30107, Normal, 35817..35822),
        //Raids
        new(30108, Savage, new ItemIdCollection(35245..35264, 35734, 35736), 35826),
        new(30109, Normal, 35817..35822),
        new(30110, Savage, new ItemIdCollection(35735, 35737, 35738, 35739, 35828, 35829), 35825),
        new(30111, Normal, 35817..35822),
        new(30112, Savage, 35740..35743, 35823),
        new(30113, Normal, 35817..35822),
        new(30114, Savage, new ItemIdCollection(35735, 35737, 35739, 35830, 35831), 35824),
        //6.1
        new(87, Normal, 37166..37227),
        new(20083, Extreme, 36923.. 36942, 36809),
        new(30106, Ultimate, 36810),
        new(30115, Normal, 37131..37165, 36820),
        //6.2
        new(88, Normal, 38156..38210),
        new(20084, Normal, 38437),
        new(20085, Extreme, 37856..37875, 38374),
        new(30116, Normal, 38375..38380),
        new(30117, Savage, 38396..38399, 38381),
        new(30118, Normal, 38375..38380),
        new(30119, Savage, new ItemIdCollection(38391, 38393, 38394, 38395, 38386, 38387), 38383),
        new(30120, Normal, 38375..38380),
        new(30121, Savage, new ItemIdCollection(38391, 38393, 38395, 38388, 38389), 38382),
        new(30122, Normal, 38375..38380, 38385),
        new(30123, Savage, new ItemIdCollection(38081..38099, 38390, 38392), 38384),
        //6.3
        new(89, Normal, 38957.. 39011), //Lapis Manalis
        new(30125, Normal, new ItemIdCollection(39089..39123, 39373, 39481, 39600, 39601, 39602), 38950), //Euphrosyne
        //6.4
        new(90, Normal, 40240..40294), //Dungeon
        new(20089, Normal), //Trial
        new(20090, Extreme, new ItemIdCollection(39940..39959, 40296), 40295), //Trial
        //Raids
        new(30126, Normal, 40297..40302),
        new(30127, Savage, 40313..40316, 40303),
        new(30128, Normal, 40297..40302),
        new(30129, Savage, new ItemIdCollection(40308, 40310, 40312, 40320, 40321), 40304),
        new(30130, Normal, 40297..40302),
        new(30131, Savage, new ItemIdCollection(40308, 40310, 40309, 40311, 40312, 40318, 40319), 40305),
        new(30132, Normal, 40297..40302, 40317),
        new(30133, Savage, new ItemIdCollection(40165..40183, 40307), 40306),
        //6.5
        new(00091, Normal, 40765..40819), //Dungeon (The Lunar Subterrane)
        new(20091, Normal), //Trial (The Abyssal Fracture)
        new(20092, Extreme, new ItemIdCollection(41033..41051, 41054), 41053),
        new(30134, Normal, 40897..40931), //Alliance Raid (Thaleia)
        //7.0
        //Dungeons
        new(00092, Normal, 42067..42143), //Worqor Zormor
        new(00093, Normal, 42529..42568), //Origenics
        new(00094, Normal, 41913..41989), //Ihuykatumu
        new(00095, Normal), //Alexandria
        new(00096, Normal, 42221..42297), //The Skydeep Cenote
        new(00097, Normal, 42375..42451), //Vanguard
        new(00098, Normal), //Tender Valley
        new(00099, Normal), //The Strayborough Deadwalk
        //Trials
        new(20093, Normal), //Worqor Lar Dor
        new(20094, Extreme, new ItemIdCollection(42773..42794, 43526), 43539), //Worqor Lar Dor
        new(20095, Normal), //The Interphos
        new(20096, Normal), //Everkeep
        new(20097, Extreme, 42795..42814, 43540), //Everkeep
        //Raids
        new(30135, Normal, 43541..43546), //M1
        new(30136, Savage, 43533..43536, 43549), //M1S
        new(30137, Normal, 43541..43546), //M2
        new(30138, Savage, new ItemIdCollection(43528, 43530, 43532, 43555, 43548), 43550), //M2S
        new(30139, Normal, new ItemIdCollection(43541..43544, 43546)), //M3
        new(30140, Savage, new ItemIdCollection(43529, 43531, 43553, 43554), 43551), //M3S
        new(30141, Normal, 43542..43546, 43547), //M4
        new(30142, Savage, new ItemIdCollection(43101..43121, 43527), 43552), //M4S
        //7.1
        new(00100, Normal, 44550..44604), //Dungeon: Yuweyawata Field Station
        new(30144, Normal, 44514..44548), //Alliance-Raid - Jeuno: The First Walk
        new(20099, Extreme, 44696..44717, 44718..44719), // The Minstrel's Ballad: Sphene's Burden
        new(64009, Unreal), //The Jade Stoa (Unreal)
        //7.2
        new(00101, Normal, 46344..46398), //Dungeon: The underkeep
        new(30145, Normal, 46721..46726), //M1
        new(30146, Savage, 46716..46719, 46732), //M1 S
        new(30147, Normal, 46721..46726), //M2
        new(30148, Savage, new ItemIdCollection(46711, 46713, 46715, 46731, 46728), 46733), //M2 S
        new(30149, Normal, 46721..46726), //M3
        new(30150, Savage, new ItemIdCollection(46712, 46714, 46730, 46729), 46734), //M3 S
        new(30151, Normal, 46721..46726), //M4
        new(30152, Savage, new ItemIdCollection(46630..46650, 46710), 46735), //M4 S
        //7.3
        new(00102, Normal, 47108..47162), //Dungeon: The Meso Terminal
        new(30153, Normal, 46860..46894), //Alliance Raid: San d'Oria: The Second Walk
        new(20104, Normal), //Trial: the Ageless Necropolis
        new(20105, Extreme, 46984..47005, 46981..46982), //Trial:The Minstrel's Ballad: Necron's Embrace
        new(64011, Unreal), //The Wreath of Snakes (Unreal)
        //7.4
        new(00103, Normal, 49372..49426), //Mistwake,
        new(30154, Normal, 49749..49754), //M1
        new(30155, Savage, 49744..49747, 49760), //M1 S
        new(30156, Normal, 49749..49754), //M2
        new(30157, Savage, new ItemIdCollection(49739, 49741, 49743, 49756, 49759), 49761), //M2 S
        new(30158, Normal, 49749..49754), //M3
        new(30159, Savage, new ItemIdCollection(49740, 49742, 49757, 49758), 49762), //M3 S
        new(30160, Normal, 49749..49754, 49755), //M4
        new(30161, Savage, new ItemIdCollection(49658..49678, 49738), 49763), //M4 S
    ];
}