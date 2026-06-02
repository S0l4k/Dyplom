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
    [SerializeField] private ScrollRect rightScroll; 

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
    [SerializeField] private Color playerColor = new Color(0.4f, 0.8f, 1f);   
    [SerializeField] private Color npcColor = new Color(0.9f, 0.9f, 0.9f);    
    [SerializeField] private Color systemColor = new Color(0.7f, 0.7f, 0.7f); 
    [SerializeField] private string playerAlign = "right";
    [SerializeField] private string npcAlign = "left";
    public string FormatConversation(List<Message> messages)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var msg in messages)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(
                msg.isPlayer ? playerColor :
                msg.sender == "System" || msg.sender.Contains("AUTOMATED") ? systemColor : npcColor);

            string align = msg.isPlayer ? playerAlign : "left";

            sb.Append($"<align={align}><color=#{colorHex}><b>{msg.sender}</b></color>");
            if (!string.IsNullOrEmpty(msg.timestamp))
            {
                sb.Append($" <color=#666><size=80%>[{msg.timestamp}]</size></color>");
            }
            sb.Append("</align>\n");

            sb.Append("<color=#333333>━━━━━━━━━━━━━━━━━━━━━━━━</color>\n");
            string formattedText = msg.text
                .Replace("\r\n", "<br>")  
                .Replace("\n", "<br>")   
                .Replace("\r", "<br>");   

            formattedText = formattedText.Replace("<br><br>", "<br><br><br>");

            sb.Append($"<align={align}><line-height=1.4em>{formattedText}</line-height></align>");

            sb.Append("\n<color=#333333>━━━━━━━━━━━━━━━━━━━━━━━━</color>");
            sb.Append("\n<size=20></size>\n\n");  
        }

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

        contentText.text = FormatConversation(email.messages);

        email.isRead = true;
        BuildList();

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.rectTransform);

        if (rightScroll != null)
        {
            rightScroll.normalizedPosition = new Vector2(0, 1);
        }
    }

}