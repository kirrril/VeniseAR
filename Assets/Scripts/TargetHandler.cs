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
            // if (img.trackingState != TrackingState.Tracking) continue;


            string key = img.referenceImage.guid.ToString();
            if (placed.Contains(key)) continue;

            await Task.Delay(200);

            var result = await anchorManager.TryAddAnchorAsync(img.pose);
            if (!result.status.IsSuccess()) continue;

            ARAnchor anchor = result.value;
            imageAnchors[key] = anchor;

            Debug.Log("Image position : " + img.transform.position);
            Debug.Log("Ancre créée à : " + anchor.transform.position);

            GameObject content = GetContent(img.referenceImage.name);
            if (content == null) continue;

            content.transform.SetParent(anchor.transform, false);
            content.transform.localPosition = new Vector3(0, 0f, 0);
            content.transform.localRotation = Quaternion.Euler(0, 0, 0);

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif

            content.SetActive(true);

            placed.Add(key);
        }

        foreach (var img in args.updated)
        {
            string key = img.referenceImage.guid.ToString();
            if (!placed.Contains(key)) continue;

            if (TryGetAnchorForImage(key, out ARAnchor anchor))
            {
                anchor.transform.position = img.transform.position;
                anchor.transform.rotation = img.transform.rotation;
            }
        }
    }

    private bool TryGetAnchorForImage(string key, out ARAnchor anchor)
    {
        if (imageAnchors.TryGetValue(key, out anchor)) return true;
        anchor = null;
        return false;
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
}