using UnityEngine;
using System.Collections;

[System.Serializable]
public class Tile
{
    public string name;
    public bool walkable;
    public int type;
    public GameObject[] graphic;
    public int team;    //0 - Neutral, 1 - Blue, 2 - Red
    public bool partOfRoom = false;
}
