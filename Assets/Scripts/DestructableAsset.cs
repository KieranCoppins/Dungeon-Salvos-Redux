using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructableAsset : MonoBehaviour
{
    public GameObject player;
    public MapGeneration map;
    public int x;
    public int y;

    public int health = 25;

    void OnMouseUp()
    {
        Player PS = player.GetComponent<Player>();
        PS.GeneratePathTo(x, y);
        PS.target = this.gameObject;
        PS.playerPath.RemoveAt(PS.playerPath.Count - 1);
        Destroy(GameObject.FindGameObjectWithTag("Marker"));
        Instantiate(PS.redMarker, map.TileCoordToWorldCoord(x, y) + new Vector3(0, 0.2f, 0), Quaternion.identity);
    }

    void Update()
    {
        if (health <= 0)
        {
            player.GetComponent<Player>().target = null;
            Destroy(this.gameObject);
            map.assetPresent[x, y] = false;
            map.graph[x, y].movementCost = map.CostToEnterTile(x, y);
        }
    }
}
