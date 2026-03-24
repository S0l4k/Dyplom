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
    public EventReference appearSound;   // np. "event:/demon/appear"
    public EventReference disappearSound; // Opcjonalnie: dźwięk przy znikaniu

    [Header("Effects - Particles")]
    public ParticleSystem appearParticles;   // Przypisz tutaj swój prefab lub efekt z childa
    public ParticleSystem disappearParticles; // Przypisz tutaj efekt znikania

    // Jeśli nie przypiszesz w Inspectorze, skrypt spróbuje znaleźć dziecko o nazwie "VFX"
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

        // 🔍 AUTOMATYCZNE WYSZUKANIE CZĄSTECZEK, JEŚLI NIE PRZYPISANO
        if (appearParticles == null)
        {
            Transform vfxRoot = transform.Find(autoFindVFXName);
            if (vfxRoot != null)
            {
                var systems = vfxRoot.GetComponentsInChildren<ParticleSystem>();
                if (systems.Length > 0) appearParticles = systems[0];
            }
        }

        // Domyślnie znikanie to ten sam efekt co pojawianie, jeśli nie ustawiono innego
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
            Debug.Log($"[Demon] 🎲 Skipped appearance in {roomTag}");
            return;
        }

        var data = roomPresences.FirstOrDefault(r => r.roomTag == roomTag);
        if (data == null) return;

        Debug.Log($"[Demon] 👻 APPEARING in: {roomTag}");
        isBusy = true;

        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;

        // 🔊 DŹWIĘK
        if (!appearSound.IsNull)
            RuntimeManager.PlayOneShot(appearSound, transform.position);

        // ✨ CZĄSTECZKI POJAWIENIA
        SpawnParticles(currentAppearVFX, transform.position);

        // 📷 SHAKE
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

        Debug.Log($"[Demon] 👹 FORCE APPEAR in: {roomTag}");
        isBusy = true;

        transform.position = data.spawnPoint.position;
        transform.rotation = data.spawnPoint.rotation;

        if (!appearSound.IsNull)
            RuntimeManager.PlayOneShot(appearSound, transform.position);

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

        Debug.Log($"[Demon] ⬅️ Exiting: {currentRoomTag}");
        isBusy = true;

        // 🔊 DŹWIĘK ZNIKANIA (opcjonalnie)
        if (!disappearSound.IsNull)
            RuntimeManager.PlayOneShot(disappearSound, transform.position);

        // ✨ CZĄSTECZKI ZNIKANIA
        SpawnParticles(currentDisappearVFX, transform.position);

        // 📷 MNIEJSZY SHAKE PRZY ZNIKA NIU
        if (mainCamera != null)
            StartCoroutine(ShakeCamera(shakeDuration * 0.6f, shakeAmount * 0.7f));

        SetVisibility(false);
        currentRoomTag = "";
        isBusy = false;

        if (dialogActivator != null)
            dialogActivator.enabled = false;
    }

    // ✅ HELPER DO WYWOŁYWANIA CZĄSTECZEK
    private void SpawnParticles(ParticleSystem ps, Vector3 pos)
    {
        if (ps == null) return;

        // Upewnij się, że system jest w dobryj pozycji (jeśli nie jest dzieckiem demona)
        // Jeśli ParticleSystem jest dzieckiem tego GameObject, pozycja jest względna, więc musimy uważać.
        // Najbezpieczniej jest mieć VFX jako dziecko, ale wywoływać Play() lokalnie.

        // Resetujemy pozycję jeśli VFX nie jest dzieckiem (rzadki przypadek przy tym setupie)
        if (ps.transform.parent != transform)
        {
            ps.transform.position = pos;
        }

        // Ważne dla retro efektu: wyłącz emitowanie ciągłe, odpal raz
        var emission = ps.emission;
        emission.enabled = true;

        ps.Play();

        // Opcjonalnie: wymuszenie emisji wszystkich cząstek naraz jeśli to burst
        // ps.Emit(10); // Można użyć zamiast Play(), jeśli wolisz kontrolować ilość ręcznie
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