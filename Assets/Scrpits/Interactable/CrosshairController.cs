// CrosshairController.cs
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public static CrosshairController Instance { get; private set; }
    private int _activeCount = 0;
    private bool _isVisible = false;

    void Awake()
    {
        Instance = this;
        _isVisible = false;
    }

    // ✅ Wywołaj gdy gracz MOŻE interaktować
    public void Show()
    {
        _activeCount++;
        if (!_isVisible)
        {
            _isVisible = true;
            gameObject.SetActive(true);
        }
    }

    // ✅ Wywołaj gdy gracz PRZESTAJE celować
    public void Hide()
    {
        _activeCount = Mathf.Max(0, _activeCount - 1);
        if (_activeCount == 0 && _isVisible)
        {
            _isVisible = false;
            gameObject.SetActive(false);
        }
    }

    // ✅ Wymuś ukrycie (dialog, komputer, cutscena)
    public void ForceHide()
    {
        _activeCount = 0;
        _isVisible = false;
        gameObject.SetActive(false);
    }
}