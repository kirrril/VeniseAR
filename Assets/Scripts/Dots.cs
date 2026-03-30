using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Dots : MonoBehaviour
{
    private TMP_Text dotsText;
    public event Action DotsCompleted;
    void Awake()
    {
        dotsText = transform.GetComponent<TMP_Text>();
    }
    void OnEnable()
    {
        StartCoroutine(AddDots());
    }

    void OnDisable()
    {
        dotsText.text = "";
        StopAllCoroutines();
    }

    private IEnumerator AddDots()
    {
        yield return new WaitForSeconds(0.2f);
        dotsText.text = ".";
        yield return new WaitForSeconds(0.2f);
        dotsText.text = "..";
        yield return new WaitForSeconds(0.2f);
        dotsText.text = "...";
        yield return new WaitForSeconds(0.2f);
        dotsText.text = "....";
        yield return new WaitForSeconds(0.2f);
        dotsText.text = ".....";
        yield return new WaitForSeconds(0.2f);
        dotsText.text = "......";
        DotsCompleted?.Invoke();
    }
}
