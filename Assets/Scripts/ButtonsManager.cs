using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

public class ButtonsManager : MonoBehaviour
{
    [SerializeField] private ARSession arSession;
    public void OnCuratorClick()
    {
        SceneManager.LoadScene("CuratorScene");
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
        if (SceneManager.GetActiveScene().name == "ARScene" && arSession != null)
        {
            arSession.Reset();
        }
        SceneManager.LoadScene("EntryScene");
    }
}
