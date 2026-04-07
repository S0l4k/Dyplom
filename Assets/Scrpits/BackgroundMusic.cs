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

    /// <summary>
    /// Uruchamia muzykę przez AudioManager.
    /// </summary>
    public void PlayMusic()
    {
        if (musicEvent.IsNull || AudioManager.Instance == null)
        {
            Debug.LogWarning("[BackgroundMusic] musicEvent or AudioManager is not assigned!");
            return;
        }

        AudioManager.Instance.PlayMusic(musicEvent, fadeInTime);
        isPlaying = true;
        Debug.Log($"[BackgroundMusic] Started: {musicEvent.Guid}");
    }

    /// <summary>
    /// Zatrzymuje muzykę przez AudioManager.
    /// </summary>
    public void StopMusic()
    {
        if (musicEvent.IsNull || AudioManager.Instance == null || !isPlaying) return;

        // ✅ Użyj Guid jako klucza – taki sam jak w AudioManager.PlayMusic
        AudioManager.Instance.StopEvent(musicEvent.Guid.ToString(), true);
        isPlaying = false;
        Debug.Log($"[BackgroundMusic] Stopped: {musicEvent.Guid}");
    }

    /// <summary>
    /// Zmień muzykę na inną (np. przy zmianie fazy gry).
    /// </summary>
    public void ChangeMusic(EventReference newMusicEvent, float fadeTime = 1f)
    {
        if (newMusicEvent.IsNull) return;

        StopMusic(); // Zatrzymaj starą
        musicEvent = newMusicEvent; // Ustaw nową
        fadeInTime = fadeTime; // Zaktualizuj fade
        PlayMusic(); // Zagraj nową
    }

    private void OnDestroy()
    {
        // Bezpieczne zatrzymanie przy niszczeniu obiektu
        if (isPlaying)
        {
            StopMusic();
        }
    }
}