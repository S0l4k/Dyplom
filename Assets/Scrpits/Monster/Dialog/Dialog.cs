using System.Collections;
using UnityEngine;
using TMPro;
using FMODUnity;
using FMOD.Studio;

public class Dialog : MonoBehaviour
{
    [Header("Dialog Data")]
    public DialogNode[] dialogNodes;

    [Header("UI Elements")]
    public GameObject[] answers;
    public TextMeshProUGUI dialogText;
    public GameObject dialogueBG;

    [Header("Marker Settings")]
    public string npcMarkerColor = "#FFFFFF33";
    public string playerMarkerColor = "#FFFFFF33";
    public float typeSpeed = 0.08f;
    public float delayBetweenAnswers = 0.3f;

    [Header("FMOD")]
    public EventReference npcVoiceEvent;   // ✅ NOWY SPOSÓB
    private EventInstance npcVoiceInstance;

    private int currentNode = 0;
    private bool optionsActive = false;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool skipTyping = false;

    void Awake()
    {
        HideAll();
    }

    void Update()
    {
        if (isTyping && Input.GetKeyDown(KeyCode.Space))
            skipTyping = true;

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
        dialogueBG.SetActive(true);
    }

    void ShowNode()
    {
        HideAll();
        dialogueBG.SetActive(true);

        dialogText.gameObject.SetActive(true);
        typingCoroutine = StartCoroutine(FullDialogSequence(dialogNodes[currentNode]));
    }

    IEnumerator FullDialogSequence(DialogNode node)
    {
        isTyping = true;
        skipTyping = false;

        // 🔊 START "UNDERTALE VOICE"
        npcVoiceInstance = RuntimeManager.CreateInstance(npcVoiceEvent);
        npcVoiceInstance.start();

        // NPC line
        yield return StartCoroutine(TypeTextWithMarker(dialogText, node.npcLine, npcMarkerColor));

        // 🔇 STOP VOICE
        npcVoiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        npcVoiceInstance.release();

        yield return new WaitForSeconds(0.4f);

        // Player answers
        for (int i = 0; i < node.responses.Length && i < answers.Length; i++)
        {
            TMP_Text answerTMP = answers[i].GetComponent<TMP_Text>();
            answers[i].SetActive(true);

            yield return StartCoroutine(TypeTextWithMarker(
                answerTMP,
                node.responses[i],
                playerMarkerColor
            ));

            yield return new WaitForSeconds(delayBetweenAnswers);
        }

        isTyping = false;
        optionsActive = true;
    }

    IEnumerator TypeTextWithMarker(TMP_Text textObj, string text, string markerColor)
    {
        string openTag = $"<mark={markerColor}>";
        string closeTag = "</mark>";

        textObj.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            if (skipTyping)
            {
                textObj.text = openTag + text + closeTag;
                yield break;
            }

            string visible = text.Substring(0, i + 1);
            textObj.text = openTag + visible + closeTag;

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

    void EndDialog()
    {
        HideAll();
        gameObject.SetActive(false);
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
        dialogueBG.SetActive(false);
    }
}
