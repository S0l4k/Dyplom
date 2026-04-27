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
    public void StartPlaying(Vector3 position)
    {
        if (_voiceInstance.isValid() && !_isStarted && AudioManager.Instance != null)
        {
            AudioManager.Instance.StartAmbientInstance(_voiceInstance, position);
            _isStarted = true;
            UpdateCombinedVolume();
        }
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