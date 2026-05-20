using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Dictionary<string, EventInstance> activeEventInstances = new Dictionary<string, EventInstance>();

    // ============================================================
    // VOLUME CONTROL (prosta metoda - bez busów)
    // ============================================================
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    private float lastVolumeBeforeMute = 1f;
    private bool isMuted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSavedVolume();
    }

    private void LoadSavedVolume()
    {
        // ✅ Sprawdź, czy w ogóle istnieje klucz "MasterVolume"
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            // Jeśli nie ma klucza -> ustaw domyślne 1.0 i zapisz
            masterVolume = 1f;
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.Save();
        }
        else
        {
            // Jeśli klucz istnieje -> wczytaj zapisaną wartość
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        lastVolumeBeforeMute = masterVolume;
    }
    // === DODAJ TE DWIE METODY DO SWOJEGO AudioManager.cs ===

    public void SetMasterVolumeDirect(float volume)
    {
        volume = Mathf.Clamp01(volume);
        masterVolume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
        if (volume > 0.01f) { isMuted = false; lastVolumeBeforeMute = volume; }
        UpdateActiveInstancesVolume();
    }

    public void SetMuteStateDirect(bool mute)
    {
        if (mute) { lastVolumeBeforeMute = masterVolume; masterVolume = 0f; isMuted = true; }
        else { masterVolume = lastVolumeBeforeMute; isMuted = false; }
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
        UpdateActiveInstancesVolume();
    }
    public void AdjustMasterVolume(float step)
    {
        masterVolume = Mathf.Clamp01(masterVolume + step);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
        if (masterVolume > 0f) isMuted = false;

        // ✅ NOWE: zaktualizuj głośność wszystkich już grających instancji
        UpdateActiveInstancesVolume();
    }

    public void ToggleMute()
    {
        if (isMuted)
        {
            masterVolume = lastVolumeBeforeMute;
            isMuted = false;
        }
        else
        {
            lastVolumeBeforeMute = masterVolume;
            masterVolume = 0f;
            isMuted = true;
        }
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();

        // ✅ NOWE: zaktualizuj głośność wszystkich już grających instancji
        UpdateActiveInstancesVolume();
    }

    /// <summary>
    /// Aktualizuje głośność wszystkich instancji w activeEventInstances.
    /// </summary>
    private void UpdateActiveInstancesVolume()
    {
        foreach (var pair in activeEventInstances)
        {
            if (pair.Value.isValid())
            {
                pair.Value.setVolume(masterVolume);
            }
        }
    }

    public float GetMasterVolume() => masterVolume;
    public bool IsMuted() => isMuted;

    private void ApplyVolumeToInstance(EventInstance instance)
    {
        if (instance.isValid())
        {
            instance.setVolume(masterVolume);
        }
    }

    // ============================================================
    // AUDIO PLAYBACK METHODS
    // ============================================================

    public void PlaySFX(EventReference eventRef, Vector3? position = null)
    {
        if (eventRef.IsNull) return;

        var instance = RuntimeManager.CreateInstance(eventRef);

        if (position.HasValue)
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));
        }

        ApplyVolumeToInstance(instance);
        instance.start();
        instance.release();
    }

    public EventInstance CreateAmbientInstance(EventReference eventRef)
    {
        if (eventRef.IsNull) return default;

        var instance = RuntimeManager.CreateInstance(eventRef);
        ApplyVolumeToInstance(instance);
        return instance;
    }

    public void StartAmbientInstance(EventInstance instance, Vector3 position)
    {
        if (!instance.isValid()) return;
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();
    }

    public void StopAmbientInstance(ref EventInstance instance, bool fadeOut = true)
    {
        if (instance.isValid())
        {
            int mode = fadeOut ? 1 : 0;
            instance.stop((FMOD.Studio.STOP_MODE)mode);
            instance.release();
            instance = default;
        }
    }

    public EventInstance PlayRandomSound(EventReference eventRef, Vector3? position = null)
    {
        if (eventRef.IsNull) return default;

        var instance = RuntimeManager.CreateInstance(eventRef);

        if (position.HasValue)
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));
        }

        ApplyVolumeToInstance(instance);
        instance.start();
        instance.release();
        return instance;
    }

    /// <summary>
    /// Odtwarza muzykę lub dźwięk pętli z opcjonalnym fade-in.
    /// </summary>
    public void PlayMusic(EventReference eventRef, float fadeIn = 0f)
    {
        if (eventRef.IsNull) return;

        // ✅ Użyj Guid jako klucza – stabilny i unikalny
        string musicKey = eventRef.Guid.ToString();

        // Sprawdź czy ta sama muzyka już gra
        if (activeEventInstances.ContainsKey(musicKey))
        {
            EventInstance existingInstance = activeEventInstances[musicKey];
            existingInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) return;
        }

        // Zatrzymaj inną muzykę (tylko te z activeEventInstances)
        StopAllMusic();

        // Stwórz nową instancję
        EventInstance newInstance = RuntimeManager.CreateInstance(eventRef);
        ApplyVolumeToInstance(newInstance);

        if (fadeIn > 0)
        {
            newInstance.start();
            newInstance.setVolume(0f);
            StartCoroutine(FadeInCoroutine(newInstance, fadeIn));
        }
        else
        {
            newInstance.start();
        }

        // Dodaj do śledzenia pod kluczem Guid
        if (activeEventInstances.ContainsKey(musicKey))
        {
            activeEventInstances[musicKey] = newInstance;
        }
        else
        {
            activeEventInstances.Add(musicKey, newInstance);
        }
    }

    public EventInstance PlayDialogVoice(EventReference eventRef)
    {
        if (eventRef.IsNull) return default;

        var instance = RuntimeManager.CreateInstance(eventRef);
        ApplyVolumeToInstance(instance);
        instance.start();
        return instance;
    }

    public void PlayFootstep(EventReference eventRef, int surfaceType, Vector3 position)
    {
        if (eventRef.IsNull) return;

        var instance = RuntimeManager.CreateInstance(eventRef);
        instance.setParameterByName("Surface", surfaceType);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        ApplyVolumeToInstance(instance);

        instance.start();
        instance.release();
    }

    public void StopDialogVoice(ref EventInstance instance, bool immediate = true)
    {
        if (instance.isValid())
        {
            int mode = immediate ? 0 : 1;
            instance.stop((FMOD.Studio.STOP_MODE)mode);
            instance.release();
            instance = default;
        }
    }

    private IEnumerator FadeInCoroutine(EventInstance instance, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newVolume = Mathf.Clamp01(elapsed / duration);
            instance.setVolume(newVolume);
            yield return null;
        }
        instance.setVolume(1f);
    }

    public void StopEvent(string eventName, bool fadeOut = true)
    {
        if (activeEventInstances.TryGetValue(eventName, out EventInstance instance))
        {
            int stopMode = fadeOut ? 1 : 0;
            instance.stop((FMOD.Studio.STOP_MODE)stopMode);
            instance.release();
            activeEventInstances.Remove(eventName);
        }
    }

    public void StopAllMusic()
    {
        var keys = new List<string>(activeEventInstances.Keys);

        foreach (var key in keys)
        {
            if (activeEventInstances.TryGetValue(key, out EventInstance instance))
            {
                instance.stop((FMOD.Studio.STOP_MODE)1);
                instance.release();
            }
        }
        activeEventInstances.Clear();
    }

    public void SetGlobalParameter(string paramName, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName(paramName, value);
    }

    public float GetGlobalParameter(string paramName)
    {
        RuntimeManager.StudioSystem.getParameterByName(paramName, out float value);
        return value;
    }


    private void OnDestroy()
    {
        StopAllMusic();
    }
}