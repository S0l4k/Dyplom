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
        CreateExorcismThread();
    }
    void CreateExorcismThread()
    {
        var convo = new List<Message> {
        // 1. Gracz pisze
        new Message("Jack White",
            "Blessings,\n\nI am writing to you because on the website of the Church of the Blessed Virgin Mary, you were listed as an exorcist.\n\nMy name is Jack. I spent the last fifteen years in a psychiatric institution because of visions of a strange entitythat first appeared in my life when I was a child, shortly after my friends and I tried summoning spirits as a game. \n\nAfter years of psychiatric treatment and medication, the visions eventually stopped. Unfortunately, the entity has recently begun appearing again.\n\nI see it in puddle reflections, store windows, sometimes standing far behind crowds of people. Every day it gets closer to me. It has started appearing in my dreams again, and I can hear it whispering into my ear. \n\nDo you know what kind of entity this could be? \n\nSincerely, \n\nJack White",
            isPlayer: true, timestamp: "Day 1"),

        // 2. Ksiądz odpowiada
        new Message("Father Wiktor Skrzat",
            "Blessings,\n\nFrom what you describe, it is possible that you are dealing with a demonic entity. Unfortunately, without more details, I cannot determine exactly what kind of demon it may be nor how powerful it is.\n\nAt the moment, I am in Romania. I was called here to investigate a series of unexplained deaths occurring at an isolated raccoon farm.\n\nNevertheless, I ask that you describe everything in as much detail as possible.\r\n\r\nHow does the entity communicate with you? Does it command you to harm yourself or others? Has it ever appeared helpful? Has it tried to gain your trust through seemingly peaceful intentions? \n\nThe more information you provide, the easier it will be for me to assess your situation. \n\nMay God protect you,\nFather Wiktor Skrzat",
            isPlayer: false, timestamp: "Day 1, 14:22"),

        // 3. Gracz kontynuuje
        new Message("Jack White",
            "Alright. I'll try to explain everything.\n\nWhen it first appeared, I only saw it occasionally, always from far away Standing at bus stops while I passed by in a car. Sometimes outside my window. Sometimes hidden in the background of dreams. It never spoke — it simply watched. \n\n Only after it started getting closer did it finally talk to me.\r\n\r\nIt told me it was my friend. It helped me. It whispered answers during tests at school, comforted me when my parents called me a freak during arguments. Once, it even helped me stand up to a boy who used to bully me. \n\nEventually it became part of my daily life. It never left me alone. \n\nThat was also when it convinced me to eat broken glass. After that incident, I was institutionalized. \n\nIf you have any more questions, I’ll answer them all.",
            isPlayer: true, timestamp: "Day 2"),
        // 4. Ksiądz odpowiada
        new Message("Father Wiktor Skrzat",
            "Based on the details you provided, I believe you may be dealing with a servant of Asmodeus — or possibly Asmodeus himself.\r\n\r\nIf the entity persuaded you to mutilate yourself in such a horrific manner, then it is extremely dangerous. The emotional manipulation you described only strengthens this conclusion.\r\n\r\nIf my suspicions are correct, exorcism will be necessary.\r\n\r\nI will return to the country in two weeks. Until then, I ask that you read the Holy Scriptures daily and make the sign of the cross whenever the entity appears. You should also carry a crucifix with you at all times.\r\n\r\nPlease document every anomaly you experience and continue sending them to me.\r\n\r\nDo not allow fear to consume you.\r\n\r\nIn God’s grace,\r\nFather Wiktor Skrzat",
            isPlayer: false, timestamp: "Day 2"),
        // 5 Gracz pisze
            new Message("Jack White",
            "It’s getting worse.\r\n\r\nI did everything you told me to do. At first it worked — the entity disappeared, or didn’t appear at all.\r\n\r\nBut recently, when I made the sign of the cross, the demon only laughed and started walking toward me. If I hadn’t had the crucifix, I think it would have gotten to me.\r\n\r\nYesterday, while reading the Bible, I suddenly froze with fear. I heard a child crying from inside the drawer beside my desk. When I opened it, the crying stopped — and then something violently forced every drawer open at once.\r\n\r\nPlease help me.",
            isPlayer: true, timestamp: "Day 5, 14:22"),
        // 6 Ksiadz odpowiada
            new Message("Father Wiktor Skrzat",
            "Do not be afraid, my child.\r\n\r\nThe Lord sees your suffering and your struggle. He will not allow you to be lost — but you must endure.\r\n\r\nI will return in three days. We will begin the exorcism then.\r\n\r\nUntil that time, continue praying and under no circumstances speak to the entity except through prayer.\r\n\r\nGod be with you.",
            isPlayer: false, timestamp: "Day 5, 14:22"),
        // 7 Ksiadz odpowiada
            new Message("Jack White",
            "It’s taking control.\r\n\r\nWhile I was reading the Bible, it appeared right in front of me. When I raised the crucifix, it walked toward me and snapped my wrist backward like it was nothing.\r\n\r\nThen it touched the Bible.\r\n\r\nIt burned in its hands.",
            isPlayer: true, timestamp: "Day 5, 14:22"),
        // 10. Systemowe powiadomienie
        new Message("System",
            "<b>AUTOMATED DELIVERY FAILURE NOTICE</b>\n\nYour message could not be delivered to:\n<b>fr.w.skrzat@██████.pl</b>\n\nReason: Recipient address is currently unavailable.",
            isPlayer: false, timestamp: "ERROR"),

        
    };

        var email = new Email(
            id: "exorcism_001",
            subject: "Seeking help — entity visions",
            sender: "Main Character",
            preview: "Blessings, I am writing to you because...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
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