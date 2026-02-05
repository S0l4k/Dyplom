using UnityEngine;

public class DemonStoryController : MonoBehaviour
{
    public EnemyAI demon;

    public void StartFinalChase()
    {
        GameState.FinalChase = true;

        demon.ai.enabled = true;
        demon.chasing = true;

        Debug.Log("[Story] Final chase started.");
    }
}
