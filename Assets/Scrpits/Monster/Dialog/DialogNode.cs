using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogNode
{
    [TextArea(3, 5)]
    public string npcLine;

    public string[] responses;
    public int[] nextNodeIndex;
    public UnityEvent[] responseEvents;
}
