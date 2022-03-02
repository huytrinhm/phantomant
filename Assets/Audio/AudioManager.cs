using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }
    public AudioSource musicPlayer;
    public AudioSource soundEffectPlayer;

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
            this.soundEffectPlayer.volume = this._soundEffectVolume;
        }
    }

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Destroy(this.musicPlayer.gameObject);
            Destroy(this.soundEffectPlayer.gameObject);
            Destroy(this.gameObject);
        } else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(this.musicPlayer.gameObject);
            DontDestroyOnLoad(this.soundEffectPlayer.gameObject);
        }
    }
}
