using System.Collections;
using UnityEngine;
using Vuforia;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private Vector3 sculpturePosition;
    [SerializeField] private Vector3 sculptureRotation;
    private ImageTargetBehaviour imageTarget;
    private bool placed = false;

    void Awake()
    {
        imageTarget = GetComponent<ImageTargetBehaviour>();
        imageTarget.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDestroy()
    {
        if (imageTarget != null)
            imageTarget.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour observerBehaviour, TargetStatus targetStatus)
    {
        if (!placed && targetStatus.Status == Status.TRACKED)
        {
            placed = true;
            StartCoroutine(PlaceSculptureDisableTarget());
        }
    }

    private IEnumerator PlaceSculptureDisableTarget()
    {
        yield return new WaitForSeconds(1f);
        placed = true;
        ARAnchor anchor = new GameObject("Anchor_" + content.name).AddComponent<ARAnchor>();
        anchor.transform.position = imageTarget.transform.position;
        anchor.transform.rotation = imageTarget.transform.rotation;

        content.transform.SetParent(anchor.transform, worldPositionStays: true);
        content.transform.position = imageTarget.transform.position + sculpturePosition;
        content.transform.rotation = Quaternion.Euler(sculptureRotation);
        content.SetActive(true);
    }
}
