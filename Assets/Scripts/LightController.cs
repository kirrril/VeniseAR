using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARCameraManager))]
public class LightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private Light directionalLight;

    [Header("Requested Light Estimation")]
    [SerializeField]
    private LightEstimation requestedLightEstimation =
        LightEstimation.AmbientIntensity |
        LightEstimation.AmbientColor |
        LightEstimation.MainLightDirection |
        LightEstimation.MainLightIntensity;

    [Header("Apply Estimation")]
    [SerializeField] private bool applyIntensity = true;
    [SerializeField] private bool applyColor = true;
    [SerializeField] private bool applyDirection = true;

    [Header("Intensity")]
    [SerializeField] private float intensityMultiplier = 1f;
    [SerializeField] private float minIntensity = 0.2f;
    [SerializeField] private float maxIntensity = 1.8f;
    [SerializeField, Min(0f)] private float intensitySmoothing = 8f;

    [Header("Color")]
    [SerializeField, Min(0f)] private float colorSmoothing = 8f;

    [Header("Direction Fallback")]
    [SerializeField] private bool forceManualDirection = false;
    [SerializeField] private bool useManualDirectionWhenUnavailable = true;
    [SerializeField] private Vector3 manualDirectionEuler = new Vector3(50f, -30f, 0f);
    [SerializeField, Min(0f)] private float rotationSmoothing = 8f;

    void Reset()
    {
        cameraManager = GetComponent<ARCameraManager>();
    }

    void Awake()
    {
        if (cameraManager == null)
            cameraManager = GetComponent<ARCameraManager>();
    }

    void OnEnable()
    {
        if (cameraManager == null || directionalLight == null)
            return;

        cameraManager.requestedLightEstimation |= requestedLightEstimation;
        cameraManager.frameReceived += OnCameraFrameReceived;

        if (forceManualDirection || useManualDirectionWhenUnavailable)
            ApplyDirection(Quaternion.Euler(manualDirectionEuler), true);
    }

    void OnDisable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    // void Update()
    // {
    //     if (!applyDirection || !forceManualDirection || directionalLight == null)
    //         return;

    //     ApplyDirection(Quaternion.Euler(manualDirectionEuler), false);
    // }

    public void SetManualYaw(float t)
    {
        t = Mathf.Clamp01(t);
        manualDirectionEuler.y = t * 360f;

        // if (forceManualDirection && directionalLight != null)
        //     ApplyDirection(Quaternion.Euler(manualDirectionEuler), true);
    }

    public void SetForceManualDirection(bool enabled)
    {
        forceManualDirection = enabled;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (directionalLight == null)
            return;

        var estimation = args.lightEstimation;

        if (applyIntensity) UpdateIntensity(estimation);
        if (applyColor) UpdateColor(estimation);
        if (applyDirection) UpdateDirection(estimation);
    }

    private void UpdateIntensity(ARLightEstimationData estimation)
    {
        float? estimated = estimation.averageMainLightBrightness ?? estimation.averageBrightness;
        if (!estimated.HasValue)
            return;

        float target = Mathf.Clamp(estimated.Value * intensityMultiplier, minIntensity, maxIntensity);
        directionalLight.intensity = Mathf.Lerp(
            directionalLight.intensity,
            target,
            Damp(intensitySmoothing));
    }

    private void UpdateColor(ARLightEstimationData estimation)
    {
        Color? estimated = estimation.mainLightColor ?? estimation.colorCorrection;
        if (!estimated.HasValue)
            return;

        Color target = estimated.Value;
        target.a = 1f;

        directionalLight.color = Color.Lerp(
            directionalLight.color,
            target,
            Damp(colorSmoothing));
    }

    private void UpdateDirection(ARLightEstimationData estimation)
    {
        if (forceManualDirection)
        {
            ApplyDirection(Quaternion.Euler(manualDirectionEuler), false);
            return;
        }

        if (estimation.mainLightDirection.HasValue)
        {
            Vector3 dir = estimation.mainLightDirection.Value;
            if (dir.sqrMagnitude > 0.0001f)
            {
                ApplyDirection(Quaternion.LookRotation(dir.normalized, Vector3.up), false);
                return;
            }
        }

        if (useManualDirectionWhenUnavailable)
            ApplyDirection(Quaternion.Euler(manualDirectionEuler), false);
    }

    private void ApplyDirection(Quaternion targetRotation, bool immediate)
    {
        if (immediate)
        {
            directionalLight.transform.rotation = targetRotation;
            return;
        }

        directionalLight.transform.rotation = Quaternion.Slerp(
            directionalLight.transform.rotation,
            targetRotation,
            Damp(rotationSmoothing));
    }

    private static float Damp(float smoothing)
    {
        if (smoothing <= 0f)
            return 1f;

        return 1f - Mathf.Exp(-smoothing * Time.deltaTime);
    }
}