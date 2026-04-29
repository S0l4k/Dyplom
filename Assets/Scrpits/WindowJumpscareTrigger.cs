using UnityEngine;
using System.Collections;

public class WindowJumpscareTrigger : MonoBehaviour
{
    [Header("References")]
    public GameNarrativeManager narrativeManager;
    public Transform demonSpawnPoint;  // Gdzie ma się pojawić demon (ZA graczem)

    [Header("Jumpscare Settings")]
    [Tooltip("Offset w górę, żeby kamera patrzyła na twarz demona, a nie stopy (np. 1.5)")]
    public Vector3 demonLookAtOffset = new Vector3(0f, 1.5f, 0f);

    [Tooltip("Czy używać offsetu przy patrzeniu na demona")]
    public bool useLookAtOffset = true;

    [Header("Debug")]
    public bool showDebugGizmos = true;

    // === ✅ FLAGA: czy trigger już się odpalił ===
    [HideInInspector]
    public bool hasTriggered = false;

    private PlayerController playerController;
    private PlayerCam playerCam;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        gameObject.SetActive(false); // Domyślnie wyłączone
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (narrativeManager == null) return;

        hasTriggered = true;
        Debug.Log("[WindowJumpscareTrigger] ✅ Player entered trigger - starting jumpscare");
        StartCoroutine(TriggerJumpscare());
    }

    public void ActivateTrigger() => gameObject.SetActive(true);

    private IEnumerator TriggerJumpscare()
    {
        // 1. 🔒 Zablokuj gracza
        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;

        // 2. 👹 Pokaż demona ZA graczem
        if (narrativeManager.demonFinalJumpscareObject != null && demonSpawnPoint != null)
        {
            narrativeManager.demonFinalJumpscareObject.transform.position = demonSpawnPoint.position;
            narrativeManager.demonFinalJumpscareObject.transform.rotation = demonSpawnPoint.rotation;
            narrativeManager.demonFinalJumpscareObject.SetActive(true);
            Debug.Log("[WindowJumpscareTrigger] 👹 Demon ACTIVATED at: " + demonSpawnPoint.position);
        }

        // 3. 🔄 Kamera płynnie obraca się NA DEMONA (z offsetem na twarz!)
        Transform playerCamera = Camera.main?.transform;
        if (narrativeManager.demonFinalJumpscareObject != null && playerCamera != null)
        {
            // ✅ Oblicz pozycję celu Z OFFSETEM (żeby patrzeć na twarz, nie stopy)
            Transform demonTransform = narrativeManager.demonFinalJumpscareObject.transform;
            Vector3 lookAtTarget = demonTransform.position;

            if (useLookAtOffset)
            {
                // Dodaj offset w lokalnej przestrzeni demona (Y = wyżej)
                lookAtTarget = demonTransform.TransformPoint(demonLookAtOffset);
                Debug.Log($"[DEBUG] 🎯 Looking at demon with offset: {lookAtTarget} (head level)");
            }

            // SmoothLookAt na punkt z offsetem
            yield return StartCoroutine(narrativeManager.SmoothLookAt(
                playerCamera,
                CreateTempLookTarget(lookAtTarget),  // ✅ Tymczasowy target z offsetem
                narrativeManager.cameraTurnToDemonDuration
            ));
        }

        // 4. 🔊 Dźwięk reveal'u demona
        if (!narrativeManager.demonJumpscareRevealSFX.IsNull && playerCamera != null)
        {
            AudioManager.Instance.PlaySFX(
                narrativeManager.demonJumpscareRevealSFX,
                playerCamera.position
            );
        }

        // 5. ⏱️ 1.5 sekundy na zarejestrowanie demona
        yield return new WaitForSeconds(1.5f);

        // 6. 🌑 Fade out + dźwięk rozbitego okna
        if (narrativeManager.screenFader != null)
        {
            yield return StartCoroutine(narrativeManager.screenFader.FadeOut(narrativeManager.jumpscareFadeDuration));
        }

        if (!narrativeManager.windowBreakAndFallSFX.IsNull && playerCamera != null)
        {
            AudioManager.Instance.PlaySFX(narrativeManager.windowBreakAndFallSFX, playerCamera.position);
        }

        // 7. ⏱️ Czarny ekran z dźwiękami
        yield return new WaitForSeconds(narrativeManager.blackScreenDuration);

        // 8. 🔚 Exit gry
        Debug.Log("[Narrative] 🔚 Second ending complete - exiting game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// Tworzy tymczasowy obiekt jako cel dla SmoothLookAt (auto-usuwa się po 2s).
    /// </summary>
    private Transform CreateTempLookTarget(Vector3 position)
    {
        GameObject temp = new GameObject("TempJumpscareLookTarget");
        temp.transform.position = position;
        temp.transform.rotation = Quaternion.identity;
        Destroy(temp, 2f); // 🔥 Auto-cleanup
        return temp.transform;
    }

    // === 🎨 Debug gizmos w Editorze ===
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;

        if (narrativeManager?.demonFinalJumpscareObject != null && useLookAtOffset)
        {
            // Pokaż gdzie kamera będzie patrzeć (zielona kula = twarz demona)
            Vector3 headPos = narrativeManager.demonFinalJumpscareObject.transform.TransformPoint(demonLookAtOffset);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(headPos, 0.15f);

            // Linia od demona do "głowy"
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(narrativeManager.demonFinalJumpscareObject.transform.position, headPos);
        }
    }
#endif
}