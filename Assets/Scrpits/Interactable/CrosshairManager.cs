// CrosshairManager.cs – wrzuć do folderu Scripts
using UnityEngine;

public class CrosshairManager : MonoBehaviour
{
    // ✅ Public static reference – przypisujesz RAZ w Inspectorze
    public  GameObject crosshair;

    // ✅ Helper methods dla wygody (opcjonalne)
    public  void Show() => crosshair?.SetActive(true);
    public  void Hide() => crosshair?.SetActive(false);
}