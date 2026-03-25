using System.Collections;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] MoveGizmo moveGizmo;
    [SerializeField] GyroCamera gyroCamera;
    public Rigidbody rb;
    public Transform cameraPlace;
    public Transform entryPoint;
    private bool gyroIsOn;

    void Start()
    {
        transform.position = entryPoint.position;
        transform.rotation = entryPoint.rotation;
        gyroIsOn = gyroCamera.gyroAvailable ? true : false;
    }

    void FixedUpdate()
    {
        HandleWalking();
    }

    private void HandleWalking()
    {
        MovePlayer();
        RotatePlayer();
    }

    private void MovePlayer()
    {
        Vector3 targetVelocity = transform.forward * moveGizmo.speedY * 2.5f + transform.right * moveGizmo.speedX * 1.5f;
        rb.linearVelocity = targetVelocity;
    }

    private void RotatePlayer()
    {
        if (gyroIsOn)
        {
            float yaw = Camera.main.transform.eulerAngles.y;
            rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
            rb.angularVelocity = Vector3.zero;
            return;
        }

        float yawSpeedRad = moveGizmo.speedX * 1.5f;
        rb.angularVelocity = new Vector3(0f, yawSpeedRad, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;
    }
}
