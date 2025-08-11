using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace FinalProjMonoGame;

public class AudioManager
{
    static ContentManager content;
    static Dictionary<string, Song> songs = new Dictionary<string, Song>();
    static Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    static List<SoundEffectInstance> soundEffectInstances = new List<SoundEffectInstance>();

    //static float prevMusicVolume;
    static float prevSondEffectVolume;
    
    public static float SongVolume
    {
        get { return MediaPlayer.Volume; }
        set { MediaPlayer.Volume = Math.Clamp(value, 0, 1); }
    }

    public static float SoundEffectVolume
    {
        get { return SoundEffect.MasterVolume; }
        set { SoundEffect.MasterVolume = Math.Clamp(value, 0, 1); }
    }

    public static void MuteAudio()
    {
        prevSondEffectVolume = SoundEffect.MasterVolume;
        
        SoundEffect.MasterVolume = 0;
        MediaPlayer.IsMuted = true;
    }
    
    public static void UnmuteAudio()
    {
        SoundEffect.MasterVolume = prevSondEffectVolume;
        MediaPlayer.IsMuted = false;
    }
    
    public AudioManager(ContentManager contentManager)
    {
        content = contentManager;
    }
    
    public static void AddSong(string songName, string fileName)
    {
        if (!songs.ContainsKey(songName))
            songs[songName] = content.Load<Song>(fileName);
    }

    public static Song GetSong(string songName)
    {
        return songs.GetValueOrDefault(songName);
    }

    public static void PlaySong(string songName, bool isLoop = true, float volume = 1)
    {
        Song s = GetSong(songName);
        if (s == null) return;

        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Stop();

        MediaPlayer.Volume = volume;
        MediaPlayer.IsRepeating = isLoop;
        MediaPlayer.Play(s);
    }
    
    public static void AddSoundEffect(string effectName, string fileName)
    {
        if (!soundEffects.ContainsKey(effectName))
            soundEffects[effectName] = content.Load<SoundEffect>(fileName);
    }

    public static SoundEffectInstance GetSoundEffectInstance(string effectName)
    {
        return soundEffects.GetValueOrDefault(effectName).CreateInstance();
    }
    
    public static SoundEffectInstance PlaySoundEffect(string effectName, bool isLoop = true, float volume = 1)
    {
        var instance = GetSoundEffectInstance(effectName);
        
        soundEffectInstances.Add(instance);
        
        instance.Volume = volume;
        instance.IsLooped = isLoop;
        instance.Play();

        return instance;
    }
}