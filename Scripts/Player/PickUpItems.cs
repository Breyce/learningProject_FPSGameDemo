using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ʰȡ
/// </summary>
public class PickUpItems : MonoBehaviour
{
    public float rotateSpeed;
    public int itemID;
    private GameObject weaponModel;
    private Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        rotateSpeed = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, rotateSpeed * Time.deltaTime, 0); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player") 
        {
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            inventory = GameObject.Find("Inventory").GetComponent<Inventory>();

            if (inventory.weapons.Count == 3) return;
            //���һ�ȡInventory�ĸ����������塣
            weaponModel = inventory.gameObject.transform.GetChild(itemID).gameObject;
            Debug.Log(weaponModel.name);
            Debug.Log(player);

            player.PickUpWeapon(itemID, weaponModel);

            Destroy(gameObject);
        }
    }
}
