/////////////////////////////////////////////////////////////
/// AR Anchors

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// public class TargetHandler : MonoBehaviour
// {
//     [SerializeField] ARTrackedImageManager imageManager;
//     [SerializeField] ARAnchorManager anchorManager;
//     [SerializeField] GameObject sphereSculpture;
//     [SerializeField] GameObject rectangularSculpture;
//     [SerializeField] GameObject showerSculpture;
//     [SerializeField] ScaleAdjustment scaleAdjstment;
//     private HashSet<string> placed = new();
//     private Dictionary<string, ARAnchor> imageAnchors = new();
//     public event Action contentWasPlaced;

//     void OnEnable()
//     {
//         imageManager.trackablesChanged.AddListener(OnImageDetected);
//         Screen.sleepTimeout = SleepTimeout.NeverSleep;
//     }

//     void OnDisable()
//     {
//         imageManager.trackablesChanged.RemoveListener(OnImageDetected);
//         Screen.sleepTimeout = SleepTimeout.SystemSetting;
//     }

//     void OnApplicationPause(bool pause)
//     {
//         if (!pause)
//         {
//             InputSystem.Update();
//         }
//     }

//     public async void OnImageDetected(ARTrackablesChangedEventArgs<ARTrackedImage> args)
//     {
//         foreach (var img in args.added.ToList())
//         {
//             await TryPlaceAnchorAndContent(img);
//         }

//         foreach (var img in args.updated.ToList())
//         {
//             string key = img.referenceImage.guid.ToString();
//             if (!placed.Contains(key)) continue;

//             if (img.trackingState == TrackingState.Tracking)
//                 AdjustAnchor(key, img);
//         }
//     }

//     private Quaternion GetUprightYawRotation(ARTrackedImage img)
//     {
//         Vector3 yawSource = Vector3.ProjectOnPlane(img.transform.right, Vector3.up);

//         if (yawSource.sqrMagnitude < 0.0001f)
//         {
//             yawSource = Vector3.ProjectOnPlane(img.transform.forward, Vector3.up);
//         }

//         if (yawSource.sqrMagnitude < 0.0001f)
//         {
//             yawSource = Vector3.forward;
//         }

//         return Quaternion.LookRotation(yawSource.normalized, Vector3.up);
//     }

//     private async Task TryPlaceAnchorAndContent(ARTrackedImage img)
//     {
//         if (img == null) return;
//         string key = img.referenceImage.guid.ToString();
//         if (placed.Contains(key)) return;

//         await Task.Delay(200);

//         // var anchorPose = new Pose(img.pose.position, Quaternion.identity);

//         var anchorPose = new Pose(img.pose.position, GetUprightYawRotation(img));
//         var result = await anchorManager.TryAddAnchorAsync(anchorPose);
//         if (!result.status.IsSuccess()) return;
//         ARAnchor anchor = result.value;
//         if (anchor == null) return;
//         imageAnchors[key] = anchor;

//         bool contentPlaced = PlaceContent(img.referenceImage.name, anchor.transform);
//         if (!contentPlaced) return;

// #if UNITY_ANDROID || UNITY_IOS
//         Handheld.Vibrate();
// #endif

//         contentWasPlaced?.Invoke();
//         placed.Add(key);
//     }

//     private bool TryGetAnchorForContent(string key, out ARAnchor anchor)
//     {
//         if (imageAnchors.TryGetValue(key, out anchor)) return true;
//         anchor = null;
//         return false;
//     }

//     private void AdjustAnchor(string key, ARTrackedImage img)
//     {
//         if (img == null) return;

//         if (TryGetAnchorForContent(key, out ARAnchor anchor))
//         {
//             anchor.transform.position = img.transform.position;
//             anchor.transform.rotation = GetUprightYawRotation(img);
//         }
//     }

