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

    // for restore after toggle
    static float prevSongVolume = 1f;
    static float prevSoundEffectVolume = 1f;

    public AudioManager(ContentManager contentManager)
    {
        content = contentManager;
    }
    
    // global volume controls
    public static float SongVolume
    {
        get => MediaPlayer.Volume;
        set
        {
            float v = Math.Clamp(value, 0f, 1f);
            MediaPlayer.Volume = v;
            if (v > 0f) 
                prevSongVolume = v;
        }
    }

    public static float SoundEffectVolume
    {
        get => SoundEffect.MasterVolume;
        set
        {
            float v = Math.Clamp(value, 0f, 1f);
            SoundEffect.MasterVolume = v;
            if (v > 0f)
                prevSoundEffectVolume = v;
        }
    }

    // separate mutes for music and sfx
    public static bool MusicEnabled
    {
        get => !MediaPlayer.IsMuted;
        set => MediaPlayer.IsMuted = !value;
    }
    
    public static bool SfxEnabled
    {
        get => SoundEffect.MasterVolume > 0f;
        set
        {
            if (value)
                SoundEffect.MasterVolume = (prevSoundEffectVolume > 0f) ? prevSoundEffectVolume : 1f;
            else SoundEffect.MasterVolume = 0f;
        }
    }

    public static void ToggleMusic() => MusicEnabled = !MusicEnabled;
    public static void ToggleSfx() => SfxEnabled = !SfxEnabled;

    // content loading
    public static void AddSong(string songName, string fileName)
    {
        if (!songs.ContainsKey(songName))
            songs[songName] = content.Load<Song>(fileName);
    }

    public static void AddSoundEffect(string effectName, string fileName)
    {
        if (!soundEffects.ContainsKey(effectName))
            soundEffects[effectName] = content.Load<SoundEffect>(fileName);
    }

    // playback
    public static Song GetSong(string songName) => songs.GetValueOrDefault(songName);
    
    public static void PlaySong(string songName, bool isLoop = true, float volume = 1f)
    {
        Song s = GetSong(songName);
        if (s == null) return;

        // stops current song
        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Stop();

        MediaPlayer.IsRepeating = isLoop;
        SongVolume = volume;
        MediaPlayer.Play(s);
    }

    public static SoundEffectInstance? GetSoundEffectInstance(string effectName)
    {
        if (!soundEffects.TryGetValue(effectName, out var sfx) || sfx == null)
            return null;
        return sfx.CreateInstance();
    }

    public static SoundEffectInstance? PlaySoundEffect(string effectName, bool isLoop = true, float volume = 1f,
        float pitch = 0f, float pan = 0f)
    {
        var instance = GetSoundEffectInstance(effectName);
        if (instance == null) return null;
        
        instance.IsLooped = isLoop;
        instance.Volume = Math.Clamp(volume, 0f, 1f);
        instance.Pitch = Math.Clamp(pitch, -1f, 1f);
        instance.Pan = Math.Clamp(pan, -1f, 1f);
        
        soundEffectInstances.Add(instance);
        instance.Play();
        return instance;
    }
}