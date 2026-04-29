using UnityEngine;
// ✅ Nie potrzebujesz UnityEditor – używamy [ContextMenu]

public class MainMenuCameraController : MonoBehaviour
{
    [Header("Camera Groups")]
    [Tooltip("Domyślna kamera (pierwszy start / New Game)")]
    public GameObject defaultCameraGroup;

    [Tooltip("Kamera więzienia (Ending 1: współpraca z demonem)")]
    public GameObject prisonCameraGroup;

    [Tooltip("Kamera szpitala (Ending 2: pokonanie demona)")]
    public GameObject hospitalCameraGroup;

    [Header("Settings")]
    [Tooltip("Czy automatycznie przełączyć kamery na Start()")]
    public bool switchOnStart = true;

    private void Start()
    {
        if (switchOnStart)
        {
            SwitchCameraByEnding();
        }
    }

    /// <summary>
    /// Przełącza kamery na podstawie zapisanego zakończenia.
    /// </summary>
    public void SwitchCameraByEnding()
    {
        EndingSaveManager.EndingType ending = EndingSaveManager.LoadEnding();

        // Najpierw wyłącz WSZYSTKIE grupy
        SetCameraGroupActive(defaultCameraGroup, false);
        SetCameraGroupActive(prisonCameraGroup, false);
        SetCameraGroupActive(hospitalCameraGroup, false);

        // Włącz odpowiednią
        switch (ending)
        {
            case EndingSaveManager.EndingType.Cooperate:
                SetCameraGroupActive(prisonCameraGroup, true);
                Debug.Log("[MainMenu] 🔓 Prison camera active (Ending 1)");
                break;

            case EndingSaveManager.EndingType.Defeat:
                SetCameraGroupActive(hospitalCameraGroup, true);
                Debug.Log("[MainMenu] 🔓 Hospital camera active (Ending 2)");
                break;

            case EndingSaveManager.EndingType.None:
            default:
                SetCameraGroupActive(defaultCameraGroup, true);
                Debug.Log("[MainMenu] 🔓 Default camera active (New Game)");
                break;
        }
    }

    private void SetCameraGroupActive(GameObject group, bool active)
    {
        if (group != null) group.SetActive(active);
    }

    /// <summary>
    /// ✅ KLUCZOWA METODA: Reset + przełączenie na domyślną + ładowanie gry
    /// Wywołuj z przycisku "New Game" w UI.
    /// </summary>
    public void OnNewGameClicked()
    {
        Debug.Log("[MainMenu] 🎮 New Game clicked - resetting ending state");

        // 1️⃣ Reset zapisu endingu
        EndingSaveManager.ResetEnding();

        // 2️⃣ Natychmiast przełącz kamerę na domyślną (jeszcze w menu!)
        SwitchCameraByEnding();

        // ✅ KONIEC – resztę (loading screen, cutscenka, LoadScene) obsłuż w swoim skrypcie!
        Debug.Log("[MainMenu] ✅ Ending reset + camera switched. Proceed with your loading flow.");
    }

    /// <summary>
    /// Metoda ładująca scenę gry – dostosuj nazwę do swojego projektu!
    /// </summary>
   

    // === 🧪 DEBUG: opcje w menu kontekstowym (prawy klik na komponencie) ===
#if UNITY_EDITOR
    [ContextMenu("🔁 Force Default Camera")]
    private void Debug_ForceDefault()
    {
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.None);
        SwitchCameraByEnding();
        Debug.Log("[DEBUG] Forced Default Camera");
    }

    [ContextMenu("🔒 Force Prison Camera")]
    private void Debug_ForcePrison()
    {
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.Cooperate);
        SwitchCameraByEnding();
        Debug.Log("[DEBUG] Forced Prison Camera");
    }

    [ContextMenu("🏥 Force Hospital Camera")]
    private void Debug_ForceHospital()
    {
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.Defeat);
        SwitchCameraByEnding();
        Debug.Log("[DEBUG] Forced Hospital Camera");
    }
#endif
}