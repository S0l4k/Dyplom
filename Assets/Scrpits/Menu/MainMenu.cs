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
    public GameObject transitionOverlay;

    [Header("Transition Settings")]
    public float transitionDuration = 0.8f;

    [Header("Audio UI - SLIDERY")]
    public Slider volumeSlider;
    public Toggle muteToggle;
    public TMP_Text volumeValueText;

    [Header("Mouse Settings")]
    public Slider mouseSensitivitySlider;
    [Range(0.1f, 10f)]
    public float defaultSensitivity = 2f;
    public float mouseSensitivity = 2f;

    [Header("References")]
    public SettingsManager settingsManager;

    [Header("Menu Ambient")]
    public EventReference menuAmbientEvent;
    private EventInstance menuAmbientInstance;

    [Header("Ending Lock")]
    public MainMenuCameraController cameraController;
    public TMP_Text cantEscapeText;
    public Camera shakeCamera;
    public float shakeIntensity = 0.05f;
    public float shakeDuration = 2f;
    public bool lockQuitOnPrison = true;
    public bool lockQuitOnHospital = true;
    public Color cantEscapeColor = new Color(1f, 0.2f, 0.2f, 1f);

    private Vector3 originalCamPosition;
    private Coroutine shakeCoroutine;
    private Coroutine overlayFadeCoroutine;
    private Coroutine transitionCoroutine;

    private void Start()
    {
        if (Time.timeScale <= 0f)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }

        SetupAudioUI();

        SetupSensitivityUI();

        ApplyVolumeToFMOD();

        ShowMainMenu();

        if (!menuAmbientEvent.IsNull)
        {
            menuAmbientInstance = RuntimeManager.CreateInstance(menuAmbientEvent);

            float vol = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bool muted = (vol <= 0.01f);
            menuAmbientInstance.setVolume(muted ? 0f : vol);

            menuAmbientInstance.start();
        }

        if (cantEscapeText != null)
        {
            cantEscapeText.gameObject.SetActive(false);
        }
    }
    // ════════════════════════════════════════════════════
    // ─── AUDIO UI SETUP ─────────────────────────────────
    // ════════════════════════════════════════════════════

    private void SetupAudioUI()
    {
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bool savedMute = (savedVol <= 0.01f);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedMute ? 0f : savedVol;
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = savedMute;
            muteToggle.onValueChanged.RemoveAllListeners();
            muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        }

        UpdateVolumeText();
        ApplyVolumeToFMOD(); 
    }

    private void OnVolumeSliderChanged(float value)
    {
        bool isMuted = (value <= 0.01f);

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();

        float finalVol = isMuted ? 0f : value;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", finalVol);

        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.setVolume(finalVol);
        }

        if (muteToggle != null)
            muteToggle.isOn = isMuted;

        UpdateVolumeText();
    }

    private void OnMuteToggleChanged(bool isMuted)
    {
        float lastVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float newVol = isMuted ? 0f : (lastVol > 0.01f ? lastVol : 1f);

        PlayerPrefs.SetFloat("MasterVolume", newVol);
        PlayerPrefs.Save();

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", newVol);

        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.setVolume(newVol);
        }

        if (volumeSlider != null)
            volumeSlider.value = newVol;

        UpdateVolumeText();
    }

    private void UpdateVolumeText()
    {
        if (volumeValueText == null) return;

        float vol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bool muted = (muteToggle != null && muteToggle.isOn) || vol <= 0.01f;

        volumeValueText.text = muted ? "MUTED" : $"{Mathf.RoundToInt(vol * 100)}%";
    }

    private void ApplyVolumeToFMOD()
    {
        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bool isMuted = (PlayerPrefs.GetInt("MasterVolume", 1) <= 0.01f);
        float finalVol = isMuted ? 0f : savedVol;

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", finalVol);

        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.setVolume(finalVol);
        }
    }

    // ════════════════════════════════════════════════════
    // ─── SENSITIVITY UI SETUP ───────────────────────────
    // ════════════════════════════════════════════════════

    private void SetupSensitivityUI()
    {
        if (mouseSensitivitySlider == null) return;

        float menuSens = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);

        mouseSensitivitySlider.minValue = 0.1f;
        mouseSensitivitySlider.maxValue = 2000f;
        mouseSensitivitySlider.value = menuSens;  
        mouseSensitivity = menuSens;

        mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
        mouseSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        mouseSensitivity = value;

        if (settingsManager != null)
        {
            settingsManager.SaveSensitivity(value);
        }
    }

    // ════════════════════════════════════════════════════
    // ─── SCENE TRANSITION ───────────────────────────────
    // ════════════════════════════════════════════════════

    public void PlayGame()
    {
        ResetGameSession();
        enabled = false;

        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            menuAmbientInstance.release();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (transitionOverlay != null)
        {
            transitionOverlay.SetActive(true);
            Image img = transitionOverlay.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(0f, 0f, 0f, 0f);
                transitionCoroutine = StartCoroutine(FadeTransition(img, 0f, 1f, transitionDuration, OnTransitionComplete));
            }
            else
            {
                OnTransitionComplete();
            }
        }
        else
        {
            OnTransitionComplete();
        }
        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
            menuAmbientInstance.release();
            menuAmbientInstance = default;
        }
    }
    private void ResetGameSession()
    {

        GameState.ResetAll();

        if (QuestManager.Instance != null)
            QuestManager.Instance.ResetAllQuests();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

    }
    private IEnumerator FadeTransition(Image img, float startAlpha, float endAlpha, float duration, System.Action onComplete)
    {
        if (img == null)
        {
            yield break;
        }

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

        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, endAlpha);

        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }

    private void OnTransitionComplete()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }

    // ════════════════════════════════════════════════════
    // ─── QUIT + ENDING LOCK ─────────────────────────────
    // ════════════════════════════════════════════════════

    public void QuitGame()
    {
        if (IsQuitLocked())
        {
            StartCoroutine(CantEscapeSequence());
            return;
        }
        Application.Quit();
    }

    private bool IsQuitLocked()
    {
        if (cameraController == null)
        {
            return false;
        }

        if (lockQuitOnPrison && cameraController.prisonCameraGroup != null && cameraController.prisonCameraGroup.activeSelf)
        {
            return true;
        }

        if (lockQuitOnHospital && cameraController.hospitalCameraGroup != null && cameraController.hospitalCameraGroup.activeSelf)
        {
            return true;
        }

        return false;
    }

    private IEnumerator CantEscapeSequence()
    {
        if (cantEscapeText != null)
        {
            cantEscapeText.gameObject.SetActive(true);
            cantEscapeText.text = "YOU CAN'T ESCAPE";
            cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, 0f);
        }

        Camera camToShake = FindActiveMainCamera();
        if (camToShake != null)
        {
            originalCamPosition = camToShake.transform.localPosition;
            shakeCoroutine = StartCoroutine(ShakeCamera(camToShake));
        }

        float elapsed = 0f;
        float flickerSpeed = 0.1f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            if (cantEscapeText != null)
            {
                float alpha;
                if (Time.time % flickerSpeed < flickerSpeed * 0.5f)
                {
                    alpha = Random.Range(0.7f, 1f);
                }
                else
                {
                    alpha = Random.Range(0.2f, 0.5f);
                }
                cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, alpha);
            }

            yield return null;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        if (camToShake != null)
        {
            camToShake.transform.localPosition = originalCamPosition;
        }

        if (cantEscapeText != null)
        {
            cantEscapeText.color = new Color(cantEscapeColor.r, cantEscapeColor.g, cantEscapeColor.b, 0f);
            cantEscapeText.gameObject.SetActive(false);
        }

    }

    private Camera FindActiveMainCamera()
    {
        Camera main = Camera.main;
        if (main != null && main.gameObject.activeInHierarchy)
        {
            return main;
        }

        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.CompareTag("MainCamera") && cam.gameObject.activeInHierarchy)
            {
                return cam;
            }
        }

        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.activeInHierarchy)
            {
                return cam;
            }
        }

        return null;
    }

    private IEnumerator ShakeCamera(Camera cam)
    {
        if (cam == null)
        {
            yield break;
        }

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

    // ════════════════════════════════════════════════════
    // ─── UI PANELS ──────────────────────────────────────
    // ════════════════════════════════════════════════════

    public void ShowSettings()
    {
        if (overlayFadeCoroutine != null)
        {
            StopCoroutine(overlayFadeCoroutine);
            overlayFadeCoroutine = null;
        }

        if (settingsOverlay != null)
        {
            Image img = settingsOverlay.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }
            settingsOverlay.SetActive(true);
        }

        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        if (settingsOverlay != null)
        {
            Image img = settingsOverlay.GetComponent<Image>();
            if (img != null)
            {
                overlayFadeCoroutine = StartCoroutine(FadeOverlay(img, 0f, 0.5f, 0.2f));
            }
        }

    }

    public void ShowMainMenu()
    {
        if (overlayFadeCoroutine != null)
        {
            StopCoroutine(overlayFadeCoroutine);
            overlayFadeCoroutine = null;
        }

        if (settingsOverlay != null)
        {
            Image img = settingsOverlay.GetComponent<Image>();
            if (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }
            settingsOverlay.SetActive(false);
        }

        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private IEnumerator FadeOverlay(Image img, float startAlpha, float endAlpha, float duration)
    {
        if (img == null)
        {
            yield break;
        }

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

        img.color = new Color(baseColor.r, baseColor.g, baseColor.b, endAlpha);
        overlayFadeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            menuAmbientInstance.release();
            menuAmbientInstance = default; 
        }
    }
}