using Godot;
using System;

public partial class volume_slider : HSlider
{
    private SoundManager soundManager;
    [Export]
    public StringName busName;
    public int busIndex;
    public override void _Ready()
    {
        busIndex = AudioServer.GetBusIndex(busName);

        soundManager = GetNode<SoundManager>("/root/SoundManager");
        
        Value = soundManager.GetVolume(busIndex);
    }

    public void OnValueChanged(float value)
    {
        soundManager.SetVolume(busIndex, (float)value);
        GetNode<Game>("/root/Game").SaveConfig();
    }
}
