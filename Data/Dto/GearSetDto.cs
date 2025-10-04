namespace HimbeertoniRaidTool.Common.Data.Dto;

public class GearSetDto(GearSet gearSet)
{
    public string Name = gearSet.Name;

    public string? Alias = gearSet.Alias;

    public DateTime TimeStamp = gearSet.TimeStamp;

    public GearItemDto? MainHand = gearSet[GearSetSlot.MainHand];
    public GearItemDto? OffHand = gearSet[GearSetSlot.OffHand];
    public GearItemDto? Head = gearSet[GearSetSlot.Head];
    public GearItemDto? Body = gearSet[GearSetSlot.Body];
    public GearItemDto? Hands = gearSet[GearSetSlot.Hands];
    public GearItemDto? Legs = gearSet[GearSetSlot.Legs];
    public GearItemDto? Feet = gearSet[GearSetSlot.Feet];
    public GearItemDto? Ear = gearSet[GearSetSlot.Ear];
    public GearItemDto? Neck = gearSet[GearSetSlot.Neck];
    public GearItemDto? RingR = gearSet[GearSetSlot.Ring1];
    public GearItemDto? RingL = gearSet[GearSetSlot.Ring2];

    public HqItemDto? Food = gearSet.Food;
}