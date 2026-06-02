using UnityEngine;

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

    public void SwitchCameraByEnding()
    {
        EndingSaveManager.EndingType ending = EndingSaveManager.LoadEnding();

        SetCameraGroupActive(defaultCameraGroup, false);
        SetCameraGroupActive(prisonCameraGroup, false);
        SetCameraGroupActive(hospitalCameraGroup, false);

        switch (ending)
        {
            case EndingSaveManager.EndingType.Cooperate:
                SetCameraGroupActive(prisonCameraGroup, true);
                break;

            case EndingSaveManager.EndingType.Defeat:
                SetCameraGroupActive(hospitalCameraGroup, true);
                break;

            case EndingSaveManager.EndingType.None:
            default:
                SetCameraGroupActive(defaultCameraGroup, true);
                break;
        }
    }

    private void SetCameraGroupActive(GameObject group, bool active)
    {
        if (group != null) group.SetActive(active);
    }

    public void OnNewGameClicked()
    {
        EndingSaveManager.ResetEnding();

        SwitchCameraByEnding();
    }

#if UNITY_EDITOR
    [ContextMenu("🔁 Force Default Camera")]
    private void Debug_ForceDefault()
    {
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.None);
        SwitchCameraByEnding();
    }

    [ContextMenu("🔒 Force Prison Camera")]
    private void Debug_ForcePrison()
    {
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.Cooperate);
        SwitchCameraByEnding();
    }

    [ContextMenu("🏥 Force Hospital Camera")]
    private void Debug_ForceHospital()
    {
        EndingSaveManager.SaveEnding(EndingSaveManager.EndingType.Defeat);
        SwitchCameraByEnding();
    }
#endif
}