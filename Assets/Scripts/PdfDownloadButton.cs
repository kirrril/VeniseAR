using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[Serializable]
public class StringUnityEvent : UnityEvent<string>
{
}

public class PdfDownloadButton : MonoBehaviour
{
    [Header("PDF Source")]
    [SerializeField] private string pdfUrl;
    [SerializeField] private string fileName = "ImageTargets.pdf";

    [Header("Android Download UI")]
    [SerializeField] private string downloadTitle = "Venise AR - Image Targets";
    [SerializeField] [TextArea] private string downloadDescription = "Telechargement du PDF des image targets";

    [Header("Callbacks")]
    [SerializeField] private UnityEvent onDownloadStarted;
    [SerializeField] private StringUnityEvent onDownloadSucceeded;
    [SerializeField] private StringUnityEvent onDownloadFailed;

    private Coroutine activeDownloadCoroutine;
    private string lastSavedPath;
    private string lastError;

    public string LastSavedPath => lastSavedPath;
    public string LastError => lastError;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SharePdfAtPath(string filePath);
#endif

    public void DownloadTargetsPdf()
    {
        if (string.IsNullOrWhiteSpace(pdfUrl))
        {
            Fail("PDF URL non renseignee dans l'Inspector.");
            return;
        }

        if (!Uri.TryCreate(pdfUrl, UriKind.Absolute, out Uri parsedUri) ||
            (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
        {
            Fail("PDF URL invalide. Utilise une URL http ou https.");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        StartAndroidDownload();
#else
        if (activeDownloadCoroutine != null)
        {
            Debug.LogWarning("Un telechargement est deja en cours.");
            return;
        }

        activeDownloadCoroutine = StartCoroutine(DownloadToAppStorageCoroutine());
#endif
    }

    public string GetExpectedSaveLocation()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return Path.Combine("Downloads", GetResolvedFileName());
#else
        return Path.Combine(Application.persistentDataPath, GetResolvedFileName());
#endif
    }

    private void StartAndroidDownload()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (TryEnqueueAndroidDownload(out string enqueueError))
        {
            lastError = string.Empty;
            lastSavedPath = Path.Combine("Downloads", GetResolvedFileName());
            onDownloadStarted?.Invoke();
            Debug.Log($"PDF enqueue dans le gestionnaire Android: {lastSavedPath}");
            return;
        }

        Fail(enqueueError);
#else
        Fail("Le telechargement Android n'est pas disponible sur cette plateforme.");
#endif
    }

    private IEnumerator DownloadToAppStorageCoroutine()
    {
        lastError = string.Empty;
        lastSavedPath = string.Empty;
        onDownloadStarted?.Invoke();

        string destinationPath = Path.Combine(Application.persistentDataPath, GetResolvedFileName());

        using (UnityWebRequest request = UnityWebRequest.Get(pdfUrl))
        {
            request.downloadHandler = new DownloadHandlerFile(destinationPath);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                Fail($"Echec du telechargement PDF: {request.error}");
                activeDownloadCoroutine = null;
                yield break;
            }
        }

        lastSavedPath = destinationPath;
        Debug.Log($"PDF telecharge dans le stockage de l'application: {lastSavedPath}");

#if UNITY_IOS && !UNITY_EDITOR
        TryPresentIosShareSheet(lastSavedPath);
#endif

        onDownloadSucceeded?.Invoke(lastSavedPath);
        activeDownloadCoroutine = null;
    }

    private void Fail(string message)
    {
        lastError = message;
        Debug.LogError(message);
        onDownloadFailed?.Invoke(message);
        activeDownloadCoroutine = null;
    }

    private string GetResolvedFileName()
    {
        string candidate = fileName;

        if (string.IsNullOrWhiteSpace(candidate) &&
            Uri.TryCreate(pdfUrl, UriKind.Absolute, out Uri parsedUri))
        {
            candidate = Path.GetFileName(parsedUri.LocalPath);
        }

        candidate = string.IsNullOrWhiteSpace(candidate) ? "ImageTargets.pdf" : candidate.Trim();

        if (!candidate.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            candidate += ".pdf";
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            candidate = candidate.Replace(invalidChar, '_');
        }

        return candidate;
    }

    private void TryPresentIosShareSheet(string filePath)
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            Debug.LogError("Impossible d'ouvrir la share sheet iOS: fichier PDF introuvable.");
            return;
        }

        try
        {
            SharePdfAtPath(filePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Impossible d'ouvrir la share sheet iOS: {ex.Message}");
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private bool TryEnqueueAndroidDownload(out string error)
    {
        error = string.Empty;

        try
        {
            string resolvedFileName = GetResolvedFileName();

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject pdfUri = uriClass.CallStatic<AndroidJavaObject>("parse", pdfUrl);
            AndroidJavaObject request = new AndroidJavaObject("android.app.DownloadManager$Request", pdfUri);
            AndroidJavaClass requestClass = new AndroidJavaClass("android.app.DownloadManager$Request");
            AndroidJavaClass environmentClass = new AndroidJavaClass("android.os.Environment");
            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");

            int notificationVisibility =
                requestClass.GetStatic<int>("VISIBILITY_VISIBLE_NOTIFY_COMPLETED");

            request.Call<AndroidJavaObject>("setTitle", downloadTitle);
            request.Call<AndroidJavaObject>("setDescription", downloadDescription);
            request.Call<AndroidJavaObject>("setMimeType", "application/pdf");
            request.Call<AndroidJavaObject>("setNotificationVisibility", notificationVisibility);
            request.Call<AndroidJavaObject>("setVisibleInDownloadsUi", true);
            request.Call<AndroidJavaObject>("setAllowedOverMetered", true);
            request.Call<AndroidJavaObject>("setAllowedOverRoaming", true);

            string downloadsDirectory = environmentClass.GetStatic<string>("DIRECTORY_DOWNLOADS");
            request.Call<AndroidJavaObject>(
                "setDestinationInExternalPublicDir",
                downloadsDirectory,
                resolvedFileName);

            string downloadService = contextClass.GetStatic<string>("DOWNLOAD_SERVICE");
            AndroidJavaObject downloadManager =
                currentActivity.Call<AndroidJavaObject>("getSystemService", downloadService);

            if (downloadManager == null)
            {
                error = "DownloadManager Android indisponible sur ce device.";
                return false;
            }

            downloadManager.Call<long>("enqueue", request);
            return true;
        }
        catch (Exception ex)
        {
            error = $"Impossible de lancer le telechargement Android: {ex.Message}";
            return false;
        }
    }
#endif
}
