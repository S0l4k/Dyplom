using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailboxUI : MonoBehaviour
{
    [SerializeField] private Transform listContainer;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject emailButtonPrefab;
    [SerializeField] private ScrollRect rightScroll; // przeciągnij ScrollRect z prawego panelu

    private List<Email> currentEmails;
    private int activeIndex = -1;

    public void Init(List<Email> emails)
    {
        currentEmails = emails;
        BuildList();
    }

    public void RefreshList(List<Email> emails)
    {
        currentEmails = emails;
        BuildList();
    }

    private void BuildList()
    {
        // Czyść starą listę
        foreach (Transform t in listContainer) Destroy(t.gameObject);

        for (int i = 0; i < currentEmails.Count; i++)
        {
            var btnObj = Instantiate(emailButtonPrefab, listContainer);
            var btn = btnObj.GetComponent<Button>();
            var label = btnObj.GetComponentInChildren<TMP_Text>();

            label.text = currentEmails[i].isRead
            ? currentEmails[i].subject
            : $"<b>{currentEmails[i].subject}</b>";

            int index = i;
            btn.onClick.AddListener(() => SelectEmail(index));
        }
    }
    [Header("Styl konwersacji")]
    [SerializeField] private Color playerColor = new Color(0.4f, 0.8f, 1f);   // jasny niebieski
    [SerializeField] private Color npcColor = new Color(0.9f, 0.9f, 0.9f);    // jasny szary
    [SerializeField] private Color systemColor = new Color(0.7f, 0.7f, 0.7f); // szary
    [SerializeField] private string playerAlign = "right";
    [SerializeField] private string npcAlign = "left";

    // 🔹 GŁÓWNA METODA: zamienia konwersację na TMP-compatible string
    public string FormatConversation(List<Message> messages)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var msg in messages)
        {
            // Kolor i align w zależności od nadawcy
            string colorHex = ColorUtility.ToHtmlStringRGB(
                msg.isPlayer ? playerColor :
                msg.sender == "System" || msg.sender.Contains("AUTOMATED") ? systemColor : npcColor);

            string align = msg.isPlayer ? playerAlign : "left";

            // Nagłówek wiadomości (nadawca + czas)
            if (!string.IsNullOrEmpty(msg.sender))
            {
                sb.Append($"<align={align}><color=#{colorHex}><b>{msg.sender}</b></color>");
                if (!string.IsNullOrEmpty(msg.timestamp))
                {
                    sb.Append($" <color=#666><size=80%>[{msg.timestamp}]</size></color>");
                }
                sb.Append("</align>\n");
            }

            // Treść wiadomości
            sb.Append($"<align={align}>{msg.text}</align>\n");

            // większy odstęp między wiadomościami
            sb.Append("<size=8>\n</size>");
            sb.Append("<color=#444444>────────────────────────</color>\n\n");
        }

        // Specjalny styl dla systemowych powiadomień (np. Delivery Failure)
        string result = sb.ToString();
        result = result.Replace("[AUTOMATED]", "<color=#666><i>[AUTOMATED]</i></color>");
        result = result.Replace("AUTOMATED DELIVERY FAILURE NOTICE",
            "<color=#aa4444><b>⚠️ AUTOMATED DELIVERY FAILURE NOTICE</b></color>");

        return result;
    }

    private void SelectEmail(int index)
    {
        if (index < 0 || index >= currentEmails.Count) return;

        var email = currentEmails[index];
        contentText.text = FormatConversation(email.messages); // 🔹 Tu zmieniamy!

        email.isRead = true;
        rightScroll.verticalScrollbar.value = 1f; // scroll na górę
        BuildList(); // odśwież kolory na liście
    }

 
}