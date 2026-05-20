using UnityEngine;
using System.Collections;
using FMODUnity;

public class StairJumpscareTrigger : MonoBehaviour
{
    [Header("Demon Object")]
    [Tooltip("Obiekt demona z animacją (domyślnie wyłączony w Inspectorze)")]
    public GameObject jumpscareDemonObject;

    [Tooltip("Animator na obiekcie demona (musi mieć trigger 'mini_jumpscare')")]
    public Animator jumpscareAnimator;

    [Header("🔊 Jumpscare Sound (FMOD)")]
    [Tooltip("Event FMOD do odtworzenia przy mini-jumpscare")]
    public EventReference jumpscareSFX;

    [Tooltip("Czy dźwięk ma być 3D (pozycyjny), czy 2D (globalny)?")]
    public bool is3DSound = true;

    [Header("Timing")]
    [Tooltip("Jak długo demon jest widoczny przed zniknięciem")]
    public float visibleTime = 0.8f;

    [Tooltip("Opóźnienie przed pokazaniem demona po wejściu w trigger")]
    public float delayBeforeShow = 0.1f;

    [Tooltip("Opóźnienie dźwięku względem pojawienia się demona")]
    [Range(0f, 0.5f)] public float soundDelayOffset = 0f;

    [Header("Settings")]
    [Tooltip("Czy trigger dezaktywuje się po jednorazowym użyciu?")]
    public bool oneTimeUse = true;

    [Tooltip("Czy gracz musi się PORUSZAĆ (nie stać), żeby trigger zadziałał?")]
    public bool requirePlayerMoving = false;

    [Tooltip("Minimalna prędkość gracza, żeby trigger zadziałał (gdy requirePlayerMoving = true)")]
    public float minPlayerSpeed = 2f;

    private bool hasTriggered = false;
    private bool isCooldown = false;
    private PlayerController cachedPlayer;

    private void Start()
    {
        cachedPlayer = FindObjectOfType<PlayerController>();

        if (jumpscareDemonObject != null)
            jumpscareDemonObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ✅ WARUNEK 1: Tylko podczas FinalChase
        if (!GameState.FinalChase) return;

        // ✅ WARUNEK 2: Tylko gracz
        if (!other.CompareTag("Player")) return;

        // ✅ WARUNEK 3: One-time use
        if (oneTimeUse && hasTriggered) return;

        // ✅ WARUNEK 4: Cooldown
        if (isCooldown) return;

        // ✅ WARUNEK 5: Optional - gracz musi się poruszać
        if (requirePlayerMoving && cachedPlayer != null)
        {
            // ✅ Sprawdź prędkość gracza (uniwersalne, działa z każdym PlayerController)
            float playerSpeed = cachedPlayer.GetComponent<CharacterController>()?.velocity.magnitude
                             ?? cachedPlayer.GetComponent<Rigidbody>()?.linearVelocity.magnitude
                             ?? 0f;

            if (playerSpeed < minPlayerSpeed) return;
        }

        // ✅ TRIGGER AKTYWNY
        TriggerMiniJumpscare();
    }

    private void TriggerMiniJumpscare()
    {
        if (jumpscareDemonObject == null || jumpscareAnimator == null)
        {
            Debug.LogWarning($"[StairJumpscare] {name} - missing demon references!");
            return;
        }

        hasTriggered = true;
        isCooldown = true;

        StartCoroutine(PlayJumpscareSequence());
    }

    private IEnumerator PlayJumpscareSequence()
    {
        // 1. Opóźnienie przed pokazaniem
        yield return new WaitForSeconds(delayBeforeShow);

        // 2. Pokaż obiekt demona
        jumpscareDemonObject.SetActive(true);

        // 3. Odpal animację
        jumpscareAnimator.SetTrigger("mini_jumpscare");

        // 🔊 4. Odtwórz dźwięk (z opcjonalnym opóźnieniem)
        if (!jumpscareSFX.IsNull)
        {
            yield return new WaitForSeconds(soundDelayOffset);
            PlayJumpscareSound();
        }

        // 5. Czekaj aż animacja się odtworzy
        yield return new WaitForSeconds(visibleTime);

        // 6. Schowaj obiekt
        jumpscareDemonObject.SetActive(false);

        // 7. Reset cooldown (jeśli nie one-time-use)
        if (!oneTimeUse)
        {
            yield return new WaitForSeconds(1f);
            isCooldown = false;
        }
    }

    // 🔊 METODA: Odtwarzanie dźwięku - DOPASOWANA DO TWOJEGO AudioManager
    private void PlayJumpscareSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[StairJumpscare] AudioManager.Instance is null! Using fallback.");
            RuntimeManager.PlayOneShot(jumpscareSFX, GetSoundPosition());
            return;
        }

        // ✅ PlaySFX przyjmuje tylko (EventReference, Vector3?) - bez volumeMultiplier
        if (is3DSound)
        {
            AudioManager.Instance.PlaySFX(jumpscareSFX, GetSoundPosition());
        }
        else
        {
            // 2D sound - null jako pozycja
            AudioManager.Instance.PlaySFX(jumpscareSFX, null);
        }

        Debug.Log($"[StairJumpscare] 🔊 Playing: {jumpscareSFX.Guid} | 3D:{is3DSound}");
    }

    // 🔊 Helper: Pozycja dla dźwięku 3D
    private Vector3 GetSoundPosition()
    {
        if (jumpscareDemonObject != null && jumpscareDemonObject.activeSelf)
            return jumpscareDemonObject.transform.position;

        if (cachedPlayer != null)
            return cachedPlayer.transform.position;

        return transform.position;
    }

    // ✅ Reset triggera
    public void ResetTrigger()
    {
        hasTriggered = false;
        isCooldown = false;
        if (jumpscareDemonObject != null)
            jumpscareDemonObject.SetActive(false);
    }


}