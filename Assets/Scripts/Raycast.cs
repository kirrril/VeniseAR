using System;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    public event Action<GameObject> SelectionChanged;

    private GameObject current;

    void Update()
    {
        GameObject next = null;

        if (Physics.Raycast(transform.position, transform.forward, out var hit, 10f))
        {
            next = hit.collider.transform.gameObject;
        }

        if (next == current) return;

        current = next;
        SelectionChanged?.Invoke(current);
    }
}
