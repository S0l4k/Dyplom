using UnityEngine;
using System.Collections;

public class DemonStoryController : MonoBehaviour
{
    public EnemyAI demon;

    public void StartFinalChase()
    {
        Debug.Log("[DemonStoryController] StartFinalChase wywołane!");
        GameState.FinalChase = true;

        // 1. Natychmiast zakończ poprzedni quest
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Go back to your flat");
        }

        // 2. Włącz demona
        if (demon != null && demon.ai != null)
        {
            demon.ai.enabled = true;
            demon.chasing = true;
            Debug.Log("[DemonStoryController] Demon AI włączony i zaczyna pościg.");
        }

        // 3. Odpal korutinę z opóźnieniem
        StartCoroutine(AddRunQuestDelayed());
    }

    private IEnumerator AddRunQuestDelayed()
    {
        Debug.Log("[DemonStoryController] Rozpoczynanie odliczania 4s do questa RUN...");
        yield return new WaitForSeconds(4.0f);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest("RUN");
            Debug.Log("[DemonStoryController] ✅ Quest 'RUN' został DODANY.");
        }
        else
        {
            Debug.LogError("[DemonStoryController] ❌ QuestManager jest NULL! Nie dodano questa RUN.");
        }
    }
}