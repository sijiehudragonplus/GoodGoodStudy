using System.Collections.Generic;
using Newtonsoft.Json;

partial class Account
{
    [JsonProperty]
    private Equipments m_Equipments = new Equipments();

    public static Equipments Equipments => s_Instance!.m_Equipments;
}

[JsonObject]
public sealed class Equipments
{
    [JsonProperty]
    public Dictionary<int, Equipment> EquippedMap = new Dictionary<int, Equipment>();
}

[JsonObject]
public class Equipment
{
    [JsonProperty]
    public int InstanceID;

    [JsonProperty]
    public int ConfigID;

    [JsonProperty]
    public int Level;

    [JsonProperty]
    public Property[] Properties;
}

[JsonObject]
public class Property
{
    [JsonProperty]
    public int Type;

    [JsonProperty]
    public float Value;
}