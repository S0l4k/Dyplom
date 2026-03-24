using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Collections; // Dla StartCoroutine

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // Słownik do przechowywania aktywnych instancji muzyki/loopów
    private Dictionary<string, EventInstance> activeEventInstances = new Dictionary<string, EventInstance>();

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Odtwarza SFX przyjmując EventReference (przeciągane z FMOD Browser w Inspectorze).
    /// </summary>
    public void PlaySFX(EventReference eventRef, Vector3? position = null)
    {
        if (eventRef.IsNull) return;

        if (position.HasValue)
        {
            RuntimeManager.PlayOneShot(eventRef, position.Value);
        }
        else
        {
            RuntimeManager.PlayOneShot(eventRef);
        }
    }
    /// <summary>
    /// Tworzy instancję ambientu (nie startuje) - do późniejszego kontrolowania.
    /// </summary>
    public EventInstance CreateAmbientInstance(EventReference eventRef)
    {
        if (eventRef.IsNull) return default;
        return RuntimeManager.CreateInstance(eventRef);
    }

    /// <summary>
    /// Startuje ambient z pozycją 3D.
    /// </summary>
    public void StartAmbientInstance(EventInstance instance, Vector3 position)
    {
        if (!instance.isValid()) return;
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        instance.start();
    }

    /// <summary>
    /// Zatrzymuje i zwalnia ambient.
    /// </summary>
    public void StopAmbientInstance(ref EventInstance instance, bool fadeOut = true)
    {
        if (instance.isValid())
        {
            int mode = fadeOut ? 1 : 0; // 1=ALLOW_FADEOUT, 0=IMMEDIATE
            instance.stop((FMOD.Studio.STOP_MODE)mode);
            instance.release();
            instance = default;
        }
    }
    /// <summary>
    /// Odtwarza losowy dźwięk (np. szepty) i zwraca instancję do śledzenia.
    /// </summary>
    public EventInstance PlayRandomSound(EventReference eventRef, Vector3? position = null)
    {
        if (eventRef.IsNull) return default;

        var instance = RuntimeManager.CreateInstance(eventRef);

        if (position.HasValue)
        {
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(position.Value));
        }

        instance.start();
        instance.release(); // PlayOneShot style - auto release
        return instance;
    }

    /// <summary>
    /// Odtwarza muzykę lub dźwięk pętli z opcjonalnym fade-in.
    /// </summary>
    public void PlayMusic(string eventName, float fadeIn = 0f)
    {
        if (string.IsNullOrEmpty(eventName)) return;

        // Sprawdź czy już gra
        if (activeEventInstances.ContainsKey(eventName))
        {
            EventInstance existingInstance = activeEventInstances[eventName];
            existingInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING) return;
        }

        // Zatrzymaj inną muzykę
        StopAllMusic();

        // Stwórz nową instancję
        EventInstance newInstance = RuntimeManager.CreateInstance(eventName);

        // Fade In przez korutinę (bezpieczniejsze niż parametry FMOD)
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

        // Dodaj do śledzenia
        if (activeEventInstances.ContainsKey(eventName))
        {
            activeEventInstances[eventName] = newInstance;
        }
        else
        {
            activeEventInstances.Add(eventName, newInstance);
        }
    }
    /// <summary>
    /// Odtwarza głos dialogowy i zwraca instancję do kontroli (stop/release).
    /// </summary>
    public EventInstance PlayDialogVoice(EventReference eventRef)
    {
        if (eventRef.IsNull) return default;

        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        instance.start();
        return instance;
    }
    /// <summary>
    /// Odtwarza krok z parametrem Surface i pozycją 3D.
    /// </summary>
    public void PlayFootstep(EventReference eventRef, int surfaceType, Vector3 position)
    {
        if (eventRef.IsNull) return;

        var instance = RuntimeManager.CreateInstance(eventRef);
        instance.setParameterByName("Surface", surfaceType);

        // ✅ POPRAWIONE: RuntimeUtils, nie RuntimeManager
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));

        instance.start();
        instance.release();
    }
    /// <summary>
    /// Zatrzymuje i zwalnia instancję głosu dialogowego.
    /// </summary>
    public void StopDialogVoice(ref EventInstance instance, bool immediate = true)
    {
        if (instance.isValid())
        {
            int mode = immediate ? 0 : 1; // 0=IMMEDIATE, 1=ALLOW_FADEOUT
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

    /// <summary>
    /// Zatrzymuje konkretny event i zwalnia pamięć.
    /// </summary>
    /// <summary>
    /// Zatrzymuje konkretny event i zwalnia pamięć.
    /// STOP_MODE: 0 = IMMEDIATE, 1 = ALLOW_FADEOUT
    /// </summary>
    public void StopEvent(string eventName, bool fadeOut = true)
    {
        if (activeEventInstances.TryGetValue(eventName, out EventInstance instance))
        {
            // Używamy wartości int zamiast enuma, aby uniknąć konfliktów namespace'ów
            int stopMode = fadeOut ? 1 : 0; // 1 = ALLOW_FADEOUT, 0 = IMMEDIATE
            instance.stop((FMOD.Studio.STOP_MODE)stopMode);
            instance.release();
            activeEventInstances.Remove(eventName);
        }
    }

    /// <summary>
    /// Zatrzymuje wszystkie muzyki/loopy.
    /// </summary>
    public void StopAllMusic()
    {
        var keys = new List<string>(activeEventInstances.Keys);

        foreach (var key in keys)
        {
            if (activeEventInstances.TryGetValue(key, out EventInstance instance))
            {
                // 1 = ALLOW_FADEOUT dla płynnego wyciszenia
                instance.stop((FMOD.Studio.STOP_MODE)1);
                instance.release();
            }
        }
        activeEventInstances.Clear();
    }

    /// <summary>
    /// Ustawia parametr globalny w FMOD (np. "Ammo", "Health", "Weather").
    /// </summary>
    public void SetGlobalParameter(string paramName, float value)
    {
        // Poprawna ścieżka do ustawiania parametrów globalnych w FMOD Unity Integration
        RuntimeManager.StudioSystem.setParameterByName(paramName, value);
    }

    /// <summary>
    /// Pobiera wartość parametru globalnego.
    /// </summary>
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