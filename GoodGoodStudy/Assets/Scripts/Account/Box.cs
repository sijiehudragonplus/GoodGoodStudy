using System.Collections.Generic;
using Newtonsoft.Json;

partial class Account
{
    [JsonProperty]
    private Box m_Box = new Box();

    public static Box Box => s_Instance!.m_Box;
}

[JsonObject]
public sealed class Box
{
    [JsonProperty]
    public int Level;

    [JsonProperty]
    public List<Equipments> CachedEquipments = new List<Equipments>();

    /// <summary>
    /// 升级
    /// </summary>
    public void Upgrade()
    {
    }
}