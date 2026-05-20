using System;
using UnityEngine;

[Serializable]
public class Message
{
    public string sender;           // "Main Character", "Father Wiktor Skrzat", "System"
    public string text;             // Treœæ wiadomoœci
    public bool isPlayer;           // Czy to gracz? (do wyrównania do prawej / koloru)
    public string timestamp;        // Opcjonalnie: "12:04", "Day 3", etc.

    public Message(string sender, string text, bool isPlayer = false, string timestamp = "")
    {
        this.sender = sender;
        this.text = text;
        this.isPlayer = isPlayer;
        this.timestamp = timestamp;
    }
}