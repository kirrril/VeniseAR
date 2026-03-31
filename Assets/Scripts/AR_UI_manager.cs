using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AR_UI_manager : MonoBehaviour
{
    [SerializeField] private Raycast raycast;
    [SerializeField] private TargetHandler targetHandler;
    [SerializeField] private GameObject closeSceneButton;
    [SerializeField] private GameObject contentMenu;
    [SerializeField] private GameObject burgerButton;
    [SerializeField] private GameObject burgerContent;
    [SerializeField] private GameObject closeBurgerButton;
    [SerializeField] private GameObject optionsHint;
    [SerializeField] private GameObject targetHint;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text artist;
    [SerializeField] private TMP_Text year;
    [SerializeField] private TMP_Text dimensions;
    private bool burgerIsOn;
    private string burgerKey = "burgerOn";
    private bool displayOptionsHint;
    private string optionshintKey = "optionsHint";
    private bool displayTargetHint;

    private string titleText;
    private string artistText;
    private string yearText;
    private string dimensionsText;

    void OnEnable()
    {
        raycast.SelectionChanged += GetInfosSwitchMenu;
        targetHandler.contentWasPlaced += HideTargetHint;

        contentMenu.SetActive(false);

        burgerIsOn = PlayerPrefs.GetInt(burgerKey, 1) == 1;
        SwitchMenu();

        displayOptionsHint = PlayerPrefs.GetInt(optionshintKey, 1) == 1;
        DisplayOptionsHint(displayOptionsHint);
    }

    void OnDisable()
    {
        raycast.SelectionChanged -= GetInfosSwitchMenu;
        targetHandler.contentWasPlaced -= HideTargetHint;
    }

    void Start()
    {
        displayTargetHint = true;
        targetHint.SetActive(displayTargetHint);
    }

    public void BurgerOnOff()
    {
        burgerIsOn = !burgerIsOn;
        SwitchMenu();
        PlayerPrefs.SetInt(optionshintKey, 0);
        int burgerKeyValue = burgerIsOn ? 1 : 0;
        PlayerPrefs.SetInt(burgerKey, burgerKeyValue);
        PlayerPrefs.Save();
        displayOptionsHint = false;
        DisplayOptionsHint(displayOptionsHint);
    }

    public void SwitchMenu()
    {
        if (!burgerIsOn)
        {
            burgerButton.SetActive(true);
            burgerContent.SetActive(false);
            return;
        }
        burgerButton.SetActive(false);
        burgerContent.SetActive(true);
        title.text = titleText;
        artist.text = artistText;
        year.text = yearText;
        dimensions.text = dimensionsText;
    }

    private void GetInfosSwitchMenu(GameObject content)
    {
        if (content == null)
        {
            contentMenu.SetActive(false);
            SwitchMenu();
            return;
        }
        titleText = GetTitle(content.name);
        artistText = GetArtist(content.name);
        yearText = GetYear(content.name);
        dimensionsText = GetDimensions(content.name);
        contentMenu.SetActive(true);
        SwitchMenu();
    }

    private void DisplayOptionsHint(bool doDisplay)
    {
        optionsHint.SetActive(doDisplay);
    }

    private void HideTargetHint()
    {
        if (!displayTargetHint) return;
        
        displayTargetHint = false;
        targetHint.SetActive(displayTargetHint);
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
}
