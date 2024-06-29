using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

[GlobalClass]
public partial class Stats : Node
{
    [Export]
    public int maxHealth = 100;

    [Export]
    public int health;

    [Signal]
    public delegate void HealthChangeEventHandler();

    public override void _Ready()
    {
        SetHealth(maxHealth);
    }

    public void SetHealth(int v)
    {
        health = Mathf.Clamp(v, 0, maxHealth);
        EmitSignal(SignalName.HealthChange);
    }

    public void ChangeHealth(int offset)
    {
        health += offset;
        health = Mathf.Clamp(health, 0, maxHealth);
        EmitSignal(SignalName.HealthChange);
    }

    public int GetHealth()
    {
        return health;
    }

    public void ResetStats()
    {
        SetHealth(maxHealth);
    }

    public string Serializer(StatsInfo statInfo)
    {
        return JsonSerializer.Serialize(statInfo);
    }

    public void DeSerializer(string json)
    {
        StatsInfo statInfo = JsonSerializer.Deserialize<StatsInfo>(json);
        maxHealth = statInfo.maxHealth;
        health = statInfo.health;
    }

    public StatsInfo GetStatInfo()
    {
        return new StatsInfo
        {
            maxHealth = maxHealth,
            health = health
        };
    }

    public void SetStatsInfo(StatsInfo statsInfo)
    {
        maxHealth = statsInfo.maxHealth;
        SetHealth(statsInfo.health);

        GD.Print("Successfully Set"+Serializer(statsInfo));
    }
}

public class StatsInfo
{
    public string name { get; set; }
    public int health { get; set; }
    public int maxHealth { get; set; }
}