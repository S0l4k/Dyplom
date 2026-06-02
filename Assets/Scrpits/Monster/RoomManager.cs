using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    private string currentRoom = "";
    private HashSet<string> activeRooms = new HashSet<string>(); 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayerEnteredRoom(string roomTag)
    {
        activeRooms.Add(roomTag);
        UpdateCurrentRoom();
    }

    public void PlayerExitedRoom(string roomTag)
    {
        activeRooms.Remove(roomTag);
        UpdateCurrentRoom();
    }

    private void UpdateCurrentRoom()
    {
        string newRoom = activeRooms.Count > 0 ? activeRooms.First() : "";

        if (newRoom != currentRoom)
        {
            currentRoom = newRoom;

            var demon = FindObjectOfType<DemonRoomPresence>();
            if (demon != null)
            {
                if (!string.IsNullOrEmpty(newRoom))
                    demon.EnterRoom(newRoom);
                else
                    demon.ExitRoom();
            }
        }
    }

    public string GetCurrentRoom() => currentRoom;
}