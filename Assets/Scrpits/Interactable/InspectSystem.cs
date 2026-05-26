using UnityEngine;

public class InspectSystem : MonoBehaviour
{
    public float rotationSpeed = 100f;
   // ✅ Klawisz do wyjścia (domyślnie ESC)

    private Transform _target;
    private bool isInspecting = false;

    // ✅ STARA METODA – dla kompatybilności z NarrativeInspectTrigger
    public void SetTarget(Transform target)
    {
        if (target == null) return;

        _target = target;
        isInspecting = true;

        // ✅ NOWE: Ustaw flagę przy wejściu w inspect
        GameState.IsInspecting = true;

        Debug.Log("[InspectSystem] 🔍 SetTarget + IsInspecting = true");
    }

    // ✅ NOWA METODA – wyjście z inspect (przejście do flashbacku)
    // Wywołuj ją z przycisku UI lub przy ESC
    public void ExitInspectMode()
    {
        if (!isInspecting) return;

        _target = null;
        isInspecting = false;

        // ✅ PRZEŁĄCZ FLAGI: koniec inspect, start flashbacku
        GameState.IsInspecting = false;
        GameState.IsInFlashback = true;

        Debug.Log("[InspectSystem] 🔓 ExitInspectMode → IsInFlashback = true");
    }

    // ✅ ALTERNATYWA: Proste wyłączenie inspect bez przechodzenia w flashback
    // (jeśli chcesz tylko zamknąć inspect, a nie startować flashbacku)
    public void CloseInspectOnly()
    {
        if (!isInspecting) return;

        _target = null;
        isInspecting = false;
        GameState.IsInspecting = false;

        Debug.Log("[InspectSystem] ❌ CloseInspectOnly → IsInspecting = false");
    }

    void Update()
    {
        if (_target == null) return;

  

        // ✅ Obracanie myszką
        if (Input.GetMouseButtonDown(0))
            previousMousePosition = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - previousMousePosition;
            float rotX = delta.y * rotationSpeed * Time.deltaTime;
            float rotY = -delta.x * rotationSpeed * Time.deltaTime;

            Quaternion rotation = Quaternion.Euler(rotX, rotY, 0);
            _target.rotation = rotation * _target.rotation;

            previousMousePosition = Input.mousePosition;
        }
    }

    private Vector3 previousMousePosition;
}