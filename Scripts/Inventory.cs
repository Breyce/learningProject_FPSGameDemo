using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 人物的武器库，武器切换，添加，去除功能
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
    /// 武器切换函数
    /// ChargeCurrentWeapons()：武器切换顶层函数
    /// ChargeWeapon()：实现武器切换
    /// AddWeapon()：添加武器
    /// ThrowWeapon()：丢弃武器
    /// </summary>
    public void ChargeCurrentWeapons()
    {
        // -0.1 下，0 不动， 0.1 上
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //下一把武器
            ChargeWeapon(currentWeaponID + 1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //上一把武器
            ChargeWeapon(currentWeaponID - 1);
        }

        //通过小键盘切换武器
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

        if(weaponID > weapons.Max(weapons.IndexOf)) //若编号大于最大的则变成最小的
        {
            weaponID = weapons.Min(weapons.IndexOf);
        }
        else if(weaponID < weapons.Min(weapons.IndexOf))//若编号小于最小的则变成最大的
        {
            weaponID = weapons.Max(weapons.IndexOf);
        }

        currentWeaponID = weaponID; //更新武器索引

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
            print("存在该武器");
            return;
        }
        else
        {
            if(weapons.Count < 3)
            {
                weapons.Add(weapon);
                ChargeWeapon(currentWeaponID + 1); //显示武器
                weapon.gameObject.SetActive(true);
            }
        }
    }

    public void ThrowWeapon(GameObject weapon)
    {
        if (!weapons.Contains(weapon))
        {
            print("不存在该武器，无法抛弃");
            return;
        }
        else
        {
            if (weapons.Count < 3)
            {
                weapons.Remove(weapon);
                ChargeWeapon(currentWeaponID - 1); //显示武器
                weapon.gameObject.SetActive(false);
            }
        }
    }
}

