using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public Transform inventoryPosition;

    void Update()
    {
        transform.position = inventoryPosition.position;
    }
}
