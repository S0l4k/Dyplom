using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity; // ✅ FMOD

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
    public EventReference appearSound;   // przypisz event np. "event:/demon/appear"

    [Header("Effects")]
    [Range(0.05f, 0.3f)] public float appearDelay = 0.1f;
    [Range(0.1f, 0.4f)] public float shakeDuration = 0.25f;
    [Range(0.05f, 0.2f)] public float shakeAmount = 0.1f;

    private SkinnedMeshRenderer[] renderers;
    private string currentRoomTag = "";
    private bool isBusy = false;

    private void Start()
    {
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        SetVisibility(false);

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void EnterRoom(string roomTag)
    {
        if (currentRoomTag == roomTag || isBusy) return;

        // ✅ RZUTUJ KOŚCIĄ
        if (Random.value > appearanceChance)
        {
            Debug.Log($"[Demon] 🎲 Skipped appearance in {roomTag} (roll: {Random.value:F2})");
            return;
        }

        var data = roomPresences.FirstOrDefault(r => r.roomTag == roomTag);
        if (data == null) return;

        Debug.Log($"[Demon] 👻 APPEARING in: {roomTag}");
        isBusy = true;

        // ✅ TELEPORT (TYLKO RAZ!)
        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;

        // ✅ DŹWIĘK POJAWIENIA SIĘ (3D, losowość w FMOD Multi Sound)
        if (!appearSound.IsNull)
        {
            RuntimeManager.PlayOneShot(appearSound, transform.position);
        }

        // ✅ SCREEN SHAKE (jak było wcześniej)
        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration, shakeAmount));

        SetVisibility(false);
        StartCoroutine(ShowAfterDelay(data, roomTag));
    }
    public void ForceAppear(string roomTag)
    {
        if (isBusy) return;

        var data = roomPresences.FirstOrDefault(r => r.roomTag == roomTag);
        if (data == null)
        {
            Debug.LogWarning($"[Demon] Brak danych dla roomTag: {roomTag}");
            return;
        }

        Debug.Log($"[Demon] 👹 WYMUSZONE pojawienie się w: {roomTag}");
        isBusy = true;

        // 📌 TELEPORT
        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;

        // 🔊 DŹWIĘK
        if (!appearSound.IsNull)
            RuntimeManager.PlayOneShot(appearSound, transform.position);

        // 📷 SHAKE
        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration, shakeAmount));

        // 👁️ POKAŻ PO OPÓŹNIENIU
        SetVisibility(false);
        StartCoroutine(ShowAfterDelay(data, roomTag));
    }

    private IEnumerator ShowAfterDelay(RoomPresenceData data, string roomTag)
    {
        yield return new WaitForSeconds(appearDelay);

        SetVisibility(true);

        // ✅ ANIMACJA (twoja sprawdzona sekwencja)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            yield return null;
            animator.SetTrigger(data.animationTrigger);
        }

        // ✅ DIALOG
        if (dialogActivator != null && data.dialogNodes != null)
        {
            dialogActivator.dialogNodes = data.dialogNodes;
            dialogActivator.enabled = true;
        }

        currentRoomTag = roomTag;
        isBusy = false;
    }

    public void ExitRoom()
    {
        if (string.IsNullOrEmpty(currentRoomTag) || isBusy) return;

        Debug.Log($"[Demon] ⬅️ Exiting: {currentRoomTag}");
        isBusy = true;



        // ✅ SCREEN SHAKE (jak było wcześniej)
        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration * 0.6f, shakeAmount * 0.7f));

        SetVisibility(false);
        currentRoomTag = "";
        isBusy = false;

        if (dialogActivator != null)
            dialogActivator.enabled = false;
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

    private void SetVisibility(bool visible)
    {
        foreach (var r in renderers)
            if (r != null) r.enabled = visible;

        gameObject.SetActive(true);
    }
}