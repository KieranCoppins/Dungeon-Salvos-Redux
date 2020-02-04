using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour {

    public int x;
    public int y;
    public Player player;
    public MapGeneration map;

    void Start()
    {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<MapGeneration>();
    }

    void OnMouseUp()
    {
        player.GeneratePathTo(x, y);
        Destroy(GameObject.FindGameObjectWithTag("Marker"));
        Instantiate(player.greenMarker, map.TileCoordToWorldCoord(x, y) + new Vector3(0, 0.2f, 0), Quaternion.identity);
        player.target = this.gameObject;
    }
}
