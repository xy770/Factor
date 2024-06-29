using Godot;
using System;

public partial class SoundManager : Node
{
    [Export]
    public Node sfx;
    [Export]
    public AudioStreamPlayer BGMPlayer;

    public enum Bus
    {
        Master = 0,
        SFX = 1,
        BGM = 2
    }

    public void PlaySFX(string name)
    {
        var player = sfx.GetNode<AudioStreamPlayer>(name);
        if(player != null)
        {
            player.Play();
        }
    }

    public void PlayBGM(AudioStream stream)
    {
        if(BGMPlayer.Stream == stream || BGMPlayer.Playing)
        {
            return;
        }
        BGMPlayer.Stream = stream;
        BGMPlayer.Play();
    }

    public float GetVolume(int busIndex)
    {
        var db = AudioServer.GetBusVolumeDb(busIndex);
        return Mathf.DbToLinear(db);
    }

    public void SetVolume(int busIndex, float volume)
    {
        AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volume));
    }
}
