using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class SchoolAmbienceController : MonoBehaviour
{
    [Header("Audio Settings")]
    public EventReference childrenVoicesEvent;
    public float maxDistance = 15f;
    public float minVolume = 0f;
    public float maxVolume = 1f;  // ✅ Local multiplier (nie master volume!)

    [Header("References")]
    public Transform demonPosition;
    public Transform player;

    private EventInstance _voiceInstance;
    private bool _isStarted = false;
    private float _localVolume = 1f;  // ✅ Nasz mnożnik dystansowy

    void Start()
    {
        if (player == null)
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.transform;
        }

        // ✅ Utwórz instancję przez AudioManager (obsługuje master volume)
        if (!childrenVoicesEvent.IsNull && AudioManager.Instance != null)
        {
            _voiceInstance = AudioManager.Instance.CreateAmbientInstance(childrenVoicesEvent);
            _localVolume = maxVolume;
            UpdateCombinedVolume();
        }
    }

    void Update()
    {
        if (!_voiceInstance.isValid() || player == null || demonPosition == null) return;

        // ✅ Aktualizuj pozycję 3D dla spatializacji
        if (_isStarted)
        {
            _voiceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        }

        // ✅ Oblicz volume na podstawie dystansu do demona
        float distance = Vector3.Distance(player.position, demonPosition.position);
        float t = Mathf.InverseLerp(0f, maxDistance, distance);
        float targetLocal = Mathf.Lerp(minVolume, maxVolume, t);

        // ✅ Smooth transition
        _localVolume = Mathf.MoveTowards(_localVolume, targetLocal, Time.deltaTime * 2f);

        // ✅ Zastosuj POŁĄCZONY volume: master × local
        UpdateCombinedVolume();
    }

    // ✅ Łączy master volume z AudioManager + nasz local multiplier
    private void UpdateCombinedVolume()
    {
        if (_voiceInstance.isValid() && AudioManager.Instance != null)
        {
            // Master volume (globalny) × local volume (dystansowy)
            float combined = AudioManager.Instance.GetMasterVolume() * _localVolume;
            _voiceInstance.setVolume(combined);
        }
    }

    // ✅ Start odtwarzania z pozycją 3D
    // === W SchoolAmbienceController.cs ===

    // ✅ Zmień StartPlaying() na taką wersję z retry:
    public void StartPlaying(Vector3 position)
    {
        // 1️⃣ Jeśli instancja jeszcze nie istnieje – spróbuj ją stworzyć TERAZ
        if (!_voiceInstance.isValid() && !childrenVoicesEvent.IsNull && AudioManager.Instance != null)
        {
            Debug.Log("[SchoolAmbience] 🔄 Creating voice instance on-demand...");
            _voiceInstance = AudioManager.Instance.CreateAmbientInstance(childrenVoicesEvent);
            _localVolume = maxVolume;
            UpdateCombinedVolume();
        }

        // 2️⃣ Jeśli nadal nievalid – spróbuj ponownie za 0.1s (max 3 próby)
        if (!_voiceInstance.isValid())
        {
            Debug.LogWarning("[SchoolAmbience] ⏳ Voice instance not ready, retrying...");
            StartCoroutine(TryStartPlayingDelayed(position, 3));
            return;
        }

        // 3️⃣ Start odtwarzania
        if (!_isStarted && AudioManager.Instance != null)
        {
            AudioManager.Instance.StartAmbientInstance(_voiceInstance, position);
            _isStarted = true;
            _voiceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            UpdateCombinedVolume();
            Debug.Log("[SchoolAmbience] 🎵 Voices STARTED at: " + position);
        }
    }

    // ✅ NOWA metoda: opóźnione próby startu
    private IEnumerator TryStartPlayingDelayed(Vector3 position, int maxRetries)
    {
        int attempts = 0;
        while (attempts < maxRetries && (!_voiceInstance.isValid() || !_isStarted))
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;

            if (_voiceInstance.isValid() && !_isStarted && AudioManager.Instance != null)
            {
                AudioManager.Instance.StartAmbientInstance(_voiceInstance, position);
                _isStarted = true;
                _voiceInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
                UpdateCombinedVolume();
                Debug.Log($"[SchoolAmbience] 🎵 Voices STARTED after {attempts} retries");
                yield break;
            }
        }

        if (!_voiceInstance.isValid())
            Debug.LogError("[SchoolAmbience] ❌ Failed to start voices after " + maxRetries + " attempts!");
    }

    // ✅ Fade out local volume (nie wpływa na master!)
    public void FadeOutVoices(float duration = 1f)
    {
        if (_voiceInstance.isValid())
            StartCoroutine(FadeLocalVolume(0f, duration));
    }

    private IEnumerator FadeLocalVolume(float target, float time)
    {
        float start = _localVolume;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            _localVolume = Mathf.Lerp(start, target, elapsed / time);
            UpdateCombinedVolume();
            yield return null;
        }
        _localVolume = target;
        UpdateCombinedVolume();
    }

    // ✅ Stop przez AudioManager
    public void StopVoices(bool fadeOut = true)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbientInstance(ref _voiceInstance, fadeOut);
            _isStarted = false;
        }
    }

    void OnDestroy()
    {
        if (_voiceInstance.isValid() && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbientInstance(ref _voiceInstance, true);
        }
    }
}