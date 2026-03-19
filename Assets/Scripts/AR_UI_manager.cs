using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AR_UI_manager : MonoBehaviour
{
    [SerializeField] private Raycast raycast;
    [SerializeField] private GameObject contentMenu;
    [SerializeField] private GameObject closeSceneButton;
    [SerializeField] private GameObject contentMenuButton;
    [SerializeField] private GameObject closeContentMenuButton;
    [SerializeField] private GameObject contentText;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text artist;
    [SerializeField] private TMP_Text year;
    [SerializeField] private TMP_Text dimensions;
    private string titleText;
    private string artistText;
    private string yearText;
    private string dimensionsText;

    void OnEnable()
    {
        raycast.SelectionChanged += HandleContentCommands;
    }

    void OnDisable()
    {
        raycast.SelectionChanged -= HandleContentCommands;
    }

    void Start()
    {
        closeSceneButton.SetActive(true);
        contentMenu.SetActive(false);
    }

    private void HandleContentCommands(GameObject content)
    {
        if (content == null)
        {
            contentMenu.SetActive(false);
            closeSceneButton.SetActive(true);
            return;
        }
        contentMenu.SetActive(true);
        contentMenuButton.SetActive(true);
        contentText.SetActive(false);
        titleText = GetTitle(content.name);
        artistText = GetArtist(content.name);
        yearText = GetYear(content.name);
        dimensionsText = GetDimensions(content.name);
    }

    private string GetTitle(string contentName)
    {
        switch (contentName)
        {
            case "RectangularSculpturePrefab":
                return "Twisting pillar";
            case "ShowerSculpturePrefab":
                return "Ice-cold shower";
            case "SphereSculpturePrefab":
                return "Big yellow bouncing ball";
            default:
                return "";
        }
    }

    private string GetArtist(string contentName)
    {
        switch (contentName)
        {
            case "RectangularSculpturePrefab":
                return "Hikusiko Takahashi";
            case "ShowerSculpturePrefab":
                return "John Doe";
            case "SphereSculpturePrefab":
                return "Juan Antonio Sanchez";
            default:
                return "";
        }
    }

    private string GetYear(string contentName)
    {
        switch (contentName)
        {
            case "RectangularSculpturePrefab":
                return "2025";
            case "ShowerSculpturePrefab":
                return "2022";
            case "SphereSculpturePrefab":
                return "2026";
            default:
                return "";
        }
    }

    private string GetDimensions(string contentName)
    {
        switch (contentName)
        {
            case "RectangularSculpturePrefab":
                return "250 x 40 x 40";
            case "ShowerSculpturePrefab":
                return "200 x 100 x 100";
            case "SphereSculpturePrefab":
                return "100 x 100 x 100";
            default:
                return "";
        }
    }

    public void OnContentMenuButtonClick()
    {
        closeSceneButton.SetActive(false);
        contentMenuButton.SetActive(false);
        closeContentMenuButton.SetActive(true);
        contentText.SetActive(true);
        title.text = titleText;
        artist.text = artistText;
        year.text = yearText;
        dimensions.text = dimensionsText;
    }

    public void OnCloseContentMenuButtonClick()
    {
        closeSceneButton.SetActive(true);
        contentMenuButton.SetActive(true);
        closeContentMenuButton.SetActive(false);
        contentText.SetActive(false);
    }
}
