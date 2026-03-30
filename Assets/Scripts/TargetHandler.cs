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
    [SerializeField] ScaleAdjustment scaleAdjstment;
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

    public async void OnImageDetected(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var img in args.added.ToList())
        {
            await TryPlaceAnchorAndContent(img);
        }

        foreach (var img in args.updated.ToList())
        {
            string key = img.referenceImage.guid.ToString();
            if (!placed.Contains(key)) continue;

            AdjustAnchor(key, img);
        }
    }

    private Quaternion GetUprightYawRotation(ARTrackedImage img)
    {
        Vector3 yawSource = Vector3.ProjectOnPlane(img.transform.right, Vector3.up);

        if (yawSource.sqrMagnitude < 0.0001f)
        {
            yawSource = Vector3.ProjectOnPlane(img.transform.forward, Vector3.up);
        }

        if (yawSource.sqrMagnitude < 0.0001f)
        {
            yawSource = Vector3.forward;
        }

        return Quaternion.LookRotation(yawSource.normalized, Vector3.up);
    }

    private async Task TryPlaceAnchorAndContent(ARTrackedImage img)
    {
        if (img == null) return;
        string key = img.referenceImage.guid.ToString();
        if (placed.Contains(key)) return;

        await Task.Delay(200);

        // var anchorPose = new Pose(img.pose.position, Quaternion.identity);
        
        var anchorPose = new Pose(img.pose.position, GetUprightYawRotation(img));
        var result = await anchorManager.TryAddAnchorAsync(anchorPose);
        if (!result.status.IsSuccess()) return;
        ARAnchor anchor = result.value;
        if (anchor == null) return;
        imageAnchors[key] = anchor;

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

    private void AdjustAnchor(string key, ARTrackedImage img)
    {
        if (img == null) return;

        if (TryGetAnchorForContent(key, out ARAnchor anchor))
        {
            anchor.transform.position = img.transform.position;
            anchor.transform.rotation = GetUprightYawRotation(img);
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

        if (scaleAdjstment != null)
        {
            scaleAdjstment.ApplyPersistedScale(content);
        }
        else
        {
            content.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        content.SetActive(true);
        return true;
    }
}
