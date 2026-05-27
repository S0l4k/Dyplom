using UnityEngine;
using TMPro;
using System.Collections;
using FMODUnity;

public class SofaInteract : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform sofaViewPoint;
    public GameObject interactText;
    public TMP_Text thoughtText;
    public Outline outline;

    [Header("Settings")]
    public float useRange = 3f;
    public float moveSpeed = 2.5f;
    public float typeSpeed = 0.07f;

    private bool canUse = false;
    private PlayerController playerController;
    private PlayerCam playerCam;
    public ItemCheck _itemCheck;

    // === 📷 Zapisane pozycje kamery (jak w ComputerInteract) ===
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;

    void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false);

        playerController = FindObjectOfType<PlayerController>();
        playerCam = FindObjectOfType<PlayerCam>();
        if (outline != null) outline.enabled = false;
    }

    void Update()
    {
        if (!GameState.InteractionsLocked || GameState.SofaSequenceActive)
        {
            canUse = false;
            if (interactText != null) interactText.gameObject.SetActive(false);
            return;
        }

        CheckForSofa();

        if (canUse && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(UseSofa());
        }
    }

    void CheckForSofa()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, useRange) && hit.collider.gameObject == gameObject)
        {
            interactText.gameObject.SetActive(true);
            canUse = true;

            // ✅ NOWE: Włącz outline gdy gracz patrzy na sofę
            if (outline != null) outline.enabled = true;

            return;
        }

        interactText.gameObject.SetActive(false);
        canUse = false;

        // ✅ NOWE: Wyłącz outline gdy gracz nie patrzy na sofę
        if (outline != null) outline.enabled = false;
    }
    IEnumerator UseSofa()
    {
        // === BLOKADA ===
        _itemCheck.enabled = false;
        GameState.SofaSequenceActive = true;
        canUse = false;
        if (interactText != null) interactText.gameObject.SetActive(false);

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;

        // === 📷 ZAPISZ aktualną pozycję kamery PRZED cutscenką (jak w ComputerInteract) ===
        originalCamPosition = playerCamera.position;
        originalCamRotation = playerCamera.rotation;
        outline.enabled = false;
        // === KAMERA: płynne przejście do pozycji na kanapie ===
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * moveSpeed;
            playerCamera.position = Vector3.Lerp(startPos, sofaViewPoint.position, elapsed);
            playerCamera.rotation = Quaternion.Slerp(startRot, sofaViewPoint.rotation, elapsed);
            yield return null;
        }

        yield return new WaitForSeconds(2f); // Krótka chwila ciszy na kanapie

        // === 🎬 PRZEKAZANIE KONTROLI do GameNarrativeManager ===
        if (GameNarrativeManager.Instance != null)
        {
            Debug.Log("[SofaInteract] 🎬 Handing off to GameNarrativeManager for extended ending");

            // ✅ PRZEKAŻ zapisane pozycje kamery!
            GameNarrativeManager.Instance.StartSecondEndingFinalSequence(originalCamPosition, originalCamRotation);
        }
        else
        {
            Debug.LogError("[SofaInteract] ❌ GameNarrativeManager NULL! Fallback exit...");
            yield return StartCoroutine(FallbackExit());
        }
    }

    private IEnumerator FallbackExit()
    {
        yield return new WaitForSeconds(1f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}