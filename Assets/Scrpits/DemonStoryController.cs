using UnityEngine;

public class DemonStoryController : MonoBehaviour
{
    public EnemyAI demon;

    public void StartFinalChase()
    {
        GameState.FinalChase = true;
        QuestManager.Instance.CompleteQuest("Go back to your flat");
        QuestManager.Instance.AddQuest("RUN");
        demon.ai.enabled = true;
        demon.chasing = true;

        Debug.Log("[Story] Final chase started.");
    }
}
