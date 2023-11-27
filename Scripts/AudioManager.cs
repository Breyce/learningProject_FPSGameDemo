using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sound Effect")]
    public AudioSource[] soundEffectsNeedStop;
    public AudioSource[] soundEffectsNoNeedStop;
    private void Awake()
    {
        instance = this;
    }

    public void PlaySoundEffect(int soundToPlay)
    {
        if (!soundEffectsNeedStop[soundToPlay].isPlaying)
            //再播放
            soundEffectsNeedStop[soundToPlay].Play();
    }

    public void PlayNoNeedStopSound(int soundIndex)
    {
        soundEffectsNoNeedStop[soundIndex].Play();
    }

    public void StopSoundEffect(int soundToPlay)
    {
        if (soundEffectsNeedStop[soundToPlay].isPlaying)
            //再播放
            soundEffectsNeedStop[soundToPlay].Stop();
    }

    public void PauseSoundEffect()
    {
        for (int i = 0; i < soundEffectsNeedStop.Length; i++)
        {
            if (soundEffectsNeedStop[i].isPlaying)
                //再播放
                soundEffectsNeedStop[i].Pause();
        }
    }

    public void PauseAllSound()
    {
        for (int i = 0; i < soundEffectsNeedStop.Length; i++)
        {
            if (soundEffectsNeedStop[i].isPlaying)
                //停止
                soundEffectsNeedStop[i].Pause();
        }
        for (int i = 0; i < soundEffectsNoNeedStop.Length; i++)
        {
            if (soundEffectsNoNeedStop[i].isPlaying)
                //再播放
                soundEffectsNoNeedStop[i].Pause();
        }
    }
}
