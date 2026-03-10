using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonsManager : MonoBehaviour
{
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
        SceneManager.LoadScene("EntryScene");
    }
}
