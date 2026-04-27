using UnityEngine;

public class CandleController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject flame;              // ✅ Child ze światłem (np. "Flame")
    public AtticQuestController quest;    // ✅ Link do questa

    [Header("Auto-light when player near")]
    public float lightRange = 2f;         // ✅ Dystans do automatycznego zapalenia

    private bool _isLit = false;
    private bool _hasPlayerWithCandle = false;

    void Start()
    {
        // ✅ Na start: niezapalona (chyba że chcesz inaczej)
        SetLit(false);
    }

    void Update()
    {
        // ✅ Jeśli już zapalona – nie robimy nic
        if (_isLit) return;

        // ✅ Sprawdź czy gracz z podniesioną świeczką jest blisko
        CheckPlayerProximity();
    }

    void CheckPlayerProximity()
    {
        // ✅ Znajdź gracza (cached w QuestController)
        PlayerController player = quest?.GetPlayer();
        if (player == null) return;

        // ✅ Sprawdź dystans
        float dist = Vector3.Distance(transform.position, player.transform.position);
        bool playerHasCandle = ItemPickup.HeldItemIsAtticCandle();  // ✅ Statyczna metoda

        _hasPlayerWithCandle = (dist <= lightRange) && playerHasCandle;

        // ✅ Zapal jeśli warunki spełnione
        if (_hasPlayerWithCandle)
        {
            SetLit(true);
        }
    }

    public void SetLit(bool lit)
    {
        if (_isLit == lit) return;  // ✅ Bez zmian jeśli stan się nie zmienił

        _isLit = lit;

        if (flame != null)
            flame.SetActive(_isLit);

        // ✅ Jeśli właśnie zapaliliśmy – zgłoś do questa
        if (_isLit && quest != null)
        {
            quest.OnCandleLit();
            Debug.Log($"[Candle] 🔥 Lit! Total: {quest.GetLitCount()}/{quest.candlesToLight}");
        }
    }

    public bool IsLit() => _isLit;
}