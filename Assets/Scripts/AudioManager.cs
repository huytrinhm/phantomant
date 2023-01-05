using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }
    public AudioSource musicPlayer;
    public Sound[] musics;
    public Sound[] soundEffects;
    public List<AudioSource> pausedSources;

    private float _musicVolume = 1f;
    private float _soundEffectVolume = 1f;

    public float MusicVolume { 
        get {
            return this._musicVolume;
        }

        set {
            this._musicVolume = Mathf.Clamp(value, 0, 100)/100;
            this.musicPlayer.volume = this._musicVolume;
        }
    }

    public float SoundEffectVolume
    {
        get
        {
            return this._soundEffectVolume;
        }

        set
        {
            this._soundEffectVolume = Mathf.Clamp(value, 0, 100)/100;
            foreach (Sound s in soundEffects)
            {
                if(s.source != null)
                    s.source.volume = _soundEffectVolume;
            }
        }
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.musicPlayer.gameObject);
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        foreach (Sound s in soundEffects)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = SoundEffectVolume * s.volumeMultiplier;
            s.source.loop = s.loop;
        }
    }

    public void PlaySoundEffect(string sound)
    {
        Sound s = Array.Find(soundEffects, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.volume = SoundEffectVolume * s.volumeMultiplier;

        if (!s.source.isPlaying)
            s.source.Play();
    }

    public void StopSoundEffect(string sound)
    {
        Sound s = Array.Find(soundEffects, item => item.name == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (s.source && s.source.isPlaying)
            s.source.Stop();
    }

    public void PauseAllSoundEffect()
    {
        foreach (Sound s in soundEffects)
        {
            if (s.source.isPlaying) {
                pausedSources.Add(s.source);
                s.source.Pause();
            }
        }
    }

    public void UnPauseAllSoundEffect()
    {
        foreach (AudioSource s in pausedSources)
        {
            s.UnPause();
        }

        pausedSources.Clear();
    }

    public void StopAllSoundEffect()
    {
        foreach (Sound s in soundEffects)
        {
            if (s.source && s.source.isPlaying)
            {
                s.source.Stop();
            }
        }
    }

    public void PlayMusic(string music)
    {
        Sound s = Array.Find(musics, item => item.name == music);
        if (s == null)
        {
            Debug.LogWarning("Music: " + name + " not found!");
            return;
        }

        if (!musicPlayer.isPlaying)
        {
            StartCoroutine(FadeInMusic(0.3f, s.clip, MusicVolume * s.volumeMultiplier));
        } else if (musicPlayer.clip != s.clip)
        {
            StartCoroutine(ChangeMusic(0.3f, 0.3f, s.clip, MusicVolume * s.volumeMultiplier));
        }
    }

    public void StopMusic()
    {
        if (musicPlayer.isPlaying)
        {
            StartCoroutine(Cor_StopMusic());
        }
    }

    IEnumerator Cor_StopMusic()
    {
        yield return FadeOutMusic(0.5f);
        if (musicPlayer.isPlaying)
            musicPlayer.Stop();
    }

    IEnumerator ChangeMusic(float fadeOut, float fadeIn, AudioClip clip, float volume)
    {
        yield return FadeOutMusic(fadeOut);
        yield return FadeInMusic(fadeIn, clip, volume);
    }

    IEnumerator FadeOutMusic(float fadeOut)
    {
        float startVolume = musicPlayer.volume;
        float elapsed = 0;
        while (elapsed < fadeOut)
        {
            musicPlayer.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeOut);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        musicPlayer.volume = 0;
    }

    IEnumerator FadeInMusic(float fadeIn, AudioClip clip, float volume)
    {
        if (musicPlayer.isPlaying)
            musicPlayer.Stop();
        musicPlayer.clip = clip;
        musicPlayer.volume = 0;
        musicPlayer.Play();

        float elapsed = 0;
        while (elapsed < fadeIn)
        {
            musicPlayer.volume = Mathf.Lerp(0, volume, elapsed / fadeIn);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        musicPlayer.volume = volume;
    }
}
