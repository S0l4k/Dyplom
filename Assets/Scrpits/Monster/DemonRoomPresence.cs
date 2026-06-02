using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;

[System.Serializable]
public class RoomPresenceData
{
    public string roomTag;
    public Transform spawnPoint;
    public string animationTrigger = "idle";
    public DialogNode[] dialogNodes;
}

public class DemonRoomPresence : MonoBehaviour
{
    public List<RoomPresenceData> roomPresences = new List<RoomPresenceData>();
    public DialogActivator dialogActivator;
    public Animator animator;
    public Camera mainCamera;

    [Header("Appearance Rules")]
    [Range(0f, 1f)] public float appearanceChance = 0.35f;

    [Header("Audio (FMOD)")]
    public EventReference appearSound;
    public EventReference disappearSound;

    [Header("Effects - Particles")]
    public ParticleSystem appearParticles;
    public ParticleSystem disappearParticles;

    [Tooltip("Jeśli pole powyżej jest puste, skrypt szuka dziecka o tej nazwie")]
    public string autoFindVFXName = "DemonVFX";

    [Header("Timing & Shake")]
    [Range(0.05f, 0.3f)] public float appearDelay = 0.1f;
    [Range(0.1f, 0.4f)] public float shakeDuration = 0.25f;
    [Range(0.05f, 0.2f)] public float shakeAmount = 0.1f;

    private SkinnedMeshRenderer[] renderers;
    private string currentRoomTag = "";
    private bool isBusy = false;
    private ParticleSystem currentAppearVFX;
    private ParticleSystem currentDisappearVFX;

    private void Start()
    {
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        SetVisibility(false);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (appearParticles == null)
        {
            Transform vfxRoot = transform.Find(autoFindVFXName);
            if (vfxRoot != null)
            {
                var systems = vfxRoot.GetComponentsInChildren<ParticleSystem>();
                if (systems.Length > 0) appearParticles = systems[0];
            }
        }

        if (disappearParticles == null)
            disappearParticles = appearParticles;

        currentAppearVFX = appearParticles;
        currentDisappearVFX = disappearParticles;
    }

    public void EnterRoom(string roomTag)
    {
        if (currentRoomTag == roomTag || isBusy) return;

        if (Random.value > appearanceChance)
        {
            return;
        }

        var data = roomPresences.FirstOrDefault(r => r.roomTag == roomTag);
        if (data == null) return;

        isBusy = true;

        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;
        if (!appearSound.IsNull)
            AudioManager.Instance.PlaySFX(appearSound, transform.position);

        SpawnParticles(currentAppearVFX, transform.position);

        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration, shakeAmount));

        SetVisibility(false);
        StartCoroutine(ShowAfterDelay(data, roomTag));
    }

    public bool ForceAppear(string roomTag)
    {
        if (isBusy) return false;

        var data = roomPresences.FirstOrDefault(r => r.roomTag == roomTag);
        if (data == null) return false;

        isBusy = true;

        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;

        if (!appearSound.IsNull)
            AudioManager.Instance.PlaySFX(appearSound, transform.position);

        SpawnParticles(currentAppearVFX, transform.position);

        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration, shakeAmount));

        SetVisibility(false);
        StartCoroutine(ShowAfterDelay(data, roomTag));

        return true;
    }

    public void ExitRoom()
    {
        if (string.IsNullOrEmpty(currentRoomTag) || isBusy) return;
        isBusy = true;

        if (!disappearSound.IsNull)
            AudioManager.Instance.PlaySFX(disappearSound, transform.position);

        SpawnParticles(currentDisappearVFX, transform.position);

        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration * 0.6f, shakeAmount * 0.7f));

        SetVisibility(false);
        currentRoomTag = "";
        isBusy = false;

        if (dialogActivator != null)
            dialogActivator.enabled = false;
    }

    private void SpawnParticles(ParticleSystem ps, Vector3 pos)
    {
        if (ps == null) return;

        if (ps.transform.parent != transform)
        {
            ps.transform.position = pos;
        }

        var emission = ps.emission;
        emission.enabled = true;
        ps.Play();
    }

    private IEnumerator ShowAfterDelay(RoomPresenceData data, string roomTag)
    {
        yield return new WaitForSeconds(appearDelay);

        SetVisibility(true);

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            yield return null;
            animator.SetTrigger(data.animationTrigger);
        }

        if (dialogActivator != null && data.dialogNodes != null)
        {
            dialogActivator.dialogNodes = data.dialogNodes;
            dialogActivator.enabled = true;
        }

        currentRoomTag = roomTag;
        isBusy = false;
    }

    private IEnumerator ShakeCamera(float duration, float amount)
    {
        if (mainCamera == null) yield break;

        Transform camTransform = mainCamera.transform;
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float shakeStrength = Mathf.Lerp(amount, 0f, t * t);

            camTransform.localPosition = originalPos +
                new Vector3(
                    Random.Range(-1f, 1f) * shakeStrength,
                    Random.Range(-1f, 1f) * shakeStrength,
                    0
                );

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }

    public void SetVisibility(bool visible)
    {
        foreach (var r in renderers)
            if (r != null) r.enabled = visible;

        gameObject.SetActive(true);
    }
}