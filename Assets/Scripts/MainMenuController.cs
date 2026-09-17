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
        saveNameButton.onClick.AddListener(SaveNameAndContinue);
        changeNameButton.onClick.AddListener(ShowNamePanel);
        playButton.onClick.AddListener(OnPlayClicked);
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
