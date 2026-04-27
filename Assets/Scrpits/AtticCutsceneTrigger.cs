using UnityEngine;

public class AtticCutsceneTrigger : MonoBehaviour
{
    public AtticQuestController questController;
    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (questController == null) return;

        // ✅ Sprawdź czy gracz wszedł
        if (!other.CompareTag("Player") && other.GetComponent<PlayerController>() == null)
            return;

        // ✅ Nie ma checka AreAllCandlesLit() – trigger jest włączony TYLKO gdy wszystkie świeczki zapalone!

        _triggered = true;
        Debug.Log("[AtticTrigger] 🎬 Player entered trigger – starting cutscene");

        questController.StartCutscene();
    }
}