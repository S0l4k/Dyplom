using System;

[Serializable]
public class Email
{
    public string id;
    public string subject;
    public string sender;
    public string preview;
    public string body;
    public bool isRead;

    // Konstruktor pomocniczy
    public Email(string id, string subject, string sender, string preview, string body, bool isRead = false)
    {
        this.id = id;
        this.subject = subject;
        this.sender = sender;
        this.preview = preview;
        this.body = body;
        this.isRead = isRead;
    }
}