using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public Slider sensitivitySlider;         
    public TMP_Text sensitivityValueText;
    public GameObject crosshair;
    public GameObject main;

    [Header("Audio Controls")]
    public Slider volumeSlider;               
    public TMP_Text volumeValueText;        

    [Header("References")]
    public PlayerController playerController;
    public PlayerCam playerCam;
    public ComputerInteract computerInteract;
    public SettingsManager settingsManager;

    [Header("Console")]
    public GameObject consolePanel;

    private bool isPaused = false;
    private float lastNonZeroVolume = 1f;    
    void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        SetupSensitivityControls();
        SetupVolumeControls();
    }

    void Update()
    {
        if (pausePanel == null || playerController == null || playerCam == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) && computerInteract != null && !computerInteract.isUsingComputer)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        if (pausePanel == null)
        {
            return;
        }

        isPaused = true;
        pausePanel.SetActive(true);
        crosshair.SetActive(false);

        if (playerController != null) playerController.enabled = false;
        if (playerCam != null) playerCam.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        UpdateVolumeUI();
    }

    public void Resume()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        main.SetActive(true);

        if (playerController != null) playerController.enabled = true;
        if (playerCam != null) playerCam.enabled = true;
        if (GameState.IsInspecting)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        crosshair.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        StopAllCoroutines();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Application.Quit();
    }

    // ════════════════════════════════════════════════════
    // ─── SENSITIVITY (0-1200) ───────────────────────────
    // ════════════════════════════════════════════════════

    private void SetupSensitivityControls()
    {
        if (playerCam == null || sensitivitySlider == null)
        {
            return;
        }

        float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", playerCam.sensX);

        float clampedSens = Mathf.Clamp(savedSens, sensitivitySlider.minValue, sensitivitySlider.maxValue);
        sensitivitySlider.value = clampedSens;

        playerCam.SetSensitivity(clampedSens);
        UpdateSensitivityUI(clampedSens);

        sensitivitySlider.onValueChanged.RemoveAllListeners();
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        float clampedValue = Mathf.Clamp(value, sensitivitySlider.minValue, sensitivitySlider.maxValue);

        if (playerCam != null)
        {
            playerCam.SetSensitivity(clampedValue);
        }

        UpdateSensitivityUI(clampedValue);
        PlayerPrefs.SetFloat("MouseSensitivity", clampedValue);
        PlayerPrefs.Save();
    }

    private void UpdateSensitivityUI(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("0"); 
    }

    // ════════════════════════════════════════════════════
    // ─── VOLUME (Slider only, no toggle) ────────────────
    // ════════════════════════════════════════════════════

    private void SetupVolumeControls()
    {
        if (AudioManager.Instance == null) return;

        float savedVolume = AudioManager.Instance.GetMasterVolume();
        lastNonZeroVolume = savedVolume > 0.01f ? savedVolume : 1f;

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        UpdateVolumeUI();
    }

    private void OnVolumeSliderChanged(float value)
    {
        if (AudioManager.Instance == null) return;

        if (value <= 0.01f)
        {
            AudioManager.Instance.SetMuteStateDirect(true);
        }
        else
        {
            if (AudioManager.Instance.IsMuted())
            {
                AudioManager.Instance.SetMuteStateDirect(false);
            }
            AudioManager.Instance.SetMasterVolumeDirect(value);
            lastNonZeroVolume = value;
        }

        UpdateVolumeUI();

    }

    private void UpdateVolumeUI()
    {
        if (volumeValueText == null || AudioManager.Instance == null) return;

        float vol = AudioManager.Instance.GetMasterVolume();
        bool isMuted = AudioManager.Instance.IsMuted() || vol <= 0.01f;

        volumeValueText.text = isMuted ? "MUTED" : $"{Mathf.RoundToInt(vol * 100)}%";
    }

    public void OpenConsole()
    {
        if (consolePanel == null || playerController == null || playerCam == null) return;

        isPaused = true;
        consolePanel.SetActive(true);
        playerController.enabled = false;
        playerCam.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        var inputField = consolePanel.GetComponentInChildren<TMP_InputField>();
        if (inputField != null) inputField.ActivateInputField();
    }

    public void CloseConsole()
    {
        if (consolePanel == null || playerController == null || playerCam == null) return;

        isPaused = false;
        consolePanel.SetActive(false);
        playerController.enabled = true;
        playerCam.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    void OnApplicationQuit()
    {
        if (playerCam != null)
        {
            PlayerPrefs.SetFloat("MouseSensitivity", playerCam.sensX);
            PlayerPrefs.Save();
        }
    }
}