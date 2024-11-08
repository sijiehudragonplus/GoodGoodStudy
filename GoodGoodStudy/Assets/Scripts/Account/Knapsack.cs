using Newtonsoft.Json;

partial class Account
{
    [JsonProperty]
    private Knapsack m_Knapsack = new Knapsack();

    public static Knapsack Knapsack => s_Instance!.m_Knapsack;
}

[JsonObject]
public sealed class Knapsack
{
}