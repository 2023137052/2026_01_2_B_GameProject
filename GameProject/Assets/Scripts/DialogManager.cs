using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance {  get; private set; }

    [Header("Dialog References")]
    [SerializeField] private DialogDatabaseSO dialogDatabase;

    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel;


    [SerializeField] private Image portraitImage;

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button NextButton;

    [Header("Dialog Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool useTypewriterEffect = true;

    private bool isTyping = false;
    private Coroutine typingCoroutinr;

    private DialogSO currentDialog;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if(dialogDatabase != null)
        {
            dialogDatabase.Initailize();
        }
        else
        {
            Debug.LogError("Dialog Database is not assinged to Dialog Manager");
        }

        if (NextButton != null)
        {
            NextButton.onClick.AddListener(NextDialog);
        }
        else
        {
            Debug.LogError("Next Button is Not assigned");
        }
    }
    public void StartDialog(int dialogId)
    {
        DialogSO dialog = dialogDatabase.GetDialodByld(dialogId);
        if(dialog != null)
        {
            StartDialog(dialog);
        }
        else
        {
            Debug.LogError($"Dialog with ID {dialogId} not found!");
        }
    }
    public void StartDialog(DialogSO dialog)
    {
        if (dialog == null) return;

        currentDialog = dialog;
        ShowDialog();
        dialogPanel.SetActive(true);
    }

    public void ShowDialog()
    {
        Debug.Log(currentDialog.portraitPath);

        if (currentDialog  == null) return ;
        characterNameText.text = currentDialog.characterName;

        if (useTypewriterEffect)
        {
            StartTypingEffect(currentDialog.text);
        }
        else
        {
            dialogtext.text = currentDialog.text;
        }


        if (currentDialog.portrait != null)
        {
            portraitImage.sprite = currentDialog.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else if (!string.IsNullOrEmpty(currentDialog.portraitPath))
        {
            Sorite portrait = Resources.Load<Sprite>(currentDialog.portraitPath);
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Portrait not found at path : {currentDialog.portraitPath}");
                portraitImage.gameObject.SetActive(false);
            }
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    public void CloseDialog()
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
        StopTypingEffect();
    }

    public void NextDialog()
    {
        if(isTyping)
        {
            StopTypingEffect();
            dialogText.text = currentDialog.text;
            isTyping = false;
            return;
        }


        if (currentDialog != null && currentDialog.nextId > 0)
        {
            DialogSO nextDialog = dialogDatabase.GetDialodByld(currentDialog.nextId);
            if (nextDialog != null)
            {
                currentDialog = nextDialog;
                ShowDialog();
            }
        }
        else
        {
            CloseDialog();
        }
    }
    void Start()
    {
        CloseDialog() ;
        StartDialog(1);
    }
    private IEnumerator TypeText(string text)
    {
        dialogText.text = "";
        foreach(char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }
    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
    private void StartTypingEffect(string text)
    {
        isTyping = true;
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }
}
