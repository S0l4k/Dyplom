using UnityEngine;
using System.Collections.Generic;

public class MailSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private MailboxUI mailboxUI;
    [SerializeField] private GameObject mailboxPanel; // panel z UI maili

    private List<Email> allEmails = new List<Email>();
    private List<Email> inbox = new List<Email>(); // tylko te, które gracz już "otrzymał"

    void Start()
    {
        // 🔹 TESTOWE DANE (usuniesz, gdy zrobisz triggerów w grze)
        AddEmailToInbox(new Email("001", "Nowy stream Mammona", "Mammon", "Użytkownik mammon transmituje na żywo",
            "<color=#FFD700>Mammon</color>\nUżytkownik mammon transmituje na żywo \n\nWbijaj na streama i zagrnij bana na za free \n\nOpona jeszcze rozgrzana", false));

        AddEmailToInbox(new Email("002", "Powiększ swojego piekelnika", "Diego", "Powiększ swojego piekielnika",
            "<color=#88ccff>Diego</color>\nPowiększ swojego piekielnika \n\n10 CM \n\n15 CM \n\n20 CM \n\n30 CM \n\nOSTROŻNIE!!!"));

        AddEmailToInbox(new Email("003", "Samotne mamuśki", "Mamuśka", "Samotne mamuśki już czekają na Ciebie",
           "<color=#88ccff>Samotne Mamuśki</color>\nWITAJ UŻYTKOWNIKU! \n\nSamotne mamuśki czekają aż je odwiedzisz już teraz"));

        AddEmailToInbox(new Email("004", "Nowy stream Mammona", "Mammon", "Użytkownik mammon transmituje na żywo",
       "<color=#FFD700>Mammon</color>\nUżytkownik mammon transmituje na żywo \n\nWbijaj na streama i zagrnij bana na za free \n\nOpona jeszcze rozgrzana", false));

        AddEmailToInbox(new Email("005", "Powiększ swojego piekelnika", "Diego", "Powiększ swojego piekielnika",
            "<color=#88ccff>Diego</color>\nPowiększ swojego piekielnika \n\n10 CM \n\n15 CM \n\n20 CM \n\n30 CM \n\nOSTROŻNIE!!!"));

        AddEmailToInbox(new Email("006", "Samotne mamuśki", "Mamuśka", "Samotne mamuśki już czekają na Ciebie",
           "<color=#88ccff>Samotne Mamuśki</color>\nWITAJ UŻYTKOWNIKU! \n\nSamotne mamuśki czekają aż je odwiedzisz już teraz"));

        AddEmailToInbox(new Email("007", "Nowy stream Mammona", "Mammon", "Użytkownik mammon transmituje na żywo",
       "<color=#FFD700>Mammon</color>\nUżytkownik mammon transmituje na żywo \n\nWbijaj na streama i zagrnij bana na za free \n\nOpona jeszcze rozgrzana", false));

        AddEmailToInbox(new Email("008", "Powiększ swojego piekelnika", "Diego", "Powiększ swojego piekielnika",
            "<color=#88ccff>Diego</color>\nPowiększ swojego piekielnika \n\n10 CM \n\n15 CM \n\n20 CM \n\n30 CM \n\nOSTROŻNIE!!!"));

        AddEmailToInbox(new Email("009", "Samotne mamuśki", "Mamuśka", "Samotne mamuśki już czekają na Ciebie",
           "<color=#88ccff>Samotne Mamuśki</color>\nWITAJ UŻYTKOWNIKU! \n\nSamotne mamuśki czekają aż je odwiedzisz już teraz"));
    }

    // ✅ Dodaj mail do skrzynki (używaj tego w trakcie gry)
    public void AddEmailToInbox(Email email)
    {
        if (allEmails.Exists(e => e.id == email.id)) return; // unikalne ID
        allEmails.Add(email);
        inbox.Add(email);

        // Jeśli UI jest otwarte → odśwież listę
        if (mailboxPanel.activeSelf) mailboxUI.RefreshList(inbox);

        // Tutaj możesz odpalić dźwięk/powiadomienie: "Nowa wiadomość!"
        Debug.Log($"📩 Nowy mail: {email.subject}");
    }

    // 🖲️ Otwarcie skrzynki (przypisz do przycisku na HUD)
    public void OpenMailbox()
    {
        mailboxPanel.SetActive(true);
        mailboxUI.Init(inbox);
    }

    // ❌ Zamknięcie skrzynki
    public void CloseMailbox()
    {
        mailboxPanel.SetActive(false);
    }
}