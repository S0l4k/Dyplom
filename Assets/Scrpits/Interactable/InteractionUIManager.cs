// InteractionUIManager.cs
using UnityEngine;

public class InteractionUIManager : MonoBehaviour
{
    public static InteractionUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject crosshair;  // ✅ Przypisz prefab celownika w Inspectorze

    // Licznik: ile skryptów chce aktualnie pokazać celownik
    private int _activeRequests = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // opcjonalnie: przetrwa sceny
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ✅ Bezpieczny start: ukryj celownik
        if (crosshair != null) crosshair.SetActive(false);
    }

    /// <summary>
    /// Zgłoś chęć pokazania celownika (wywołuj gdy gracz MOŻE interaktować)
    /// </summary>
    public void RequestShow()
    {
        _activeRequests++;
        if (crosshair != null) crosshair.SetActive(true);
    }

    /// <summary>
    /// Wycofaj żądanie pokazania celownika (wywołuj gdy gracz NIE MOŻE interaktować)
    /// </summary>
    public void RequestHide()
    {
        _activeRequests = Mathf.Max(0, _activeRequests - 1);

        // Ukryj celownik TYLKO jeśli nikt już nie prosi o jego pokazanie
        if (_activeRequests == 0 && crosshair != null)
            crosshair.SetActive(false);
    }

    /// <summary>
    /// Wymuś ukrycie celownika (np. podczas dialogu, cutsceny, pościgu)
    /// </summary>
    public void ForceHide()
    {
        _activeRequests = 0;
        if (crosshair != null) crosshair.SetActive(false);
    }

    /// <summary>
    /// Debug: ile aktywnych żądań pokazania celownika?
    /// </summary>
    public int GetActiveRequestCount() => _activeRequests;
}