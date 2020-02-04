using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class Room
{
    public List<Vector2> room;
    public bool connected = false;
    public int doorCoordX;
    public int doorCoordY;
    public int doorSide = 99999;
    public int team;
    public int posX;
    public int posY;
    public int sizeX;
    public int sizeY;
}
