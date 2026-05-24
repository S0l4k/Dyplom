using System.Collections;
using UnityEngine;
using TMPro;
using FMODUnity;
using Studio = FMOD.Studio;

public class Dialog : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject[] answers;
    public TextMeshProUGUI dialogText;
    public GameObject dialogueBG;

    [Header("Marker Settings")]

    public float typeSpeed = 0.08f;
    public float delayBetweenAnswers = 0.3f;

    [Header("FMOD")]
    public EventReference npcVoiceEvent;
    private Studio.EventInstance npcVoiceInstance;

    private DialogNode[] currentNodes;
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

        if (Input.GetKeyDown(KeyCode.Alpha2) && answers.Length > 1)
            SelectOption(1);
    }

    public void StartDialog(DialogNode[] nodes)
    {
        currentNodes = nodes;
        currentNode = 0;
        ShowNode();
        dialogueBG.SetActive(true);
        gameObject.SetActive(true);
    }

    void ShowNode()
    {
        HideAll();
        dialogueBG.SetActive(true);

        if (dialogText != null)
            dialogText.gameObject.SetActive(true);

        typingCoroutine = StartCoroutine(FullDialogSequence(currentNodes[currentNode]));
    }

    IEnumerator FullDialogSequence(DialogNode node)
    {
        isTyping = true;
        skipTyping = false;

        // ✅ ZAMIENIONE: RuntimeManager -> AudioManager
        npcVoiceInstance = AudioManager.Instance.PlayDialogVoice(npcVoiceEvent);

        yield return StartCoroutine(TypeText(dialogText, node.npcLine));

        // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
        AudioManager.Instance.StopDialogVoice(ref npcVoiceInstance, true);

        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < node.responses.Length && i < answers.Length; i++)
        {
            if (answers[i] == null) continue;

            TMP_Text answerTMP = answers[i].GetComponent<TMP_Text>();
            if (answerTMP == null) continue;

            answers[i].SetActive(true);
            yield return StartCoroutine(TypeText(answerTMP, node.responses[i]));
            yield return new WaitForSeconds(delayBetweenAnswers);
        }

        isTyping = false;
        optionsActive = true;
    }

    IEnumerator TypeText(TMP_Text textObj, string text)
    {
        if (textObj == null) yield break;

        textObj.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            if (skipTyping)
            {
                textObj.text = text;
                yield break;
            }

            textObj.text = text.Substring(0, i + 1);
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void SelectOption(int optionIndex)
    {
        if (!optionsActive || currentNodes == null) return;

        DialogNode node = currentNodes[currentNode];

        if (node.responseEvents != null &&
            optionIndex < node.responseEvents.Length &&
            node.responseEvents[optionIndex] != null)
        {
            node.responseEvents[optionIndex].Invoke();
        }

        if (node.nextNodeIndex.Length <= optionIndex || node.nextNodeIndex[optionIndex] < 0)
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

        if (dialogText != null)
            dialogText.gameObject.SetActive(false);

        if (answers != null)
        {
            foreach (var answer in answers)
            {
                if (answer != null)
                    answer.SetActive(false);
            }
        }

        optionsActive = false;
        isTyping = false;
        skipTyping = false;
        dialogueBG.SetActive(false);
    }

    void OnDestroy()
    {
        // ✅ ZAMIENIONE: bezpośrednie stop/release -> AudioManager
        AudioManager.Instance.StopDialogVoice(ref npcVoiceInstance, true);
    }
}