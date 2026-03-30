using System;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    public event Action<GameObject> SelectionChanged;
    private GameObject current;
    [SerializeField] private LayerMask raycastLayerMask;

    void Update()
    {
        GameObject next = null;

        if (Physics.Raycast(transform.position, transform.forward, out var hit, 10f, raycastLayerMask))
        {
            next = hit.collider.transform.parent.gameObject;
        }
        
        // SelectionChanged?.Invoke(current);

        if (next == current) return;

        current = next;
        SelectionChanged?.Invoke(current);
    }
}
