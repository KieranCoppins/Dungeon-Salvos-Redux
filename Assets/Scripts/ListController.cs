using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListController : MonoBehaviour {

    public GameObject listItemPrefab;
    public GameObject inventoryPanel;
    public Player playerScript;
    public Text inventoryTitle;


	public void UpdateInventory()
    {
        inventoryTitle.text = string.Format("Inventory {0}/{1}", playerScript.inventory.Count, playerScript.inventorySize);

        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        //Update Inventory
        if (playerScript.inventory.Count > 0)
        {
            int invIndex = 0;
            foreach (GameObject itemObject in playerScript.inventory)
            {
                Item item = itemObject.GetComponent<Item>();
                GameObject IP = Instantiate(listItemPrefab);
                ListItemController IC = IP.GetComponent<ListItemController>();
                IC.icon.sprite = item.icon;
                IC.name.text = item.name;
                IC.invIndex = invIndex;
                if (item.itemClass == Item.ItemClass.Armour)
                {
                    Item.ArmourType enumType = (Item.ArmourType)item.itemType;
                    string itemType = enumType.ToString();
                    IC.description.text = string.Format("Min Damage: {0}\nMax Damage: {1}\nDefence: {2}\nModifier: {3}\nSlot: {4}\n",
                        item.minDamage, item.maxDamage, item.defence, item.modifier, itemType);
                }
                else if (item.itemClass == Item.ItemClass.Weapon)
                {
                    Item.WeaponType enumType = (Item.WeaponType)item.itemType;
                    string itemType = enumType.ToString();
                    IC.description.text = string.Format("Min Damage: {0}\nMax Damage: {1}\nDefence: {2}\nModifier: {3}\nSlot: {4}\n",
                        item.minDamage, item.maxDamage, item.defence, item.modifier, itemType);
                }
                else
                {
                    Item.Consumeable enumType = (Item.Consumeable)item.itemType;
                    string itemType = enumType.ToString();
                    IC.description.text = string.Format("Min Damage: {0}\nMax Damage: {1}\nDefence: {2}\nModifier: {3}\nSlot: {4}\n",
                        item.minDamage, item.maxDamage, item.defence, item.modifier, itemType);
                }
                IP.transform.SetParent(inventoryPanel.transform);
                IP.transform.localScale = Vector3.one;
                invIndex++;
            }
        }
    }
}
