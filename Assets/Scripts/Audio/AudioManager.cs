using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton { get; private set; }

    [SerializeField]
    private AudioSource effectsSource;

    [SerializeField]
    private float effectPitchShiftRange;

    private void Awake()
    {
        if (AudioManager.Singleton != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            AudioManager.Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySoundEffect(AudioClip soundEffect)
    {
        if (soundEffect == null)
            return;
        this.effectsSource.pitch = UnityEngine.Random.Range(1 - this.effectPitchShiftRange, 1 + this.effectPitchShiftRange);
        this.effectsSource.PlayOneShot(soundEffect);
    }

    internal void SetVolume(float value)
    {
        this.effectsSource.volume = value;
        return;

    }

    internal float GetEffectsVolume()
    {
        return this.effectsSource.volume;
    }
}
