using UnityEngine;

public class StairLoop : MonoBehaviour
{
    public Transform startSegment; // gdzie gracz wraca
    public Transform endSegment;   // koniec segmentu
    public bool loopingActive = false;

private void OnTriggerEnter(Collider other)
{
    if (!loopingActive) return;

    if (other.CompareTag("Player"))
    {
        Vector3 offset = other.transform.position - endSegment.position;

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false; // wy³¹czenie CC na chwilê
            other.transform.position = startSegment.position + offset;
            cc.enabled = true;  // w³¹czamy z powrotem
        }
        else
        {
            other.transform.position = startSegment.position + offset; // fallback
        }

        Debug.Log("[StairLoop] Gracz teleportowany do pocz¹tku segmentu.");
    }
}

}
