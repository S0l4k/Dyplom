using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;

    [Header("References")]
    public PlayerController playerController;
    public PlayerCam playerCam;

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
        if (Input.GetKeyDown(KeyCode.Escape))
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

        Time.timeScale = 0f; // opcjonalne – jeœli NIE chcesz pauzy, usuñ
    }

    void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        playerController.enabled = true;
        playerCam.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
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
}
