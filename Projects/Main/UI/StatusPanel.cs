using Godot;
using System;

public partial class StatusPanel : HBoxContainer
{
    [Export]
    public Stats stats;
    [Export]
    public TextureProgressBar healthBar;
    [Export]
    public TextureProgressBar easedHealthBar;

    public override void _Ready()
    {
        if(stats == null)
        {
            stats = GetNode<Game>("/root/Game").playerStats;
        }
        stats.Connect("HealthChange", new Callable(this, nameof(UpdateHealth)));
        UpdateHealthWithoutAnimation();
    }

    void UpdateHealth()
    {
        var percentage = (float)stats.GetHealth() / stats.maxHealth;
        healthBar.Value = percentage;

        CreateTween().TweenProperty(easedHealthBar, "value", percentage, 0.3f);
    }

    void UpdateHealthWithoutAnimation()
    {
        var percentage = (float)stats.GetHealth() / stats.maxHealth;
        healthBar.Value = percentage;

        easedHealthBar.Value = percentage;
    }
}
