using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    private const string PlayerNameKey = "mathcore_playerName";

    [Header("Panels")]
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject menuPanel;

    [Header("Name Panel")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text nameError;
    [SerializeField] private Button saveNameButton;

    [Header("Menu Panel")]
    [SerializeField] private TMP_Text greetingText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button changeNameButton;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            return;
        }

        saveNameButton.onClick.AddListener(SaveNameAndContinue);
        changeNameButton.onClick.AddListener(ShowNamePanel);
        playButton.onClick.AddListener(OnPlayClicked);
    }

    private bool ValidateReferences()
    {
        bool isValid = true;
        isValid &= LogIfMissing(namePanel, nameof(namePanel));
        isValid &= LogIfMissing(menuPanel, nameof(menuPanel));
        isValid &= LogIfMissing(nameInput, nameof(nameInput));
        isValid &= LogIfMissing(nameError, nameof(nameError));
        isValid &= LogIfMissing(saveNameButton, nameof(saveNameButton));
        isValid &= LogIfMissing(greetingText, nameof(greetingText));
        isValid &= LogIfMissing(playButton, nameof(playButton));
        isValid &= LogIfMissing(changeNameButton, nameof(changeNameButton));
        return isValid;
    }

    private bool LogIfMissing(Object reference, string fieldName)
    {
        if (reference != null)
        {
            return true;
        }

        Debug.LogError($"MainMenuController: falta asignar '{fieldName}' en el Inspector. " +
            "Vuelve a correr Math Core > Construir Pantalla de Menu, o arrastra el objeto correcto al campo.", this);
        return false;
    }

    private void Start()
    {
        string savedName = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
        if (string.IsNullOrWhiteSpace(savedName))
        {
            ShowNamePanel();
        }
        else
        {
            ShowMenuPanel(savedName);
        }
    }

    private void ShowNamePanel()
    {
        nameInput.text = PlayerPrefs.GetString(PlayerNameKey, string.Empty);
        nameError.text = string.Empty;
        namePanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    private void ShowMenuPanel(string playerName)
    {
        greetingText.text = $"Jugador: {playerName}";
        namePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void SaveNameAndContinue()
    {
        string trimmedName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(trimmedName))
        {
            nameError.text = "Escribe tu nombre para continuar";
            return;
        }

        PlayerPrefs.SetString(PlayerNameKey, trimmedName);
        PlayerPrefs.Save();
        ShowMenuPanel(trimmedName);
    }

    private void OnPlayClicked()
    {
        const string sceneName = "MapSelect";
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"La escena '{sceneName}' todavía no existe.");
        }
    }
}
