using UnityEngine;

public class RectangleController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    bool alreadyTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyTriggered) return;

        alreadyTriggered = true;
        animator.SetBool("isTwisting", true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (alreadyTriggered) return;

        alreadyTriggered = true;
        animator.SetBool("isTwisting", true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        alreadyTriggered = false;
        animator.SetBool("isTwisting", false);
    }
}