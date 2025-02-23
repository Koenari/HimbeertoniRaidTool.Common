using System.ComponentModel;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace HimbeertoniRaidTool.Common.Data;

[JsonObject(MemberSerialization.OptIn)]
[ImmutableObject(true)]
public class FoodItem : HqItem
{
    private static readonly ExcelSheet<ItemAction> ItemActionSheet = CommonLibrary.ExcelModule.GetSheet<ItemAction>();
    private static readonly ExcelSheet<ItemFood> FoodSheet = CommonLibrary.ExcelModule.GetSheet<ItemFood>();
    private readonly ItemFood? _luminaFood;
    public FoodItem(uint id) : base(id)
    {
        if (GameItem.IsFood)
            _luminaFood = FoodSheet.GetRow(GameItem.ItemAction.Data[1]);
        IsHq = true;
    }

    public IEnumerable<StatType> StatTypesEffected
    {
        get
        {
            if (_luminaFood == null) yield break;
            foreach (var param in _luminaFood.Value.Params.Where(param => param.BaseParam.RowId != 0))
            {
                yield return (StatType)param.BaseParam.RowId;
            }
        }
    }

    public (int Value, bool IsRelative, int MaxValue) GetEffect(StatType statType)
    {
        if (_luminaFood is null) return (0, false, 0);
        foreach (var param in _luminaFood.Value.Params.Where(param => param.BaseParam.RowId == (uint)statType))
        {
            return (IsHq ? param.ValueHQ : param.Value, param.IsRelative, IsHq ? param.MaxHQ : param.Max);
        }
        return (0, false, 0);
    }

    public int ApplyEffect(StatType type, int before)
    {
        if (_luminaFood is null) return before;
        foreach (var param in _luminaFood.Value.Params.Where(param => param.BaseParam.RowId == (uint)type))
        {
            int added;
            if (param.IsRelative)
                added = before * (IsHq ? param.ValueHQ : param.Value) / 100;
            else
                added = IsHq ? param.ValueHQ : param.Value;
            added = int.Clamp(added, 0, IsHq ? param.MaxHQ : param.Max);
            return before + added;
        }
        return before;
    }
}