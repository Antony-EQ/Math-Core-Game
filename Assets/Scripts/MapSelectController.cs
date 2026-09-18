using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSelectController : MonoBehaviour
{
    private const string PlayerNameKey = "mathcore_playerName";

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button greeceButton;

    private void Awake()
    {
        greeceButton.onClick.AddListener(OnGreeceClicked);
    }

    private void Start()
    {
        string playerName = PlayerPrefs.GetString(PlayerNameKey, "JUGADOR");
        titleText.text = $"MAPAS";
    }

    private void OnGreeceClicked()
    {
        const string sceneName = "GreeceLevel";
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
