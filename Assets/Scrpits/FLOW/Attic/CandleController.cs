using UnityEngine;

public class CandleController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject flame;              
    public AtticQuestController quest;    

    [Header("Auto-light when player near")]
    public float lightRange = 2f;         

    private bool _isLit = false;
    private bool _hasPlayerWithCandle = false;

    void Start()
    {
        SetLit(false);
    }

    void Update()
    {
        if (_isLit) return;
        CheckPlayerProximity();
    }

    void CheckPlayerProximity()
    {
        PlayerController player = quest?.GetPlayer();
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        bool playerHasCandle = ItemPickup.HeldItemIsAtticCandle(); 

        _hasPlayerWithCandle = (dist <= lightRange) && playerHasCandle;

        if (_hasPlayerWithCandle)
        {
            SetLit(true);
        }
    }

    public void SetLit(bool lit)
    {
        if (_isLit == lit) return; 

        _isLit = lit;

        if (flame != null)
            flame.SetActive(_isLit);

        if (_isLit && quest != null)
        {
            quest.OnCandleLit();
        }
    }

    public bool IsLit() => _isLit;
}