using UnityEngine;
using UnityEngine.UI;

public class ScaleAdjustment : MonoBehaviour
{
    private const float DefaultScale = 0.5f;
    private const float MinScale = 0.1f;

    [SerializeField] private Raycast raycast;
    [SerializeField] private Slider slider;
    private GameObject content;

    void OnEnable()
    {
        if (raycast != null)
        {
            raycast.SelectionChanged += GetContentTransform;
        }
    }

    void OnDisable()
    {
        if (raycast != null)
        {
            raycast.SelectionChanged -= GetContentTransform;
        }
    }

    void Start()
    {
        if (slider != null)
        {
            slider.SetValueWithoutNotify(DefaultScale);
        }
    }

    private void GetContentTransform(GameObject currentContent)
    {
        content = currentContent;
        if (content == null || slider == null) return;

        float factor = ApplyPersistedScale(content);
        slider.SetValueWithoutNotify(factor);
    }

    public void AdjustContentScale(float factor)
    {
        if (content == null) return;

        factor = Mathf.Clamp(factor, MinScale, 1f);
        ApplyScale(content, factor);
        PlayerPrefs.SetFloat(content.name, factor);
    }

    public float ApplyPersistedScale(GameObject targetContent)
    {
        if (targetContent == null) return DefaultScale;

        string key = targetContent.name;
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, DefaultScale);
        }

        float storedFactor = PlayerPrefs.GetFloat(key, DefaultScale);
        float factor = Mathf.Clamp(storedFactor, MinScale, 1f);
        if (!Mathf.Approximately(storedFactor, factor))
        {
            PlayerPrefs.SetFloat(key, factor);
        }

        ApplyScale(targetContent, factor);
        return factor;
    }

    private static void ApplyScale(GameObject targetContent, float factor)
    {
        targetContent.transform.localScale = new Vector3(factor, factor, factor);
    }
}
