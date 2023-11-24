using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ����������⣬�����л�����ӣ�ȥ������
/// </summary>
public class Inventory : MonoBehaviour
{
    public List<GameObject> weapons = new List<GameObject>();

    public int currentWeaponID;

    // Start is called before the first frame update
    void Start()
    {
        currentWeaponID = -1;
    }

    // Update is called once per frame
    void Update()
    {
        ChargeCurrentWeapons();
    }


    /// <summary>
    /// �����л�����
    /// ChargeCurrentWeapons()�������л����㺯��
    /// ChargeWeapon()��ʵ�������л�
    /// AddWeapon()���������
    /// ThrowWeapon()����������
    /// </summary>
    public void ChargeCurrentWeapons()
    {
        // -0.1 �£�0 ������ 0.1 ��
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //��һ������
            ChargeWeapon(currentWeaponID + 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //��һ������
            ChargeWeapon(currentWeaponID - 1);
        }

        //ͨ��С�����л�����
        for (int i = 0; i < 10; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int num = 0;
                if(i == 10)
                {
                    num = 10;
                }
                else
                {
                    num = i - 1;
                }

                if(num < weapons.Count)
                {
                    ChargeWeapon(num);
                }
            }
        }
    }

    public void ChargeWeapon(int weaponID)
    {
        if (weapons.Count == 0 || weaponID == currentWeaponID) return;

        if(weaponID > weapons.Max(weapons.IndexOf)) //����Ŵ�������������С��
        {
            weaponID = weapons.Min(weapons.IndexOf);
        }
        else if(weaponID < weapons.Min(weapons.IndexOf))//�����С����С����������
        {
            weaponID = weapons.Max(weapons.IndexOf);
        }

        currentWeaponID = weaponID; //������������

        for (int i = 0; i < weapons.Count; i++)
        {
            if(i == weaponID)
            {
                weapons[i].gameObject.SetActive(true);
            }
            else
            {
                weapons[i].gameObject.SetActive(false);
            }
        }
    }

    public void AddWeapon(GameObject weapon)
    {
        if (weapons.Contains(weapon))
        {
            print("���ڸ�����");
            return;
        }
        else
        {
            if(weapons.Count < 3)
            {
                weapons.Add(weapon);
                ChargeWeapon(currentWeaponID + 1); //��ʾ����
                weapon.gameObject.SetActive(true);
            }
        }
    }

    public void ThrowWeapon(GameObject weapon)
    {
        if (!weapons.Contains(weapon))
        {
            print("�����ڸ��������޷�����");
            return;
        }
        else
        {
            if (weapons.Count < 3)
            {
                weapons.Remove(weapon);
                ChargeWeapon(currentWeaponID - 1); //��ʾ����
                weapon.gameObject.SetActive(false);
            }
        }
    }
}

