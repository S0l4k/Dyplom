using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MailboxUI : MonoBehaviour
{
    [SerializeField] private Transform listContainer;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject emailButtonPrefab;
    [SerializeField] private ScrollRect rightScroll; // przeci¹gnij ScrollRect z prawego panelu

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
        // Czyœæ star¹ listê
        foreach (Transform t in listContainer) Destroy(t.gameObject);

        for (int i = 0; i < currentEmails.Count; i++)
        {
            var btnObj = Instantiate(emailButtonPrefab, listContainer);
            var btn = btnObj.GetComponent<Button>();
            var label = btnObj.GetComponentInChildren<TMP_Text>();

            label.text = currentEmails[i].isRead
                ? currentEmails[i].preview
                : $"<b>{currentEmails[i].preview}</b>"; // pogrubienie nieprzeczytanych

            int index = i;
            btn.onClick.AddListener(() => SelectEmail(index));
        }
    }

    private void SelectEmail(int index)
    {
        if (index < 0 || index >= currentEmails.Count) return;
        activeIndex = index;
        contentText.text = currentEmails[index].body;
        currentEmails[index].isRead = true;

        // Scroll na górê
        rightScroll.verticalScrollbar.value = 1f;

        // Odœwie¿ kolory przycisków
        BuildList();
    }
}