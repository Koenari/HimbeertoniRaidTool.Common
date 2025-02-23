using System.ComponentModel;
using HimbeertoniRaidTool.Common.GameData;
using HimbeertoniRaidTool.Common.Localization;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class MateriaItem(uint id) : Item(id)
{
    private static readonly ExcelSheet<Materia> MateriaSheet = CommonLibrary.ExcelModule.GetSheet<Materia>();


    [JsonConstructor] //This is used to convert old data to new id based item data
    public MateriaItem(uint category, byte materiaLevel) : this((MateriaCategory)category, (MateriaLevel)materiaLevel)
    {
    }

    public MateriaItem(MateriaCategory category, MateriaLevel materiaLevel) : this(
        GameItem.ReverseMateriaLookup[((uint)category, materiaLevel)])
    {
    }

    public static string DataTypeNameStatic => CommonLoc.DataTypeName_materia;

    public new string DataTypeName => DataTypeNameStatic;

    public MateriaCategory Category => GameItem.MateriaLookup[Id].cat;

    public MateriaLevel Level => GameItem.MateriaLookup[Id].level;

    public StatType StatType => Category.GetStatType();

    public int GetStat() => Category != MateriaCategory.None && Level != MateriaLevel.None
        ? MateriaSheet.GetRow((uint)Category).Value[(byte)Level] : 0;

}