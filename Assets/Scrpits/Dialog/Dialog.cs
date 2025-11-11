using System.Collections;
using UnityEngine;
using TMPro;

public class Dialog : MonoBehaviour
{
    [Header("Dialog Data")]
    public DialogNode[] dialogNodes;

    [Header("UI Elements")]
    public GameObject[] answers;
    public TextMeshProUGUI dialogText;

    private int currentNode = 0;
    private bool optionsActive = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool skipTyping = false;

    [Header("Marker Settings")]
    [Tooltip("Kolor markera (RGBA w HEX) np. #00000080 dla czarnego półprzezroczystego")]
    public string markerColor = "#00000080"; // ✅ czarny marker (poprawiony - jedno #)
    public float typeSpeed = 0.08f;
    public float delayBetweenAnswers = 0.3f;

    [Header("Debug")]
    public bool happy = false;

    void Awake()
    {
        HideAll();
    }

    void Update()
    {
        // Pomijanie animacji pisania
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            skipTyping = true;
        }

        // Wybór opcji
        if (!optionsActive) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectOption(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectOption(1);
    }

    public void StartDialog()
    {
        currentNode = 0;
        ShowNode();
    }

    void ShowNode()
    {
        HideAll();

        DialogNode node = dialogNodes[currentNode];
        dialogText.gameObject.SetActive(true);

        // Uruchamiamy proces dialogu — NPC + odpowiedzi po kolei
        typingCoroutine = StartCoroutine(FullDialogSequence(node));
    }

    IEnumerator FullDialogSequence(DialogNode node)
    {
        isTyping = true;
        skipTyping = false;

        // 1️⃣ — Tekst NPC-a
        dialogText.text = "";
        for (int i = 0; i < node.npcLine.Length; i++)
        {
            if (skipTyping)
            {
                dialogText.text = node.npcLine;
                break;
            }

            dialogText.text = node.npcLine.Substring(0, i + 1);
            yield return new WaitForSeconds(typeSpeed);
        }

        // Poczekaj chwilę po zakończeniu tekstu NPC
        yield return new WaitForSeconds(0.4f);

        // 2️⃣ — Odpowiedzi (po kolei, z efektem markera)
        for (int i = 0; i < node.responses.Length && i < answers.Length; i++)
        {
            yield return StartCoroutine(TypeAnswerText(answers[i], node.responses[i]));
            yield return new WaitForSeconds(delayBetweenAnswers);
        }

        isTyping = false;
        optionsActive = true;
    }

    IEnumerator TypeAnswerText(GameObject answerObj, string text)
    {
        answerObj.SetActive(true);
        TMP_Text answerText = answerObj.GetComponent<TMP_Text>();
        answerText.text = "";

        // ✅ Używamy twardo wpisanego czarnego koloru (TMP czasem ignoruje zmienne)
        string openTag = "<mark=#00000080>";
        string closeTag = "</mark>";

        for (int i = 0; i < text.Length; i++)
        {
            if (skipTyping)
            {
                answerText.text = openTag + text + closeTag;
                yield break;
            }

            string visible = text.Substring(0, i + 1);
            answerText.text = openTag + visible + closeTag;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void SelectOption(int optionIndex)
    {
        if (!optionsActive) return;

        DialogNode node = dialogNodes[currentNode];

        if (node.responseEvents != null &&
            optionIndex < node.responseEvents.Length &&
            node.responseEvents[optionIndex] != null)
        {
            node.responseEvents[optionIndex].Invoke();
        }

        if (node.nextNodeIndex.Length <= optionIndex ||
            node.nextNodeIndex[optionIndex] < 0)
        {
            EndDialog();
            return;
        }

        currentNode = node.nextNodeIndex[optionIndex];
        ShowNode();
    }

    public void Happiness()
    {
        happy = true;
        Debug.Log("Krzysiu jest zadowolony!");
    }

    void EndDialog()
    {
        HideAll();
        Debug.Log("Dialog zakończony");
    }

    void HideAll()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogText.gameObject.SetActive(false);

        foreach (var answer in answers)
            answer.SetActive(false);

        optionsActive = false;
        isTyping = false;
        skipTyping = false;
    }
}