using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    public bool loop;

    [Range(0, 1)]
    public float volumeMultiplier = 1f;

    [HideInInspector]
    public AudioSource source;

}
