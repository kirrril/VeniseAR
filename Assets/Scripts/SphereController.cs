using UnityEngine;

public class SphereController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject dust;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        if (distance < 1.8f)
        {
            animator.SetBool("isBouncing", true);
            dust.SetActive(true);
        }
        else
        {
            animator.SetBool("isBouncing", false);
            dust.SetActive(false);
        }
    }
}
