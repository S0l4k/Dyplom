using UnityEngine;
using FMODUnity;

public class Flashlight : MonoBehaviour
{
    public GameObject ON;
    public GameObject OFF;
    private bool isON;
    [SerializeField] private EventReference flashlightEvent;

    void Start()
    {
        ON.SetActive(false);
        OFF.SetActive(true);
        isON = false;
    }

    public void TurnOn()
    {
        ON.SetActive(true);
        OFF.SetActive(false);
        isON = true;

        // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
        AudioManager.Instance.PlaySFX(flashlightEvent);
    }

    public void TurnOff()
    {
        ON.SetActive(false);
        OFF.SetActive(true);
        isON = false;

        // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
        AudioManager.Instance.PlaySFX(flashlightEvent);
    }
}