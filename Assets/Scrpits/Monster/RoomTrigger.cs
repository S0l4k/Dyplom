using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomTrigger : MonoBehaviour
{
    public string roomTag; // np. "Kitchen", "Bathroom"
    private bool playerInside = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;
            var demon = FindObjectOfType<DemonRoomPresence>();
            demon?.EnterRoom(roomTag);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerInside)
        {
            playerInside = false;
            var demon = FindObjectOfType<DemonRoomPresence>();
            demon?.ExitRoom();
        }
    }
}