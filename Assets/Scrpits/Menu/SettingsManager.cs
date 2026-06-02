using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    private const string KEY_SENSITIVITY = "Settings_MouseSensitivity";

    [Header("Defaults")]
    [Range(0f, 2000f)]  
    public float defaultSensitivity = 2f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public float GetSavedSensitivity() => PlayerPrefs.GetFloat(KEY_SENSITIVITY, defaultSensitivity);

    public void SaveSensitivity(float sensitivity)
    {
        sensitivity = Mathf.Clamp(sensitivity, 0f, 1200f);
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, sensitivity);
        PlayerPrefs.Save();
    }
}