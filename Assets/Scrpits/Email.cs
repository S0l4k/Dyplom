using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Email
{
    public string id;
    public string subject;
    public string preview;          // To, co widać na liście po lewej
    public List<Message> messages;  // 🔹 NOWE: konwersacja zamiast jednego body
    public bool isRead;
    public string sender;           // Główny nadawca (do podglądu)

    public Email(string id, string subject, string sender, string preview, List<Message> messages, bool isRead = false)
    {
        this.id = id;
        this.subject = subject;
        this.sender = sender;
        this.preview = preview;
        this.messages = messages;
        this.isRead = isRead;
    }
}