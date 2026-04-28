// CrosshairHelper.cs – wrzuć do folderu Scripts
using UnityEngine;

public static class CrosshairHelper
{
    private static GameObject _crosshair;

    public static GameObject Get()
    {
        if (_crosshair == null)
        {
            // ⭐ Szuka po TAGU – zero przypisywania w Inspectorze!
            _crosshair = GameObject.FindWithTag("Crosshair");
            if (_crosshair == null)
                Debug.LogError("[CrosshairHelper] Brak obiektu z tagiem 'Crosshair' w scenie!");
        }
        return _crosshair;
    }

    public static void Show() => Get()?.SetActive(true);
    public static void Hide() => Get()?.SetActive(false);
}