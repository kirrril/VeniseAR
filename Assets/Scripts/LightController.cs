using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARCameraManager))]
public class LightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private Light directionalLight;
    [SerializeField] private GameObject lightDirectionPanel;
    [SerializeField] private Slider manualDirectionSlider;

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
    [SerializeField] private Vector3 manualDirectionEuler = new Vector3(50f, -30f, 0f);
    [SerializeField, Min(0f)] private float rotationSmoothing = 8f;
    [SerializeField, Min(0f)] private float autoDirectionGracePeriod = 1f;

    private bool hasValidMainLightDirection;
    private bool fallbackManualActive;
    private float waitingForAutoTimer;

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

        directionalLight.enabled = true;
        hasValidMainLightDirection = false;
        fallbackManualActive = false;
        waitingForAutoTimer = 0f;

        RefreshManualUi();
        SyncSliderWithoutNotify();
    }

    void OnDisable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    public void SetManualYaw(float t)
    {
        t = Mathf.Clamp01(t);
        manualDirectionEuler.y = t * 360f;

        if (!hasValidMainLightDirection && directionalLight != null)
            ApplyDirection(Quaternion.Euler(manualDirectionEuler), true);
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
        bool directionAvailable =
            estimation.mainLightDirection.HasValue &&
            estimation.mainLightDirection.Value.sqrMagnitude > 0.0001f;

        if (directionAvailable)
        {
            if (!hasValidMainLightDirection)
            {
                hasValidMainLightDirection = true;
                fallbackManualActive = false;
                waitingForAutoTimer = 0f;
                RefreshManualUi();
            }

            Vector3 dir = estimation.mainLightDirection.Value;
            ApplyDirection(Quaternion.LookRotation(dir.normalized, Vector3.up), false);
            return;
        }

        if (hasValidMainLightDirection)
        {
            hasValidMainLightDirection = false;
            waitingForAutoTimer = 0f;
            RefreshManualUi();
        }

        waitingForAutoTimer += Time.deltaTime;

        if (waitingForAutoTimer < autoDirectionGracePeriod)
            return;

        if (!fallbackManualActive)
        {
            fallbackManualActive = true;
            RefreshManualUi();
        }

        ApplyDirection(Quaternion.Euler(manualDirectionEuler), false);
    }

    private void RefreshManualUi()
    {
        if (lightDirectionPanel != null)
            lightDirectionPanel.SetActive(applyDirection && fallbackManualActive && !hasValidMainLightDirection);
    }

    private void SyncSliderWithoutNotify()
    {
        if (manualDirectionSlider == null)
            return;

        manualDirectionSlider.SetValueWithoutNotify(Mathf.Repeat(manualDirectionEuler.y, 360f) / 360f);
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
