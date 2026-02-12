using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour
{
    [Header("Light References")]
    public Light[] apartmentLights;    // ✅ Światła w mieszkaniu
    public Color normalColor = Color.white;
    public Color demonColor = Color.red;

    [Header("Transition Settings")]
    public float transitionTime = 1.5f;
    public bool flickerEnabled = true;
    public float flickerSpeed = 2f;
    public float flickerIntensity = 0.3f;

    private Color[] originalColors;
    private float[] originalIntensities;
    private Coroutine activeTransition;
    private Coroutine activeFlicker;
    private bool isDemonPresent = false;

    void Start()
    {
        if (apartmentLights != null && apartmentLights.Length > 0)
        {
            originalColors = new Color[apartmentLights.Length];
            originalIntensities = new float[apartmentLights.Length];

            for (int i = 0; i < apartmentLights.Length; i++)
            {
                if (apartmentLights[i] != null)
                {
                    originalColors[i] = apartmentLights[i].color;
                    originalIntensities[i] = apartmentLights[i].intensity;
                }
            }
        }
    }

    // ✅ WYWOŁYWANE Z ApartmentTrigger PO RESPAWNIE DEMONA
    public void ActivateDemonLights()
    {
        if (apartmentLights == null || apartmentLights.Length == 0)
        {
            Debug.LogWarning("[LightController] Brak przypisanych świateł!");
            return;
        }

        isDemonPresent = true;

        // ✅ ZATRZYMAJ AKTYWNE COROUTINE
        if (activeTransition != null)
            StopCoroutine(activeTransition);
        if (activeFlicker != null)
            StopCoroutine(activeFlicker);

        activeTransition = StartCoroutine(TransitionToColor(demonColor));

        if (flickerEnabled)
            activeFlicker = StartCoroutine(FlickerRoutine());
    }

    // ✅ POPRAWIONE: BEZPIECZNE GASNIE ŚWIATŁ (zatrzymuje migotanie!)
    public void TurnOffAllLights()
    {
        if (apartmentLights == null) return;

        // ✅ KLUCZOWE: ZATRZYMAJ WSZYSTKIE COROUTINE MIGOTANIA
        isDemonPresent = false;
        if (activeTransition != null)
            StopCoroutine(activeTransition);
        if (activeFlicker != null)
            StopCoroutine(activeFlicker);

        // ✅ WYŁĄCZ WSZYSTKIE ŚWIATŁA (intensity + enabled)
        foreach (var light in apartmentLights)
        {
            if (light != null)
            {
                light.intensity = 0f;
                light.enabled = false; // ✅ KLUCZOWE: wyłącz światło fizycznie
                light.color = normalColor; // ✅ Reset koloru na biały
            }
        }

        Debug.Log("[LightController] 💡 Wszystkie światła ZGASZONE");
    }

    // ✅ POPRAWIONE: BEZPIECZNE PRZYWRACANIE ŚWIATŁ
    public void RestoreLights()
    {
        if (apartmentLights == null || apartmentLights.Length == 0) return;

        isDemonPresent = false;

        // ✅ ZATRZYMAJ AKTYWNE COROUTINE
        if (activeTransition != null)
            StopCoroutine(activeTransition);
        if (activeFlicker != null)
            StopCoroutine(activeFlicker);

        // ✅ WŁĄCZ ŚWIATŁA Z POWROTEM
        foreach (var light in apartmentLights)
        {
            if (light != null)
            {
                light.enabled = true;
                light.intensity = 0f; // ✅ Zaczynamy od 0 by płynnie przejść do pełnej intensywności
                light.color = normalColor;
            }
        }

        activeTransition = StartCoroutine(TransitionToColor(normalColor));
        Debug.Log("[LightController] 💡 Światła przywrócone do normy");
    }

    private IEnumerator TransitionToColor(Color targetColor)
    {
        Color[] startColors = new Color[apartmentLights.Length];
        float[] startIntensities = new float[apartmentLights.Length];

        for (int i = 0; i < apartmentLights.Length; i++)
        {
            if (apartmentLights[i] != null && apartmentLights[i].enabled)
            {
                startColors[i] = apartmentLights[i].color;
                startIntensities[i] = apartmentLights[i].intensity;
            }
        }

        float elapsed = 0f;
        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionTime);

            for (int i = 0; i < apartmentLights.Length; i++)
            {
                if (apartmentLights[i] != null && apartmentLights[i].enabled)
                {
                    apartmentLights[i].color = Color.Lerp(startColors[i], targetColor, t);

                    if (isDemonPresent && flickerEnabled && targetColor == demonColor)
                    {
                        float flicker = Mathf.Sin(Time.time * flickerSpeed * 4f) * flickerIntensity * 0.2f;
                        apartmentLights[i].intensity = Mathf.Lerp(startIntensities[i], originalIntensities[i], t) + flicker;
                    }
                    else
                    {
                        apartmentLights[i].intensity = Mathf.Lerp(startIntensities[i], originalIntensities[i], t);
                    }
                }
            }

            yield return null;
        }

        for (int i = 0; i < apartmentLights.Length; i++)
        {
            if (apartmentLights[i] != null && apartmentLights[i].enabled)
            {
                apartmentLights[i].color = targetColor;
                apartmentLights[i].intensity = originalIntensities[i];
            }
        }
    }

    private IEnumerator FlickerRoutine()
    {
        while (isDemonPresent && flickerEnabled)
        {
            for (int i = 0; i < apartmentLights.Length; i++)
            {
                if (apartmentLights[i] != null && apartmentLights[i].enabled)
                {
                    float flicker = Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity;
                    apartmentLights[i].intensity = originalIntensities[i] + flicker;
                }
            }

            yield return null;
        }
    }
}