using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace FinalProjMonoGame;

// Central audio service: loads songs/SFX via Content, controls volumes, and plays them.
// Static state makes this a global singleton-like utility for the whole game.
public class AudioManager
{
    static ContentManager content; // Content pipeline handle
    
    // Registries for loaded assets.
    static Dictionary<string, Song> songs = new Dictionary<string, Song>();
    static Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    
    // Keep references to created SFX instances (useful if you later add StopAll/Cleanup).
    static List<SoundEffectInstance> soundEffectInstances = new List<SoundEffectInstance>();

    // For restoring volumes after toggles.
    static float prevSongVolume = 1f;
    static float prevSoundEffectVolume = 1f;

    // Hook the shared ContentManager early (e.g., Game1.LoadContent).
    public AudioManager(ContentManager contentManager)
    {
        content = contentManager;
    }
    
    // Global volume controls (clamped to [0..1])
    public static float SongVolume
    {
        get => MediaPlayer.Volume;
        set
        {
            float v = Math.Clamp(value, 0f, 1f);
            MediaPlayer.Volume = v;
            if (v > 0f) 
                prevSongVolume = v; // remember last non-zero
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
                prevSoundEffectVolume = v; // remember last non-zero
        }
    }

    // Mute toggles
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
    
    // Stops current music (if any), sets loop/volume, then plays the requested song.
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

    // Creates a fresh instance for a SFX (so multiple can overlap).
    public static SoundEffectInstance? GetSoundEffectInstance(string effectName)
    {
        if (!soundEffects.TryGetValue(effectName, out var sfx) || sfx == null)
            return null;
        return sfx.CreateInstance();
    }

    // Plays a SFX with basic params; returns the instance if caller wants to stop/modify later.
    public static SoundEffectInstance? PlaySoundEffect(string effectName, bool isLoop = true, float volume = 1f,
        float pitch = 0f, float pan = 0f)
    {
        var instance = GetSoundEffectInstance(effectName);
        if (instance == null) return null;
        
        instance.IsLooped = isLoop;
        instance.Volume = Math.Clamp(volume, 0f, 1f);
        instance.Pitch = Math.Clamp(pitch, -1f, 1f);
        instance.Pan = Math.Clamp(pan, -1f, 1f);
        
        soundEffectInstances.Add(instance); // track if you plan to stop/cleanup later
        instance.Play();
        return instance;
    }
}