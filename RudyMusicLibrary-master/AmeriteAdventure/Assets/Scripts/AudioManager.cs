using UnityEngine.Audio;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
        Play("MainMenu Theme");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.nameOfSound == name);
        if (s == null)
        {
            Debug.LogWarning(name + " sound clip was not found");
            return;
        }
        foreach (Sound snd in sounds)
        {
            snd.source.Stop();
        }
        s.source.Play();
    }
}
