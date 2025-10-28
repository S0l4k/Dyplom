using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public GameObject ON;
    public GameObject OFF;
    private bool isON;

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
    }

    public void TurnOff()
    {
        ON.SetActive(false);
        OFF.SetActive(true);
        isON = false;
    }
}