//     private GameObject GetContent(string name)
//     {
//         switch (name)
//         {
//             case "SphereTargetNew":
//                 return sphereSculpture;
//             case "RectTargetNew":
//                 return rectangularSculpture;
//             case "ShowerTargetNew":
//                 return showerSculpture;
//             default:
//                 return null;
//         }
//     }

//     private bool PlaceContent(string targetImageName, Transform anchorTransform)
//     {
//         GameObject content = GetContent(targetImageName);
//         if (content == null) return false;

//         content.transform.SetParent(anchorTransform, false);
//         content.transform.localPosition = Vector3.zero;
//         content.transform.localRotation = Quaternion.identity;

//         if (scaleAdjstment != null)
//         {
//             scaleAdjstment.ApplyPersistedScale(content);
//         }
//         else
//         {
//             content.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
//         }

//         content.SetActive(true);
//         return true;
//     }
// }

/////////////////////////////////////////////////////////////////////////////
/// No AR Anchors

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TargetHandler : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private GameObject sphereSculpture;
    [SerializeField] private GameObject rectangularSculpture;
    [SerializeField] private GameObject showerSculpture;
    [SerializeField] private ScaleAdjustment scaleAdjstment;

    private readonly HashSet<string> placed = new();
    private readonly Dictionary<string, GameObject> imageContents = new();

    public event Action contentWasPlaced;

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
            await TryPlaceContent(img);
        }

        foreach (var img in args.updated.ToList())
        {
            if (img == null || img.trackingState != TrackingState.Tracking)
                continue;

            RefreshPlacedContent(img);
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

    private async Task TryPlaceContent(ARTrackedImage img)
    {
        if (img == null)
            return;

        string key = img.referenceImage.guid.ToString();
        if (placed.Contains(key))
        {
            RefreshPlacedContent(img);
            return;
        }

        await Task.Delay(200);

        if (img == null)
            return;

        GameObject content = GetContent(img.referenceImage.name);
        if (content == null)
            return;

        imageContents[key] = content;

        AttachContentToTrackedImage(content, img);

        if (scaleAdjstment != null)
        {
            scaleAdjstment.ApplyPersistedScale(content);
        }
        else
        {
            content.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        content.SetActive(true);
        placed.Add(key);

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        contentWasPlaced?.Invoke();
    }

    private void RefreshPlacedContent(ARTrackedImage img)
    {
        if (img == null)
            return;

        string key = img.referenceImage.guid.ToString();
        if (!imageContents.TryGetValue(key, out GameObject content) || content == null)
            return;

        AttachContentToTrackedImage(content, img);

        if (!content.activeSelf)
        {
            content.SetActive(true);
        }
    }

    private void AttachContentToTrackedImage(GameObject content, ARTrackedImage img)
    {
        Transform contentTransform = content.transform;
        contentTransform.SetParent(img.transform, true);
        contentTransform.position = img.transform.position;
        contentTransform.rotation = GetUprightYawRotation(img);
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

///////////////////////////////////////////////////////////////////////
/// no AR Anchors + hide content if isnt' tracked
/// 

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;

// public class TargetHandler : MonoBehaviour
// {
//     [SerializeField] private ARTrackedImageManager imageManager;
//     [SerializeField] private GameObject sphereSculpture;
//     [SerializeField] private GameObject rectangularSculpture;
//     [SerializeField] private GameObject showerSculpture;
//     [SerializeField] private ScaleAdjustment scaleAdjstment;

//     private readonly HashSet<string> placed = new();
//     private readonly Dictionary<string, GameObject> imageContents = new();

//     public event Action contentWasPlaced;

//     void OnEnable()
//     {
//         imageManager.trackablesChanged.AddListener(OnImageDetected);
//         Screen.sleepTimeout = SleepTimeout.NeverSleep;
//     }

//     void OnDisable()
//     {
//         imageManager.trackablesChanged.RemoveListener(OnImageDetected);
//         Screen.sleepTimeout = SleepTimeout.SystemSetting;
//     }

//     void OnApplicationPause(bool pause)
//     {
//         if (!pause)
//         {
//             InputSystem.Update();
//         }
//     }

//     public async void OnImageDetected(ARTrackablesChangedEventArgs<ARTrackedImage> args)
//     {
//         foreach (var img in args.added.ToList())
//         {
//             await TryPlaceContent(img);
//         }

//         foreach (var img in args.updated.ToList())
//         {
//             if (img == null)
//                 continue;

//             if (img.trackingState == TrackingState.Tracking)
//             {
//                 RefreshPlacedContent(img);
//             }
//             else
//             {
//                 HidePlacedContent(img.referenceImage.guid.ToString());
//             }
//         }

//         foreach (var removedPair in args.removed.ToList())
//         {
//             ARTrackedImage removedImage = removedPair.Value;
//             if (removedImage == null)
//                 continue;

//             HidePlacedContent(removedImage.referenceImage.guid.ToString());
//         }
//     }

//     private Quaternion GetUprightYawRotation(ARTrackedImage img)
//     {
//         Vector3 yawSource = Vector3.ProjectOnPlane(img.transform.right, Vector3.up);

//         if (yawSource.sqrMagnitude < 0.0001f)
//         {
//             yawSource = Vector3.ProjectOnPlane(img.transform.forward, Vector3.up);
//         }

//         if (yawSource.sqrMagnitude < 0.0001f)
//         {
//             yawSource = Vector3.forward;
//         }

//         return Quaternion.LookRotation(yawSource.normalized, Vector3.up);
//     }

//     private async Task TryPlaceContent(ARTrackedImage img)
//     {
//         if (img == null)
//             return;

//         string key = img.referenceImage.guid.ToString();
//         if (placed.Contains(key))
//         {
//             if (img.trackingState == TrackingState.Tracking)
//             {
//                 RefreshPlacedContent(img);
//             }
//             else
//             {
//                 HidePlacedContent(key);
//             }

//             return;
//         }

//         await Task.Delay(200);

//         if (img == null || img.trackingState != TrackingState.Tracking)
//             return;

//         GameObject content = GetContent(img.referenceImage.name);
//         if (content == null)
//             return;

//         imageContents[key] = content;

//         AttachContentToTrackedImage(content, img);

//         if (scaleAdjstment != null)
//         {
//             scaleAdjstment.ApplyPersistedScale(content);
//         }
//         else
//         {
//             content.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
//         }

//         content.SetActive(true);
//         placed.Add(key);

// #if UNITY_ANDROID || UNITY_IOS
//         Handheld.Vibrate();
// #endif

//         contentWasPlaced?.Invoke();
//     }

//     private void RefreshPlacedContent(ARTrackedImage img)
//     {
//         if (img == null)
//             return;

//         string key = img.referenceImage.guid.ToString();
//         if (!imageContents.TryGetValue(key, out GameObject content) || content == null)
//             return;

//         AttachContentToTrackedImage(content, img);

//         if (!content.activeSelf)
//         {
//             content.SetActive(true);
//         }
//     }

//     private void HidePlacedContent(string key)
//     {
//         if (string.IsNullOrWhiteSpace(key))
//             return;

//         if (!imageContents.TryGetValue(key, out GameObject content) || content == null)
//             return;

//         if (content.activeSelf)
//         {
//             content.SetActive(false);
//         }
//     }

//     private void AttachContentToTrackedImage(GameObject content, ARTrackedImage img)
//     {
//         Transform contentTransform = content.transform;
//         contentTransform.SetParent(img.transform, true);
//         contentTransform.position = img.transform.position;
//         contentTransform.rotation = GetUprightYawRotation(img);
//     }

//     private GameObject GetContent(string name)
//     {
//         switch (name)
//         {
//             case "SphereTargetNew":
//                 return sphereSculpture;
//             case "RectTargetNew":
//                 return rectangularSculpture;
//             case "ShowerTargetNew":
//                 return showerSculpture;
//             default:
//                 return null;
//         }
//     }
// }