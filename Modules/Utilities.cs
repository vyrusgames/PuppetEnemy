using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PuppetEnemy.Modules;

public class Utilities
{
    public enum AudioMixerGroupType
    {
        Persistent,
        Sound,
        Music,
        Microphone,
        MicrophoneSpectate,
        TTS,
        TTSSpectate
    }
    
    public static void FixMixerGroups(GameObject gameObject, AudioMixerGroupType audioGroup)
    {
        AudioSource[] audioSources = gameObject.GetComponentsInChildren<AudioSource>();
        
        AudioMixerGroup targetMixerGroup = audioGroup switch
        {
            AudioMixerGroupType.Persistent => AudioManager.instance.PersistentSoundGroup,
            AudioMixerGroupType.Sound => AudioManager.instance.SoundMasterGroup,
            AudioMixerGroupType.Music => AudioManager.instance.MusicMasterGroup,
            AudioMixerGroupType.Microphone => AudioManager.instance.MicrophoneSoundGroup,
            AudioMixerGroupType.MicrophoneSpectate => AudioManager.instance.MicrophoneSpectateGroup,
            AudioMixerGroupType.TTS => AudioManager.instance.TTSSoundGroup,
            AudioMixerGroupType.TTSSpectate => AudioManager.instance.TTSSpectateGroup,
            _ => throw new ArgumentOutOfRangeException(nameof(audioGroup), audioGroup, null)
        };

        foreach (AudioSource audioSource in audioSources)
        {
            audioSource.outputAudioMixerGroup = targetMixerGroup.audioMixer.outputAudioMixerGroup;
        }
    }
}