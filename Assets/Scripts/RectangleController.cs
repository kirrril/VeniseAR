using UnityEngine;

public class RectangleController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    // [SerializeField] private GameObject canvas;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        animator.SetBool("isTwisting", true);
        // canvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        animator.SetBool("isTwisting", false);
        // canvas.SetActive(false);
    }
}