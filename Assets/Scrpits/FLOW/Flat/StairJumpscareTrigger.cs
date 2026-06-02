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
        if (!GameState.FinalChase) return;

        if (!other.CompareTag("Player")) return;

        if (oneTimeUse && hasTriggered) return;

        if (isCooldown) return;

        if (requirePlayerMoving && cachedPlayer != null)
        {
            float playerSpeed = cachedPlayer.GetComponent<CharacterController>()?.velocity.magnitude
                             ?? cachedPlayer.GetComponent<Rigidbody>()?.linearVelocity.magnitude
                             ?? 0f;

            if (playerSpeed < minPlayerSpeed) return;
        }

        TriggerMiniJumpscare();
    }

    private void TriggerMiniJumpscare()
    {
        if (jumpscareDemonObject == null || jumpscareAnimator == null)
        {
            return;
        }

        hasTriggered = true;
        isCooldown = true;

        StartCoroutine(PlayJumpscareSequence());
    }

    private IEnumerator PlayJumpscareSequence()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        jumpscareDemonObject.SetActive(true);

        jumpscareAnimator.SetTrigger("mini_jumpscare");

        if (!jumpscareSFX.IsNull)
        {
            yield return new WaitForSeconds(soundDelayOffset);
            PlayJumpscareSound();
        }

        yield return new WaitForSeconds(visibleTime);

        jumpscareDemonObject.SetActive(false);

        if (!oneTimeUse)
        {
            yield return new WaitForSeconds(1f);
            isCooldown = false;
        }
    }
    private void PlayJumpscareSound()
    {
        if (AudioManager.Instance == null)
        {
            RuntimeManager.PlayOneShot(jumpscareSFX, GetSoundPosition());
            return;
        }

        if (is3DSound)
        {
            AudioManager.Instance.PlaySFX(jumpscareSFX, GetSoundPosition());
        }
        else
        {
            AudioManager.Instance.PlaySFX(jumpscareSFX, null);
        }

    }

    private Vector3 GetSoundPosition()
    {
        if (jumpscareDemonObject != null && jumpscareDemonObject.activeSelf)
            return jumpscareDemonObject.transform.position;

        if (cachedPlayer != null)
            return cachedPlayer.transform.position;

        return transform.position;
    }
    public void ResetTrigger()
    {
        hasTriggered = false;
        isCooldown = false;
        if (jumpscareDemonObject != null)
            jumpscareDemonObject.SetActive(false);
    }


}