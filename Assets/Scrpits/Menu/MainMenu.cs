using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "FlatScene";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject settingsOverlay;
    public GameObject transitionOverlay; // ← DODAJ TO

    [Header("Transition Settings")]
    public float transitionDuration = 0.8f;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float volumeStep = 0.1f;
    private float currentVolume = 1f;
    private bool isMuted = false;

    [Header("Mouse Settings")]
    public Slider mouseSensitivitySlider;
    [Range(0.1f, 10f)]
    public float defaultSensitivity = 2f;
    public float mouseSensitivity = 2f;

    [Header("Menu Ambient")]
    public EventReference menuAmbientEvent;
    private EventInstance menuAmbientInstance;

    // === 🔒 ENDING LOCK: "U cant escape" (TMP VERSION) ===
    [Header("Ending Lock Settings")]
    [Tooltip("Reference do MainMenuCameraController (do sprawdzenia aktywnej kamery)")]
    public MainMenuCameraController cameraController;

    [Tooltip("Tekst 'U cant escape' - TextMeshPro (domyślnie wyłączony)")]
    public TMP_Text cantEscapeText; // ✅ TMP_Text zamiast Text

    [Tooltip("Kamera do shake'owania (jeśli inna niż Main Camera)")]
    public Camera shakeCamera;

    [Tooltip("Siła shake'owania (0.02-0.1)")]
    public float shakeIntensity = 0.05f;

    [Tooltip("Czas trwania shake'owania (sekundy)")]
    public float shakeDuration = 2f;

    [Tooltip("Czy zablokować quit po Ending 1 (Prison)")]
    public bool lockQuitOnPrison = true;

    [Tooltip("Czy zablokować quit po Ending 2 (Hospital)")]
    public bool lockQuitOnHospital = true;

    [Tooltip("Kolor tekstu 'U cant escape'")]
    public Color cantEscapeColor = new Color(1f, 0.2f, 0.2f, 1f); // Czerwony

    private Vector3 originalCamPosition;
    private Coroutine shakeCoroutine;
    private Coroutine overlayFadeCoroutine;
    private Coroutine transitionCoroutine;

    private void Start()
    {
        if (Time.timeScale <= 0f)
        {
            Debug.LogWarning("[MainMenu] ⚠️ timeScale was paused! Resetting to 1f");
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.minValue = 0.1f;
            mouseSensitivitySlider.maxValue = 10f;
            mouseSensitivitySlider.value = defaultSensitivity;
            mouseSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            mouseSensitivity = defaultSensitivity;
        }

        ApplyVolume();
        ShowMainMenu();

        if (!menuAmbientEvent.IsNull)
        {
            menuAmbientInstance = RuntimeManager.CreateInstance(menuAmbientEvent);
            menuAmbientInstance.start();
        }

        // ✅ Ukryj "U cant escape" na start
        if (cantEscapeText != null)
            cantEscapeText.gameObject.SetActive(false);
    }

    // --- SCENE ---
    // --- SCENE TRANSITION ---

    public void PlayGame()
    {
        // 🔒 Zablokuj input, żeby nie kliknąć dwa razy
        enabled = false;

        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            menuAmbientInstance.release();
        }

        // 🔒 Ukryj cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 🎬 Start transition: pokaż overlay i fade-out do czerni
        if (transitionOverlay != null)
        {
            transitionOverlay.SetActive(true);
            var img = transitionOverlay.GetComponent<Image>();
            if (img != null)
            {
                // Najpierw ustaw na przezroczysty, potem fade do czarnego
                img.color = new Color(0f, 0f, 0f, 0f);
                transitionCoroutine = StartCoroutine(FadeTransition(img, 0f, 1f, transitionDuration, OnTransitionComplete));
            }
            else
            {
                // Fallback: jeśli nie ma Image, po prostu załaduj scenę
                OnTransitionComplete();
            }
        }
        else
        {
            // Fallback: jeśli nie przypisano overlaya
            OnTransitionComplete();
        }
    }

    // 🎞️ Korutina fade transition z callbackiem
    private IEnumerator FadeTransition(Image img, float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        Color baseColor = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, currentAlpha);
            yield return null;
        }

        // Wymuś dokładny końcowy kolor
        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, endAlpha);

        // 🔥 Wywołaj callback po zakończeniu
        onComplete?.Invoke();
    }

    // ✅ Callback: gdy transition się skończy → załaduj scenę
    private void OnTransitionComplete()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Używamy LoadScene, nie Async, bo transition już ukrył ładowanie
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// 🔒 QuitGame z blokadą po "złych" zakończeniach
    /// </summary>
    public void QuitGame()
    {
        if (IsQuitLocked())
        {
            Debug.Log("[MainMenu] 🔒 Quit blocked - playing 'U cant escape' sequence");
            StartCoroutine(CantEscapeSequence());
            return;
        }

        Debug.Log("[MainMenu] 👋 Quitting game (default ending)");
        Application.Quit();
    }

    private bool IsQuitLocked()
    {
        if (cameraController == null) return false;

        if (lockQuitOnPrison && cameraController.prisonCameraGroup != null &&
            cameraController.prisonCameraGroup.activeSelf)
            return true;

        if (lockQuitOnHospital && cameraController.hospitalCameraGroup != null &&
            cameraController.hospitalCameraGroup.activeSelf)
            return true;

        return false;
    }

    /// <summary>
    /// Sekwencja: miganie tekstu TMP + shake kamery
    /// </summary>
    /// <summary>
    /// Sekwencja: miganie tekstu TMP + shake AKTYWNEJ kamery
    /// </summary>
    private IEnumerator CantEscapeSequence()
    {
        if (cantEscapeText != null)
        {
            cantEscapeText.gameObject.SetActive(true);
            cantEscapeText.text = "YOU CAN'T ESCAPE";
            cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, 0f);
        }

        // 📷 ZNAJDŹ AKTYWNĄ KAMERĘ DYNAMICZNIE:
        Camera camToShake = FindActiveMainCamera();

        if (camToShake != null)
        {
            originalCamPosition = camToShake.transform.localPosition;
            shakeCoroutine = StartCoroutine(ShakeCamera(camToShake));
        }

        // 👁️ Miganie tekstu TMP
        float elapsed = 0f;
        float flickerSpeed = 0.1f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            if (cantEscapeText != null && Time.time % flickerSpeed < flickerSpeed * 0.5f)
            {
                float alpha = Random.Range(0.7f, 1f);
                cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, alpha);
            }
            else if (cantEscapeText != null)
            {
                float alpha = Random.Range(0.2f, 0.5f);
                cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, alpha);
            }

            yield return null;
        }

        // 🛑 Zatrzymaj shake i ukryj tekst
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        if (camToShake != null) camToShake.transform.localPosition = originalCamPosition;
        if (cantEscapeText != null)
        {
            cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, 0f);
            cantEscapeText.gameObject.SetActive(false);
        }

        Debug.Log("[MainMenu] 🔓 'U cant escape' sequence finished");
    }

    /// <summary>
    /// ✅ Znajduje aktywną kamerę z tagiem "MainCamera"
    /// Działa niezależnie od tego, która grupa kamer jest włączona.
    /// </summary>
    private Camera FindActiveMainCamera()
    {
        // 1️⃣ Najpierw spróbuj Camera.main (najszybsze, automatyczne)
        Camera main = Camera.main;
        if (main != null && main.gameObject.activeInHierarchy)
        {
            Debug.Log($"[MainMenu] 📷 Found active camera via Camera.main: {main.name}");
            return main;
        }

        // 2️⃣ Fallback: znajdź pierwszą aktywną kamerę z tagiem "MainCamera"
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("MainCamera") && cam.gameObject.activeInHierarchy)
            {
                Debug.Log($"[MainMenu] 📷 Found active camera via fallback: {cam.name}");
                return cam;
            }
        }

        // 3️⃣ Ostateczny fallback: pierwsza aktywna kamera w scenie
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy)
            {
                Debug.Log($"[MainMenu] 📷 Found active camera via last resort: {cam.name}");
                return cam;
            }
        }

        Debug.LogWarning("[MainMenu] ⚠️ No active camera found for shake!");
        return null;
    }
    private IEnumerator ShakeCamera(Camera cam)
    {
        if (cam == null) yield break;

        Transform camTransform = cam.transform;
        Vector3 originalLocalPos = camTransform.localPosition;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Random.Range(-1f, 1f) * shakeIntensity;
            float offsetY = Random.Range(-1f, 1f) * shakeIntensity;
            camTransform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
        camTransform.localPosition = originalLocalPos;
    }

    // --- UI PANELS ---

    public void ShowSettings()
    {
        // 🔴 ZATRZYMAJ + WYCZYŚĆ referencję do korutiny
        if (overlayFadeCoroutine != null)
        {
            StopCoroutine(overlayFadeCoroutine);
            overlayFadeCoroutine = null; // ← TO JEST KLUCZOWE!
        }

        // 🔄 Hard reset overlaya
        if (settingsOverlay != null)
        {
            var img = settingsOverlay.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
                settingsOverlay.SetActive(true);
            }
        }

        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        // ✅ Start fade-in
        if (settingsOverlay != null)
        {
            var img = settingsOverlay.GetComponent<Image>();
            if (img != null)
            {
                overlayFadeCoroutine = StartCoroutine(FadeOverlay(img, 0f, 0.5f, 0.2f));
            }
        }

        Debug.Log("[UI] ShowSettings: panels switched, fade-in started");
    }

    public void ShowMainMenu()
    {
        // 🔴 ZATRZYMAJ + WYCZYŚĆ referencję do korutiny
        if (overlayFadeCoroutine != null)
        {
            StopCoroutine(overlayFadeCoroutine);
            overlayFadeCoroutine = null; // ← TO JEST KLUCZOWE!
        }

        // ✅ Natychmiastowy reset overlaya (bez korutiny, dla pewności)
        if (settingsOverlay != null)
        {
            var img = settingsOverlay.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }
            settingsOverlay.SetActive(false);
        }

        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        Debug.Log("[UI] ShowMainMenu: panels switched, overlay hidden");
    }

    // 🎞️ Helper: smooth fade dla overlaya
    private IEnumerator FadeOverlay(Image img, float startAlpha, float endAlpha, float duration)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        Color baseColor = img.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t);
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, currentAlpha);
            yield return null;
        }

        // ✅ Wymuś końcowy kolor + wyczyść referencję
        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, endAlpha);
        overlayFadeCoroutine = null; // ← Bezpieczne zakończenie
    }
    // --- AUDIO ---
    public void VolumePlus() { if (isMuted) return; currentVolume = Mathf.Clamp01(currentVolume + volumeStep); ApplyVolume(); }
    public void VolumeMinus() { if (isMuted) return; currentVolume = Mathf.Clamp01(currentVolume - volumeStep); ApplyVolume(); }
    public void VolumeMute() { isMuted = !isMuted; ApplyVolume(); }

    private void ApplyVolume()
    {
        float volume = isMuted ? 0f : currentVolume;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", volume);
    }

    // --- MOUSE ---
    private void OnSensitivityChanged(float value) { mouseSensitivity = value; }

    private void OnDestroy()
    {
        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            menuAmbientInstance.release();
        }
    }
}