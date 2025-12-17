using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgm;

    public void PlayBGM()
    {
        if (bgm != null && !bgm.isPlaying)
            bgm.Play();
    }

    public void StopBGM()
    {
        if (bgm != null && bgm.isPlaying)
            bgm.Stop();
    }
}