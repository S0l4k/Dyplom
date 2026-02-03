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

    [Header("References")]
    public PlayerController playerController;
    public PlayerCam playerCam;
    public ComputerInteract computerInteract;

    [Header("Console")]
    public GameObject consolePanel;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);

        
        sensitivitySlider.value = playerCam.sensX;
        UpdateSensitivityUI(sensitivitySlider.value);

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !computerInteract.isUsingComputer)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);

        playerController.enabled = false;
        playerCam.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f; 
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        playerController.enabled = true;
        playerCam.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }
    
    public void Quit()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    void OnSensitivityChanged(float value)
    {
        playerCam.SetSensitivity(value);
        UpdateSensitivityUI(value);
    }

    void UpdateSensitivityUI(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = value.ToString("0");
    }

    public void OpenConsole()
    {
        isPaused = true;
        consolePanel.SetActive(true);

        playerController.enabled = false;
        playerCam.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        consolePanel.GetComponentInChildren<TMP_InputField>().ActivateInputField();
    }

    public void CloseConsole()
    {
        isPaused = false;
        consolePanel.SetActive(false);

        playerController.enabled = true;
        playerCam.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

}
