using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMODUnity;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneName = "FlatScene";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

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


    private void Start()
    {
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
        menuAmbientInstance = RuntimeManager.CreateInstance(menuAmbientEvent);
        menuAmbientInstance.start();

    }

    // --- SCENE ---
    public void PlayGame()
    {
        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            menuAmbientInstance.release();
        }

        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadSceneAsync(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // --- UI PANELS ---
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // --- AUDIO BUTTONS ---
    public void VolumePlus()
    {
        if (isMuted) return;
        currentVolume = Mathf.Clamp01(currentVolume + volumeStep);
        ApplyVolume();
    }

    public void VolumeMinus()
    {
        if (isMuted) return;
        currentVolume = Mathf.Clamp01(currentVolume - volumeStep);
        ApplyVolume();
    }

    public void VolumeMute()
    {
        isMuted = !isMuted;
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        float volume = isMuted ? 0f : currentVolume;
        // Dla FMOD, mo¿esz tutaj ustawiæ master volume globalnie:
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", volume);
        Debug.Log("Volume set to: " + volume);
    }

    // --- MOUSE SENSITIVITY ---
    private void OnSensitivityChanged(float value)
    {
        mouseSensitivity = value;
    }
    private void OnDestroy()
    {
        // Safety: release ambient if menu destroyed
        if (menuAmbientInstance.isValid())
        {
            menuAmbientInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            menuAmbientInstance.release();
        }
    }
}
