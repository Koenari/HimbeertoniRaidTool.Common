namespace HimbeertoniRaidTool.Common.Data;

[JsonDictionary]
public class RolePriority : Dictionary<Role, int>
{
    public delegate bool InputIntImpl(string desc, ref int val);
    private int Max => this.Aggregate(0, (sum, x) => Math.Max(sum, x.Value));
    public int GetPriority(Role r) => TryGetValue(r, out int val) ? val : Max + 1;
    public int GetPriority(Role? r) => r is not null ? GetPriority(r.Value) : Max + 1;
    public void DrawEdit(InputIntImpl uiImplementation)
    {
        foreach (var r in Enum.GetValues<Role>())
        {
            if (!r.IsCombatRole())
            {
                Remove(r);
                continue;
            }
            if (!ContainsKey(r))
            {
                Add(r, Max + 1);
            }
            int val = this[r];
            if (uiImplementation($"{r}##RolePriority", ref val))
            {
                this[r] = Math.Max(val, 0);
            }
        }
    }
    public override string ToString()
    {

        if (Count == 0)
            return string.Join(" = ", Enum.GetValues<Role>().Where(r => r.IsCombatRole()));
        var ordered = this.ToList();
        ordered.Sort((l, r) => l.Value - r.Value);
        string result = "";
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            result += $"{ordered[i].Key} {(ordered[i].Value < ordered[i + 1].Value ? ">" : "=")} ";
        }
        result += ordered[^1].Key;
        var missing = Enum.GetValues<Role>().Where(r => !ContainsKey(r)).Where(r => r.IsCombatRole()).ToList();
        if (missing.Count == 0) return result;
        result += " > ";
        for (int j = 0; j < missing.Count - 1; j++)
        {
            result += $"{missing[j]} = ";
        }
        result += missing[^1].ToString();
        return result;
    }
}