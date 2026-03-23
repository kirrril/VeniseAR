using UnityEngine;
using UnityEngine.InputSystem;

public class GyroCamera : MonoBehaviour
{
    private Vector3 initPosition = new Vector3(0, 30, 0);
    private Vector3 initRotation = new Vector3(25, -4, 0);
    private bool gyroAvailable;

    void OnEnable()
    {
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            gyroAvailable = true;
        }
        else
        {
            transform.position = initPosition;
            transform.rotation = Quaternion.Euler(initRotation);
            gyroAvailable = false;
        }  
    }

    void OnDisable()
    {
        if (AttitudeSensor.current != null)
            InputSystem.DisableDevice(AttitudeSensor.current);
    }

    void Update()
    {
        if (!gyroAvailable) return;
        if (AttitudeSensor.current == null) return;
        var q = AttitudeSensor.current.attitude.ReadValue();
        transform.rotation = GyroToUnity(q);
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        Quaternion unityQ = new Quaternion(q.x, q.y, -q.z, -q.w);

        return Quaternion.Euler(90, 0, 0) * unityQ;
    }
}
