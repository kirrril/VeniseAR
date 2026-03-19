using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private ARSession arSession;
    public void OnOnlineGalleryClick()
    {
        Application.OpenURL("https://venise.kirrril.com");
    }

    public void OnDownloadTargetsClick()
    {
        SceneManager.LoadScene("TargetScene");
    }

    public void OnStartARExperienceClick()
    {
        SceneManager.LoadScene("ARScene");
    }

    public void OnBackToMainClick()
    {
        arSession.Reset();
        SceneManager.LoadScene("EntryScene");
    }
}
