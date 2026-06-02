using UnityEngine;
using System.Collections;

public class DemonStoryController : MonoBehaviour
{
    public EnemyAI demon;

    public void StartFinalChase()
    {
        GameState.FinalChase = true;

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest("Go back to your flat");
        }

        if (demon != null && demon.ai != null)
        {
            demon.ai.enabled = true;
            demon.chasing = true;
        }

        StartCoroutine(AddRunQuestDelayed());
    }

    private IEnumerator AddRunQuestDelayed()
    {
        yield return new WaitForSeconds(4.0f);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddQuest("RUN");
        }
    }
}