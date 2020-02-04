using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    public int x;
    public int y;
    public Player playerScript;
    public Sprite icon;
    public string name;

    public enum ItemClass
    {
        Weapon, Armour, Consumable
    }
    public enum WeaponType
    {
        Sword, Spear, Symitar
    }
    public enum ArmourType
    {
        Head, Chest, Legs
    }
    //Allows for extra consumables to be added in the future
    public enum Consumeable
    {
        Health
    }
    public enum Rarity
    {
        Basic, Legendary, Masterwork
    }
    public ItemClass itemClass;
    public Rarity rarity;
    public int itemType;

    public int minDamage;
    public int maxDamage;

    public int defence;

    public int modifier;

    void OnMouseUp()
    {
        playerScript.GeneratePathTo(x, y);
        playerScript.target = this.gameObject;
        Destroy(GameObject.FindGameObjectWithTag("Marker"));
        Instantiate(playerScript.greenMarker, playerScript.mapScript.TileCoordToWorldCoord(x, y) + new Vector3(0, 0.2f, 0), Quaternion.identity);
    }


    void OnMouseOver()
    {
        //Create UI popup of item
    }

    void Start()
    {
        if (itemClass == ItemClass.Weapon)
        {
            if (itemType == (int)WeaponType.Spear)
            {
                minDamage = 3;
                maxDamage = 4;
            }

            if(itemType == (int)WeaponType.Sword)
            {
                minDamage = 2;
                maxDamage = 6;
            }
            if (itemType == (int)WeaponType.Symitar)
            {
                minDamage = 1;
                maxDamage = 8;
            }

            if (rarity == Rarity.Legendary)
            {
                minDamage = minDamage * 2;
                maxDamage = maxDamage * 2;
            }
            else if (rarity == Rarity.Masterwork)
            {
                minDamage = minDamage * 3;
                maxDamage = maxDamage * 3;
            }

        }

        if (itemClass == ItemClass.Armour)
        {
            if (itemType == (int)ArmourType.Head)
            {
                defence = 1;
            }

            if (itemType == (int)ArmourType.Chest)
            {
                defence = 5;
            }
            if (itemType == (int)ArmourType.Legs)
            {
                defence = 3;
            }

            if (rarity == Rarity.Legendary)
            {
                defence = defence * 2;
            }
            else if (rarity == Rarity.Masterwork)
            {
                defence = defence * 3;
            }
        }

        if (itemClass == ItemClass.Consumable)
        {
            modifier = 10;

            if (rarity == Rarity.Legendary)
            {
                modifier = modifier * 2;
            }
            else if (rarity == Rarity.Masterwork)
            {
                modifier = modifier * 3;
            }
        }
    }
}
