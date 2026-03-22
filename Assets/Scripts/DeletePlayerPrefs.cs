using UnityEngine;

public class DeletePlayerPrefs : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.DeleteAll();
    }
}