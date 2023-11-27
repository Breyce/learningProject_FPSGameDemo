using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPoint : MonoBehaviour
{
    public float Max_damage;
    public float Min_damage;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //Íæ¼ÒÊÜµ½ÉËº¦¿ÛÑª
            other.GetComponentInParent<PlayerMovement>().PlayerHealth(Random.Range(Min_damage,Max_damage));
        }
    }
}
