using UnityEngine;
using FMODUnity;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Music Settings")]
    [Tooltip("Event muzyki w FMOD (przeciągnij z FMOD Browser)")]
    public EventReference musicEvent;

    [Tooltip("Czy muzyka ma zacząć grać automatycznie na starcie sceny")]
    public bool playOnStart = true;

    [Tooltip("Czas fade-in na startie (sekundy)")]
    public float fadeInTime = 1f;

    private bool isPlaying = false;

    private void Start()
    {
        if (playOnStart && !musicEvent.IsNull)
        {
            PlayMusic();
        }
    }

    public void PlayMusic()
    {
        if (musicEvent.IsNull || AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlayMusic(musicEvent, fadeInTime);
        isPlaying = true;
    }

    public void StopMusic()
    {
        if (musicEvent.IsNull || AudioManager.Instance == null || !isPlaying) return;

        AudioManager.Instance.StopEvent(musicEvent.Guid.ToString(), true);
        isPlaying = false;
    }

    public void ChangeMusic(EventReference newMusicEvent, float fadeTime = 1f)
    {
        if (newMusicEvent.IsNull) return;

        StopMusic(); 
        musicEvent = newMusicEvent;
        fadeInTime = fadeTime; 
        PlayMusic(); 
    }

    private void OnDestroy()
    {
        if (isPlaying)
        {
            StopMusic();
        }
    }
}