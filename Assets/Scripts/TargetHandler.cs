using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TargetHandler : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager imageManager;
    [SerializeField] ARAnchorManager anchorManager;
    [SerializeField] GameObject sphereSculpture;
    [SerializeField] GameObject rectangularSculpture;
    [SerializeField] GameObject showerSculpture;
    private HashSet<string> placed = new();
    private Dictionary<string, ARAnchor> imageAnchors = new();

    void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnImageDetected);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnImageDetected);
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            InputSystem.Update();
        }
    }

    async void OnImageDetected(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var img in args.added.ToList())
        {
            await TryPlaceAnchorAndContent(img);
        }

        foreach (var img in args.updated.ToList())
        {
            string key = img.referenceImage.guid.ToString();
            if (!placed.Contains(key)) continue;

            AjustAnchor(key, img.transform.position, img.transform.rotation);
        }
    }

    private async Task TryPlaceAnchorAndContent(ARTrackedImage img)
    {
        if (img == null) return;

        string key = img.referenceImage.guid.ToString();
        if (placed.Contains(key)) return;

        await Task.Delay(200);

        var result = await anchorManager.TryAddAnchorAsync(img.pose);
        if (!result.status.IsSuccess()) return;

        ARAnchor anchor = result.value;
        if (anchor == null) return;
        imageAnchors[key] = anchor;

        Debug.Log("Image position: " + img.transform.position);
        Debug.Log("Anchor created at: " + anchor.transform.position);


        bool contentPlaced = PlaceContent(img.referenceImage.name, anchor.transform);
        if (!contentPlaced) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        placed.Add(key);
    }

    private bool TryGetAnchorForContent(string key, out ARAnchor anchor)
    {
        if (imageAnchors.TryGetValue(key, out anchor)) return true;
        anchor = null;
        return false;
    }

    private void AjustAnchor(string key, Vector3 position, Quaternion rotation)
    {
        if (TryGetAnchorForContent(key, out ARAnchor anchor))
        {
            anchor.transform.position = position;
            anchor.transform.rotation = rotation;
        }
    }

    private GameObject GetContent(string name)
    {
        switch (name)
        {
            case "SphereTargetNew":
                return sphereSculpture;
            case "RectTargetNew":
                return rectangularSculpture;
            case "ShowerTargetNew":
                return showerSculpture;
            default:
                return null;
        }
    }

    private bool PlaceContent(string targetImageName, Transform anchorTransform)
    {
        GameObject content = GetContent(targetImageName);
        if (content == null) return false;

        content.transform.SetParent(anchorTransform, false);
        content.transform.localPosition = Vector3.zero;
        content.transform.localRotation = Quaternion.identity;
        content.SetActive(true);
        return true;
    }
}
