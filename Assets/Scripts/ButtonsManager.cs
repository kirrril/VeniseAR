using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private ARSession arSession;
    [SerializeField] private GameObject downloadPopUp;
    [SerializeField] private GameObject downloadText;
    [SerializeField] private GameObject dotsGO;
    [SerializeField] private Dots dots;
    [SerializeField] private Button confirmDownloadButton;
    [SerializeField] private Button cancelDownloadButton;

    void OnEnable()
    {
        if (dots != null) dots.DotsCompleted += OnDotsCompleted;
    }
    void OnDisable()
    {
        if (dots != null) dots.DotsCompleted -= OnDotsCompleted;
    }
    public void OnCuratorClick()
    {
        SceneManager.LoadScene("CuratorScene");
    }

    public void OnDownloadTargetsClick()
    {
        downloadPopUp.SetActive(true);
    }

    public void OnCancelDownloadClick()
    {
        downloadPopUp.SetActive(false);
    }

    public void OnConfirmDownloadClick()
    {
        downloadText.SetActive(false);
        dotsGO.SetActive(true);
        confirmDownloadButton.interactable = false;
        cancelDownloadButton.interactable = false;
    }

    private void OnDotsCompleted()
    {
        dotsGO.SetActive(false);
        downloadText.SetActive(true);
        confirmDownloadButton.interactable = true;
        cancelDownloadButton.interactable = true;
        downloadPopUp.SetActive(false);
    }

    public void OnStartARExperienceClick()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void OnBackToMainClick()
    {
        if (SceneManager.GetActiveScene().name == "ARScene" && arSession != null)
        {
            arSession.Reset();
        }
        SceneManager.LoadScene("EntryScene");
    }
}
