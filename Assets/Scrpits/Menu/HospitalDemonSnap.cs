using UnityEngine;
using System.Collections;

public class HospitalDemonSnap : MonoBehaviour
{
    [Header("Demon Objects")]
    [Tooltip("Przypisz dokładnie 3 obiekty demona (kolejność ma znaczenie)")]
    public GameObject[] demonObjects;

    [Header("Controller")]
    [Tooltip("Opcjonalnie: odniesienie do kontrolera kamer (do sprawdzania aktywnej grupy)")]
    public MainMenuCameraController cameraController;

    [Header("Settings")]
    [Tooltip("Czas między przełączeniami (sekundy)")]
    public float snapInterval = 2.5f;

    [Tooltip("Czy snapowanie ma działać TYLKO gdy aktywna jest kamera szpitala")]
    public bool onlyWhenHospitalActive = true;

    private Coroutine snapRoutine;
    private int activeIndex = 0;

    private void Start()
    {
        if (demonObjects == null || demonObjects.Length == 0)
        {
            return;
        }

        SetActiveDemon(0);
        StartCoroutine(MonitorAndSnap());
    }

    private IEnumerator MonitorAndSnap()
    {
        while (true)
        {
            bool shouldSnap = !onlyWhenHospitalActive ||
                              (cameraController != null &&
                               cameraController.hospitalCameraGroup != null &&
                               cameraController.hospitalCameraGroup.activeSelf);

            if (shouldSnap)
            {
                if (snapRoutine == null)
                    snapRoutine = StartCoroutine(SnapLoop());
            }
            else
            {
                if (snapRoutine != null)
                {
                    StopCoroutine(snapRoutine);
                    snapRoutine = null;
                }
                foreach (var go in demonObjects) if (go != null) go.SetActive(false);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator SnapLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(snapInterval);
            activeIndex = (activeIndex + 1) % demonObjects.Length;
            SetActiveDemon(activeIndex);
        }
    }

    private void SetActiveDemon(int index)
    {
        for (int i = 0; i < demonObjects.Length; i++)
        {
            if (demonObjects[i] != null)
                demonObjects[i].SetActive(i == index);
        }
    }
}