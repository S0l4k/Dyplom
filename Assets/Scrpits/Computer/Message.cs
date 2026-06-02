using System;
using UnityEngine;

[Serializable]
public class Message
{
    public string sender;          
    public string text;             
    public bool isPlayer;          
    public string timestamp;        

    public Message(string sender, string text, bool isPlayer = false, string timestamp = "")
    {
        this.sender = sender;
        this.text = text;
        this.isPlayer = isPlayer;
        this.timestamp = timestamp;
    }
}