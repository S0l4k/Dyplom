// SettingsManager.cs - AKTUALIZACJA: zakres 0-1200
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    private const string KEY_SENSITIVITY = "Settings_MouseSensitivity";

    [Header("Defaults")]
    [Range(0f, 2000f)]  // ← ZMIANA: 10f → 1200f
    public float defaultSensitivity = 2f;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public float GetSavedSensitivity() => PlayerPrefs.GetFloat(KEY_SENSITIVITY, defaultSensitivity);

    public void SaveSensitivity(float sensitivity)
    {
        // ← ZMIANA: Clamp do 0-1200, nie 0.1-10
        sensitivity = Mathf.Clamp(sensitivity, 0f, 1200f);
        PlayerPrefs.SetFloat(KEY_SENSITIVITY, sensitivity);
        PlayerPrefs.Save();
    }
}