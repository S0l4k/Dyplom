using UnityEngine;

public class StairLoop : MonoBehaviour
{
    [Header("Segments")]
    public Transform startSegment;
    public Transform endSegment;

    [Header("Loop Logic")]
    public int loopCount = 0;
    public int loopsToTriggerDemon = 5;

    [Header("Demon")]
    public EnemyAI demon;
    public Transform demonWaitingPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!GameState.LoopSequenceActive) return;
        if (GameState.FinalChase) return;
        if (!other.CompareTag("Player")) return;

        loopCount++;
        Debug.Log($"[StairLoop] Loop #{loopCount}");

        if (loopCount == loopsToTriggerDemon)
        {
            TriggerDemonPhase();
        }

        Vector3 offset = other.transform.position - endSegment.position;

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            other.transform.position = startSegment.position + offset;
            cc.enabled = true;
        }
        else
        {
            other.transform.position = startSegment.position + offset;
        }
    }

    void TriggerDemonPhase()
    {
        if (GameState.DemonLoopPhase) return;

        GameState.DemonLoopPhase = true;

        demon.ai.enabled = false;
        demon.transform.position = demonWaitingPoint.position;
        demon.transform.rotation = demonWaitingPoint.rotation;

        Debug.Log("[StairLoop] Demon waiting at bottom.");
    }
}
