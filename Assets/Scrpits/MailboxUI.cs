using System.Collections;
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
            // === KOLOR I ALIGN ===
            string colorHex = ColorUtility.ToHtmlStringRGB(
                msg.isPlayer ? playerColor :
                msg.sender == "System" || msg.sender.Contains("AUTOMATED") ? systemColor : npcColor);

            string align = msg.isPlayer ? playerAlign : "left";

            // === NAGŁÓWEK: Nadawca + czas ===
            sb.Append($"<align={align}><color=#{colorHex}><b>{msg.sender}</b></color>");
            if (!string.IsNullOrEmpty(msg.timestamp))
            {
                sb.Append($" <color=#666><size=80%>[{msg.timestamp}]</size></color>");
            }
            sb.Append("</align>\n");

            // === ODRĘBNIK GÓRNY ===
            sb.Append("<color=#333333>━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");

            // === TREŚĆ: zamiana \r\n na <br> + lepsze line-height ===
            string formattedText = msg.text
                .Replace("\r\n", "<br>")  // Windows line breaks
                .Replace("\n", "<br>")    // Unix line breaks
                .Replace("\r", "<br>");   // Mac line breaks

            // Dodaj odstęp między akapitami (podwójne <br>)
            formattedText = formattedText.Replace("<br><br>", "<br><br><br>");

            sb.Append($"<align={align}><line-height=1.4em>{formattedText}</line-height></align>");

            // === ODRĘBNIK DOLNY + DUŻY ODSTĘP ===
            sb.Append("\n<color=#333333>━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            sb.Append("\n<size=20></size>\n\n");  // ✅ DUŻY odstęp między wiadomościami
        }

        // === SPECJALNE STYLE DLA SYSTEM ===
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

        // ✅ 1. Ustaw treść maila
        contentText.text = FormatConversation(email.messages);

        email.isRead = true;
        BuildList();

        // ✅ 2. NATYCHMIASTOWA przebudowa layoutu – KLUCZOWE!
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.rectTransform);

        // ✅ 3. Scroll na górę – użyj normalizedPosition (nie scrollbar.value!)
        if (rightScroll != null)
        {
            // ✅ normalizedPosition: (x, y) gdzie y=1 to GÓRA
            rightScroll.normalizedPosition = new Vector2(0, 1);
            Debug.Log($"[MailboxUI] 📜 Scrolled to top | normalizedPosition: {rightScroll.normalizedPosition}");
        }
    }

}