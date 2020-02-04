using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ListItemController : MonoBehaviour {

    public Image icon;
    public Text name;
    public Text description;
    public int invIndex;
    public Player playerScript;

    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public void Equipt()
    {
        playerScript.EquiptItem(invIndex);
    }

    public void DestroyItem()
    {
        playerScript.DestroyItem(invIndex);
    }
}
