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
        CreateDoctorThread();
        CreateExorcismThread();
        CreateMaryThread();
        CreateIoculatorThread();
        CreatePyzotekaThread();
        CreateFishingThread();
        CreateKamerkiThread();
    }
    void CreateExorcismThread()
    {
        var convo = new List<Message> {
        // 1. Gracz pisze
        new Message("Jack White",
            "Blessings,\n\nI am writing to you because on the website of the Church of the Blessed Virgin Mary, you were listed as an exorcist.\n\nMy name is Jack. I spent the last fifteen years in a psychiatric institution because of visions of a strange entitythat first appeared in my life when I was a child, shortly after my friends and I tried summoning spirits as a game. \n\nAfter years of psychiatric treatment and medication, the visions eventually stopped. Unfortunately, the entity has recently begun appearing again.\n\nI see it in puddle reflections, store windows, sometimes standing far behind crowds of people. Every day it gets closer to me. It has started appearing in my dreams again, and I can hear it whispering into my ear. \n\nDo you know what kind of entity this could be? \n\nSincerely, \n\nJack White",
            isPlayer: true, timestamp: ""),

        // 2. Ksiądz odpowiada
        new Message("Father Wiktor Skrzat",
            "Blessings,\n\nFrom what you describe, it is possible that you are dealing with a demonic entity. Unfortunately, without more details, I cannot determine exactly what kind of demon it may be nor how powerful it is.\n\nAt the moment, I am in Romania. I was called here to investigate a series of unexplained deaths occurring at an isolated raccoon farm.\n\nNevertheless, I ask that you describe everything in as much detail as possible.\r\n\r\nHow does the entity communicate with you? Does it command you to harm yourself or others? Has it ever appeared helpful? Has it tried to gain your trust through seemingly peaceful intentions? \n\nThe more information you provide, the easier it will be for me to assess your situation. \n\nMay God protect you,\nFather Wiktor Skrzat",
            isPlayer: false, timestamp: ""),

        // 3. Gracz kontynuuje
        new Message("Jack White",
            "Alright. I'll try to explain everything.\n\nWhen it first appeared, I only saw it occasionally, always from far away Standing at bus stops while I passed by in a car. Sometimes outside my window. Sometimes hidden in the background of dreams. It never spoke — it simply watched. \n\n Only after it started getting closer did it finally talk to me.\r\n\r\nIt told me it was my friend. It helped me. It whispered answers during tests at school, comforted me when my parents called me a freak during arguments. Once, it even helped me stand up to a boy who used to bully me. \n\nEventually it became part of my daily life. It never left me alone. \n\nThat was also when it convinced me to eat broken glass. After that incident, I was institutionalized. \n\nIf you have any more questions, I’ll answer them all.",
            isPlayer: true, timestamp: ""),
        // 4. Ksiądz odpowiada
        new Message("Father Wiktor Skrzat",
            "Based on the details you provided, I believe you may be dealing with a servant of Asmodeus — or possibly Asmodeus himself.\r\n\r\nIf the entity persuaded you to mutilate yourself in such a horrific manner, then it is extremely dangerous. The emotional manipulation you described only strengthens this conclusion.\r\n\r\nIf my suspicions are correct, exorcism will be necessary.\r\n\r\nI will return to the country in two weeks. Until then, I ask that you read the Holy Scriptures daily and make the sign of the cross whenever the entity appears. You should also carry a crucifix with you at all times.\r\n\r\nPlease document every anomaly you experience and continue sending them to me.\r\n\r\nDo not allow fear to consume you.\r\n\r\nIn God’s grace,\r\nFather Wiktor Skrzat",
            isPlayer: false, timestamp: ""),
        // 5 Gracz pisze
            new Message("Jack White",
            "It’s getting worse.\r\n\r\nI did everything you told me to do. At first it worked — the entity disappeared, or didn’t appear at all.\r\n\r\nBut recently, when I made the sign of the cross, the demon only laughed and started walking toward me. If I hadn’t had the crucifix, I think it would have gotten to me.\r\n\r\nYesterday, while reading the Bible, I suddenly froze with fear. I heard a child crying from inside the drawer beside my desk. When I opened it, the crying stopped — and then something violently forced every drawer open at once.\r\n\r\nPlease help me.",
            isPlayer: true, timestamp: ""),
        // 6 Ksiadz odpowiada
            new Message("Father Wiktor Skrzat",
            "Do not be afraid, my child.\r\n\r\nThe Lord sees your suffering and your struggle. He will not allow you to be lost — but you must endure.\r\n\r\nI will return in three days. We will begin the exorcism then.\r\n\r\nUntil that time, continue praying and under no circumstances speak to the entity except through prayer.\r\n\r\nGod be with you.",
            isPlayer: false, timestamp: ""),
        // 7 gracz pisze
            new Message("Jack White",
            "It’s taking control.\r\n\r\nWhile I was reading the Bible, it appeared right in front of me. When I raised the crucifix, it walked toward me and snapped my wrist backward like it was nothing.\r\n\r\nThen it touched the Bible.\r\n\r\nIt burned in its hands.",
            isPlayer: true, timestamp: ""),
        // 8 ksiadz odpowiada
            new Message("Father Wiktor Skrzat",
            "Hold on for one more day.\r\n\r\nTomorrow morning I will come directly to your home. Send me your address.\r\n\r\nNo matter what happens, do not surrender your will to it.",
            isPlayer: false, timestamp: ""),
        // 9 demon pisze
            new Message("Jack White",
            "You will not separate us.\r\n\r\nThe power of your God means nothing here.",
            isPlayer: true, timestamp: ""),
        // 10 ksiądz pisze
            new Message("Father Wiktor Skrzat",
            "In the name of Jesus Christ, I command you to release this man.\r\n\r\nYou have no dominion over him, demon.",
            isPlayer: false, timestamp: ""),
        // 11 demon
            new Message("Jack White",
            "You and your God are pathetic.\r\n\r\nHave a safe flight, Father.",
            isPlayer: true, timestamp: ""),
        // 12. Systemowe powiadomienie
        new Message("System",
            "<b>AUTOMATED DELIVERY FAILURE NOTICE</b>\n\nYour message could not be delivered to:\n<b>fr.w.skrzat@██████.pl</b>\n\nReason: Recipient address is currently unavailable.",
            isPlayer: false, timestamp: "ERROR"),

        
    };

        var email = new Email(
            id: "exorcism_001",
            subject: "Church of the Blessed Virgin Mary-Exorcist Wiktor Skrzat",
            sender: "Main Character",
            preview: "Blessings, I am writing to you because...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
    }

    void CreateMaryThread()
    {
        var convo = new List<Message> {

    // 1 Mary
    new Message("Mary Pak",
    "Hey,\n\nYour parents told me that you left the facility some time ago. I spent a long time wondering whether I should write to you, but I guess I just wanted to know how you’ve been after all these years.\n\nIs it better now? Have the voices you used to hear finally stopped?\n\nI was also thinking maybe we could meet up sometime and talk. It would honestly be really nice to see you again — even just for a little while, like back in our school days.\n\nTake care,\nMary",
    isPlayer: false, timestamp: ""),

    // 2 Jack
    new Message("Jack White",
    "Hey,\n\nThanks for writing. Yeah, things are much better now. They still recommend medication, but honestly, I barely take it anymore. The pills make me feel numb — all they do is make me sleepy and disconnected. I only take them when things get really bad.\n\nI’d really like to meet up. How about Thursday at 4 PM?",
    isPlayer: true, timestamp: ""),

    // 3 Mary
    new Message("Mary Pak",
    "Thursday sounds perfect.\n\nMaybe we could meet in front of our old elementary school? I haven’t been there in years, but it feels like a good place to start.\n\nSee you soon :)",
    isPlayer: false, timestamp: ""),

    // 4 Jack
    new Message("Jack White",
    "Thanks for today.\n\nIt was really good seeing you again after all these years. I think I forgot what it feels like to be normal around someone familiar.\n\nHopefully we can do it again sometime.",
    isPlayer: true, timestamp: ""),

    // 5 Mary
    new Message("Mary Pak",
    "Thank you too.\n\nHonestly, I was a little worried before meeting you, but I’m really glad you seem calmer now. It felt good talking like we used to.\n\nMaybe we could start meeting regularly? Every Thursday?",
    isPlayer: false, timestamp: ""),

    // 6 Jack
    new Message("Jack White",
    "That sounds great.\n\nIt’s been a long time since I had something to actually look forward to.\n\nSee you Thursday.",
    isPlayer: true, timestamp: ""),

    // 7 Mary
    new Message("Mary Pak",
    "I’m already looking forward to our next meeting.\n\nMaybe we could go to the movies this time? There’s a new horror film called \"Braindead\" playing in theaters right now. It looks kind of like those old VHS horror movies — I thought you might enjoy it.\n\nLet me know :)",
    isPlayer: false, timestamp: ""),

    // 8 Mary
    new Message("Mary Pak",
    "Hey,\n\nYou still haven’t replied, and we’re supposed to see each other today. Worst case, we can just decide where to go once we meet.\n\nI’ll buy the tickets before the movie starts.",
    isPlayer: false, timestamp: ""),

    // 9 Mary
    new Message("Mary Pak",
    "Hey...\n\nIs everything okay? You didn’t show up in front of the school today. I started getting worried.\n\nDoes this have something to do with your condition?",
    isPlayer: false, timestamp: ""),

    // 10 Demon/Jack
    new Message("Jack White",
    "I already told you I’m fine.\n\nI just didn’t want to see you.\n\nHe finally explained why you came back after all these years. It was never about me. You want to lock me away again. You watch me, ask about my medication, try to make me weak and quiet again. Just like they did.\n\nBut this time you won’t succeed.\n\nYou won’t separate us.",
    isPlayer: true, timestamp: ""),

    // 11 Mary
    new Message("Mary Pak",
    "None of what you’re saying is true.\n\nNobody wants to hurt you. I’m only trying to help. Please calm down and write back.\n\nI can come to your apartment within the hour. I still know one of the doctors from the ward you stayed in before. We can figure this out together before things get worse.\n\nPlease don’t stay alone right now.",
    isPlayer: false, timestamp: ""),

    // 12 Demon/Jack
    new Message("Jack White",
    "Stay out of my life.\n\nWe don’t need your help.\n\nNever contact me again.",
    isPlayer: true, timestamp: ""),

    // 13 System
    new Message("System",
    "<b>[CONTACT BLOCKED]</b>",
    isPlayer: false, timestamp: "SYSTEM")
    };

        var email = new Email(
            id: "mary_001",
            subject: "Mary Pak-Childhood Friend",
            sender: "Mary Pak",
            preview: "Your parents told me that you left the facility...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
    }

    void CreateDoctorThread()
    {
        var convo = new List<Message> {

    // 1 Doctor
    new Message("Dr. Barnaba Smith",
    "Dear Mr. White,\n\nDespite your discharge from the psychiatric facility, you remain under my outpatient care. Your prescription has been renewed for the next six months.\n\nI strongly advise you to continue taking your medication regularly and to attend follow-up appointments at my office. Please also contact me by email or phone should any symptoms return or if your condition worsens.\n\nI remain at your disposal,\nDr. Barnaba Smith",
    isPlayer: false, timestamp: ""),

    // 2 Jack
    new Message("Jack White",
    "Good afternoon, Doctor,\n\nAs I mentioned during my stay at the facility, I no longer believe I need the medication. I feel fine, and I haven’t experienced any symptoms for quite some time.\n\nThank you for your care and concern, but I most likely won’t continue the appointments.\n\nRegards,\nJack White",
    isPlayer: true, timestamp: ""),

    // 3 Doctor
    new Message("Dr. Barnaba Smith",
    "I understand your reluctance.\n\nHowever, please remember that this conviction of being “fully recovered” has appeared before during your treatment. I still encourage you to maintain at least occasional contact.\n\nShould anything happen, I am available to help.\n\nSincerely,\nDr. Barnaba Smith",
    isPlayer: false, timestamp: ""),

    // 4 Jack
    new Message("Jack White",
    "Doctor,\n\nI need a new prescription.\n\nHe came back.\n\nHe won’t let me take the pills. He threw away the entire bottle I kept in the bathroom. I can hear him again. He’s getting closer.\n\nPlease respond as soon as possible.",
    isPlayer: true, timestamp: ""),

    // 5 Doctor
    new Message("Dr. Barnaba Smith",
    "Mr. White,\n\nI am very concerned about the return of the symptoms you describe.\n\nI have attached a new prescription to this email, but I strongly urge you to schedule an appointment as soon as possible. Medication alone may not be sufficient at this stage.\n\nWe can help you, but I need your cooperation.\n\nKind regards,\nDr. Barnaba Smith",
    isPlayer: false, timestamp: ""),

    // 6 Jack
    new Message("Jack White",
    "I can’t come.\n\nHe knows I’m writing to you. He’s standing behind me right now. I can see him in the reflection of the monitor.\n\nHe doesn’t like this.\n\nIt’s getting harder to think. Sometimes I lose hours and can’t remember what I did. I can feel him taking control again.\n\nPlease help me.",
    isPlayer: true, timestamp: ""),

    // 7 Doctor
    new Message("Dr. Barnaba Smith",
    "Mr. White,\n\nI believe you may currently be a danger to yourself.\n\nIf you give your consent, I can immediately arrange for an ambulance and have you admitted for psychiatric evaluation. Please respond as quickly as possible.\n\nYou should not remain alone right now.\n\nDr. Barnaba Smith",
    isPlayer: false, timestamp: ""),

    // 8 Doctor
    new Message("Dr. Barnaba Smith",
    "Hello,\n\nI am writing again due to the lack of response from you. Please let me know if you are safe and whether you have made a decision regarding medical assistance.\n\nI would appreciate an urgent reply.\n\nSincerely,\nDr. Barnaba Smith",
    isPlayer: false, timestamp: ""),

    // 9 Demon/Jack
    new Message("Jack White",
    "Doctor,\n\neverything is fine. As I said before, I have fully recovered and there is no need to call an ambulance.\n\nI would also like to end our correspondence from this point onward. Contact with you has a negative effect on me and, overall, is harmful to both of us.\n\nOr rather —\n\nharmful to US.",
    isPlayer: true, timestamp: "")
    };

        var email = new Email(
            id: "doctor_001",
            subject: "Dr. Barnaba Smith-Psychiatrist",
            sender: "Dr. Barnaba Smith",
            preview: "Despite your discharge from the psychiatric facility...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
    }

    void CreateIoculatorThread()
    {
        var convo = new List<Message> {

    new Message("Ioculator",
    "Hello,\n\nAs part of an ongoing project, we are looking for a Level Designer / Level Artist.\n\nWe are searching for someone who understands space not as a backdrop, but as a system of user behavior — something that guides, restricts, opens up, and shapes experience over time.\n\nResponsibilities:\n\n• designing level layouts and interactive environments\n• building exploration flow and clear spatial logic\n• creating environments based on rhythm and spatial tension\n• collaborating with art and design teams\n• testing and iterating user experience within the space\n\nRequirements:\n\n• strong sense of spatial composition and structure\n• systems thinking (space as rules, not just form)\n• creativity in designing experiences\n• portfolio (including prototypes and conceptual work is welcome)\n\nWe are not looking for a “map drawer,” but for someone who can make space function like a well-designed experience system.\n\nIf you feel you think in this way — get in touch. I would be glad to see your work.\n\nBest regards,\nIoculator\nRecruitment Department",
    isPlayer: false, timestamp: "")
    };

        var email = new Email(
            id: "ioculator_001",
            subject: "Level Designer / Level Artist Recruitment",
            sender: "Ioculator",
            preview: "As part of an ongoing project, we are looking for...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
    }

    void CreateFishingThread()
    {
        var convo = new List<Message> {

    new Message("Inspector Tedi Tezykov",
    "Dear Citizen,\n\nThis is an official reminder that all recreational fishing activities carried out within registered freshwater zones require a valid fishing permit approved by the National Fishing Association.\n\nOver the past few weeks, our inspectors have reported multiple violations involving:\n\n• fishing without documentation\n• exceeding catch limits\n• use of unauthorized bait\n• improper fish storage procedures\n\nAs part of our ongoing “Clean Waters Initiative,” random inspections may be conducted without prior notice.\n\nPlease remember to keep the following items with you during every fishing trip:\n\n• valid fishing permit\n• personal identification\n• catch documentation card\n• approved equipment only\n\nFailure to comply with regulations may result in financial penalties, equipment confiscation, or temporary suspension of fishing privileges.\n\nIf you have recently renewed your permit, please ignore this message.\n\nRespectfully,\nInspector Tedi Tezykov\nNational Fishing Association\n“Protecting Waters Since 1974”",
    isPlayer: false, timestamp: "")
    };

        var email = new Email(
            id: "fishing_001",
            subject: "National Fishing Association – Permit Reminder",
            sender: "Inspector Tedi Tezykov",
            preview: "This is an official reminder that all recreational fishing...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
    }

    void CreatePyzotekaThread()
    {
        var convo = new List<Message> {

    new Message("Pyzoteka Family Restaurant",
    "Hello!\n\nTired of boring meals?\nThen it’s time to try REAL homemade Pyza!\n\nThis weekend, we invite you to PYZOTEKA for our special Pyza Festival featuring every kind of Pyza you can imagine:\n\n• Meat Pyza\n• Mushroom Pyza\n• Potato Pyza\n• Sweet Pyza\n• PYZA XXL – only for the bravest customers!\n\nEvery meal comes with a free homemade fruit compote and a 15% discount on your second serving.\n\n14 Sunny Street\nOpen daily from 10:00 AM to 10:00 PM\n\nPYZOTEKA — because life is too short for small Pyza.\n\nSee you soon!",
    isPlayer: false, timestamp: "")
    };

        var email = new Email(
            id: "pyzoteka_001",
            subject: "PYZOTEKA – Pyza Festival This Weekend",
            sender: "Pyzoteka Family Restaurant",
            preview: "Tired of boring meals? Then it’s time to try REAL homemade Pyza...",
            messages: convo,
            isRead: false
        );

        AddEmailToInbox(email);
    }

    void CreateKamerkiThread()
    {
        var convo = new List<Message> {

    new Message("KamerkiFlajszaja Notifications",
    "Hello,\n\nNew spicy live streams are now available in your area.\n\nOn KamerkiFlajszaja.com you can watch the hottest real-time shows from various nearby sources. The platform provides continuous access to live content that updates automatically based on current availability.\n\nTo view streams, simply log in and browse the active feed list. New content is added regularly as new sources become available.\n\nYou can also purchase platform tokens, which are required for extended access and unlocking additional streams.\n\nUse the code FLAJSZAJ20 to receive a 20% discount on your first token purchase.\n\nKamerkiFlajszaja.com\nHOTTEST LIVE STREAMS ANYTIME",
    isPlayer: false, timestamp: "")
    };

        var email = new Email(
            id: "kamerki_001",
            subject: "KamerkiFlajszaja – New Live Streams Available",
            sender: "KamerkiFlajszaja Notifications",
            preview: "New spicy live streams are now available in your area...",
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