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
        // ✅ NULL-CHECK przy starcie
        if (pausePanel != null)
            pausePanel.SetActive(false);
        else
            Debug.LogWarning("[PauseMenu] pausePanel is missing!");

        // ✅ NULL-CHECKI dla komponentów gracza
        if (playerCam != null && sensitivitySlider != null)
        {
            sensitivitySlider.value = playerCam.sensX;
            UpdateSensitivityUI(sensitivitySlider.value);
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    void Update()
    {
        // ✅ KLUCZOWE: BLOKUJ PAUZĘ JEŚLI UI JEST ZNISZCZONE
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
        // ✅ NULL-CHECK: pausePanel może być zniszczony po przejściu sceny
        if (pausePanel == null)
        {
            Debug.LogWarning("[PauseMenu] pausePanel destroyed - cannot pause");
            return;
        }

        isPaused = true;
        pausePanel.SetActive(true);

        // ✅ NULL-CHECKI dla komponentów gracza
        if (playerController != null)
            playerController.enabled = false;

        if (playerCam != null)
            playerCam.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        // ✅ NULL-CHECKI przed przywróceniem
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;

        if (playerCam != null)
            playerCam.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Quit()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    void OnSensitivityChanged(float value)
    {
        if (playerCam != null)
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
        // ✅ NULL-CHECKI
        if (consolePanel == null || playerController == null || playerCam == null)
        {
            Debug.LogWarning("[PauseMenu] Console UI missing - cannot open console");
            return;
        }

        isPaused = true;
        consolePanel.SetActive(true);

        playerController.enabled = false;
        playerCam.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        var inputField = consolePanel.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
            inputField.ActivateInputField();
    }

    public void CloseConsole()
    {
        // ✅ NULL-CHECKI
        if (consolePanel == null || playerController == null || playerCam == null)
            return;

        isPaused = false;
        consolePanel.SetActive(false);

        playerController.enabled = true;
        playerCam.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }
}