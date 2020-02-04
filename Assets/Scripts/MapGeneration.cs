using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MapGeneration : MonoBehaviour
{
    //Map Generation Variables
    public Tile[,] map;
    public bool[,] assetPresent;
    public int[,] walls;
    public Tile[] tileTypes;
    public List<Room> rooms;
    public List<GameObject> assets;
    public int mapWidth;
    public int mapHeight;
    public int minRoomSize;
    public int maxRoomSize;
    public int minRoomCount;
    public int maxRoomCount;
    public int tries;
    public int percentAreAssets;
    public int enemyRate;

    public GameObject player;
    public GameObject[] enemy;
    public GameObject[] items;
    public GameObject camera;

    public int itemRate;
    public int legendaryItemDropRate;
    public int masterworkItemDropRate;

    public int dungeonLevel;
    float enemyModifier = 0.6f;
    float itemModifier = 0.4f;

    public Node[,] graph;

    // Use this for initialization
    void Start()
    {
        CreateDungeon();
    }

    //Place the player randomly in the level
    void PlacePlayer()
    {
        int x = 0;
        int y = 0;
        GetRandomTile(out x, out y);
        player.transform.position = TileCoordToWorldCoord(x, y);
        Player playerScript = player.GetComponent<Player>();
        playerScript.posX = x;
        playerScript.posY = y;
        playerScript.target = null;
        playerScript.playerPath = null;        
    }

    void PlaceEnemies()
    {
        int x = 0;
        int y = 0;
        GetRandomTile(out x, out y);
        GameObject EO = Instantiate(enemy[Random.Range(0, enemy.Count())], TileCoordToWorldCoord(x, y), Quaternion.identity);
        Enemy ES = EO.GetComponent<Enemy>();
        ES.posX = x;
        ES.posY = y;
        ES.damage = Mathf.RoundToInt(ES.damage * (1 + (enemyModifier * dungeonLevel)));
        ES.maxHealth = Mathf.RoundToInt(ES.maxHealth * 1 + ((enemyModifier * dungeonLevel)));
        ES.health = ES.maxHealth;
        EO.transform.parent = this.transform;

    }

    void PlaceItem()
    {
        int x = 0;
        int y = 0;
        GetRandomTile(out x, out y);
        int itemType = Random.Range(1, 10);
        int itemIndex;
        if (itemType <= 4)
        {
            itemIndex = 0;
        }
        else if (itemType <= 8)
        {
            itemIndex = 6;
        }
        else
        {
            itemIndex = 3;
        }
        int itemRarity = Random.Range(0, 100);
        GameObject IO;
        Item IS;
        if (itemRarity <= masterworkItemDropRate)
        {
            IO = Instantiate(items[itemIndex + 2], TileCoordToWorldCoord(x, y), Quaternion.identity);
            IS = IO.GetComponent<Item>();
            IS.rarity = Item.Rarity.Masterwork;
        }
        else if (itemRarity <= legendaryItemDropRate)
        {
            IO = Instantiate(items[itemIndex + 1], TileCoordToWorldCoord(x, y), Quaternion.identity);
            IS = IO.GetComponent<Item>();
            IS.rarity = Item.Rarity.Legendary;
        }
        else
        {
            IO = Instantiate(items[itemIndex], TileCoordToWorldCoord(x, y), Quaternion.identity);
            IS = IO.GetComponent<Item>();
            IS.rarity = Item.Rarity.Basic;
        }
        IS.x = x;
        IS.y = y;
        IS.playerScript = player.GetComponent<Player>();
        IS.minDamage = Mathf.RoundToInt(IS.minDamage * (itemModifier * dungeonLevel));
        IS.maxDamage = Mathf.RoundToInt(IS.maxDamage * (itemModifier * dungeonLevel));
        IS.defence = Mathf.RoundToInt(IS.defence * (itemModifier * dungeonLevel));
        IS.modifier = Mathf.RoundToInt(IS.modifier * (itemModifier * dungeonLevel));
        if (IS.itemClass == Item.ItemClass.Weapon)
        {
            IS.itemType = Random.Range(0, 2);
        }
        else if (IS.itemClass == Item.ItemClass.Armour)
        {
            IS.itemType = Random.Range(0, 2);
        }
        else
        {
            //As there is only one item type at the moment
            IS.itemType = 0;
        }
        IO.transform.parent = this.transform;
    }

    void GetRandomTile(out int x, out int y)
    {
        while (true)
        {
            x = Random.Range(0, mapWidth);
            y = Random.Range(0, mapHeight);
            Tile tile = map[x, y];
            if (tile.type == 0 && assetPresent[x,y] == false)
            {
                break;
            }
        }
    }

    //Translate the tile coordinate to a world vector 3 coordinate
    public Vector3 TileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x * 5, 0, y * 5);
    }

    void CreatePathFindingGraph()
    {
        graph = new Node[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
                graph[x, y].neighbours = new List<Node>();
                graph[x, y].movementCost = CostToEnterTile(x, y);
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }

                if (x < mapWidth - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                if (y < mapHeight - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }

                if (x > 0 && y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                }

                if (x < mapWidth - 1 && y < mapHeight - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y + 1]);

                }

                if (x > 0 && y < mapHeight - 1)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y + 1]);

                }

                if (x < mapWidth - 1 && y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y - 1]);

                }
            }
        }
    }

    public bool UnitCanEnterTile(int x, int y)
    {
        if (map[x, y].walkable == true)
        {
            return true;
        }
        return false;
    }

    public float CostToEnterTile(int x, int y)
    {
        if (map[x, y].walkable == true && assetPresent[x,y] == false)
        {
            return 1;
        }
        else
        {
            return Mathf.Infinity;
        }
    }

    void CreateLadder()
    {
        int x = 0;
        int y = 0;
        GetRandomTile(out x, out y);
        int team = map[x, y].team;
        bool partOfRoom = map[x, y].partOfRoom;
        map[x, y] = tileTypes[13];
        map[x, y].team = team;
        map[x, y].partOfRoom = partOfRoom;
    }

    void AlignCamera()
    {
        camera.transform.position = new Vector3(player.transform.position.x - 10, 25, player.transform.position.z - 10);
    }

    //Create a dungeon
    public void CreateDungeon()
    {
        foreach (Transform child in this.transform)
        {
            if (child.gameObject.active == true)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        rooms = new List<Room>();
        map = new Tile[mapWidth, mapHeight];
        walls = new int[mapWidth, mapHeight];
        assetPresent = new bool[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                walls[x, y] = 0;
            }
        }
        FillMap(6);
        CreateRooms();
        ConnectCorridors();
        CreateLadder();
        DisplayMap();
        CreateAssets();
        CreatePathFindingGraph();
        PlacePlayer();
        AlignCamera();
        for (int i = 0; i < 100; i++)
        {
            //Place enemies
            int enemyChance = Random.Range(0, 100);
            if (enemyChance <= enemyRate)
            {
                PlaceEnemies();
            }

            int itemChance = Random.Range(0, 100);
            if (itemChance <= itemRate)
            {
                PlaceItem();
            }
                
        }
        dungeonLevel++;
    }

    //Fill the mao with a certain tile type
    void FillMap(int type)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                map[x, y] = tileTypes[type];
            }
        }
    }

    //Create the rooms in the dungeon
    void CreateRooms()
    {
        int roomCount = Random.Range(minRoomCount, maxRoomCount);
        for (int t = 0; t <= tries; t++)
        {
            for (int i = 0; i <= roomCount; i++)
            {
                if (rooms.Count() == maxRoomCount)
                {
                    break;
                }
                int roomWidth = Random.Range(minRoomSize, maxRoomSize) * 2;
                int roomHeight = Random.Range(minRoomSize, maxRoomSize) * 2;

                int posX = Random.Range(3, mapWidth - roomWidth - 3);
                int posY = Random.Range(3, mapHeight - roomHeight - 3);

                if (posX % 2 == 1)
                {
                    posX++;
                }

                if (posY % 2 == 1)
                {
                    posY++;
                }

                CreateRoom(roomWidth, roomHeight, posX, posY);
            }
        }

        if (rooms.Count() < minRoomCount)
        {
            Debug.LogError("Not enough rooms, perhaps try less rooms or a bigger map size?");
            //createDungeon();
        }
    }

    //Create an individual room with provided size and coords
    void CreateRoom(int sizeX, int sizeY, int posX, int posY)
    {
        Room newRoom = new Room();
        int randomTeam = Random.Range(1, 3);
        newRoom.team = randomTeam;
        newRoom.room = new List<Vector2>();
        newRoom.posX = posX;
        newRoom.posY = posY;
        newRoom.sizeX = sizeX;
        newRoom.sizeY = sizeY;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                newRoom.room.Add(new Vector2(x + posX, y + posY));
            }
        }
        if (!RoomIntersects(newRoom))
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    map[x + posX, y + posY] = tileTypes[newRoom.team];
                    map[x + posX, y + posY].partOfRoom = true;
                }
            }
            rooms.Add(newRoom);
        }
    }

    //Check if a troom intersects with other existing rooms
    bool RoomIntersects(Room room)
    {
        foreach (Room otherRoom in rooms)
        {
            if (room.room.Intersect(otherRoom.room).Any())
            {
                return true;
            }

            if ((room.posX < otherRoom.posX + otherRoom.sizeX + 1 && room.posX > otherRoom.posX - room.sizeX - 1) || (room.posY < otherRoom.posY + otherRoom.sizeY + 1 && room.posY > otherRoom.posY - room.sizeX - 1))
            {
                return true;
            }
        }

        return false;
    }

    //Create corridors between rooms
    void ConnectCorridors()
    {
        foreach (Room room in rooms)
        {
            if (room.connected == false)
            {
                foreach (Room otherRoom in rooms)
                {
                    if (otherRoom != room)
                    {
                        GenerateDoors(room, otherRoom);
                        GenerateCorridors(room, otherRoom);
                        otherRoom.connected = true;
                        room.connected = true;
                        break;
                    }
                }
            }
        }
    }

    //Create door ways
    void GenerateDoors(Room roomA, Room roomB)
    {
        int doorAX = roomA.posX;
        int doorAY = roomA.posY;
        int doorBX = roomB.posX;
        int doorBY = roomB.posY;

        if (doorBX <= doorAX && doorBY >= doorAY)
        {
            if (roomA.doorSide != 0)
            {
                //Left Side A
                int randomPlace = Random.Range(0, roomA.sizeY / 2);
                roomA.doorCoordX = doorAX - 2;
                roomA.doorCoordY = (randomPlace * 2) + doorAY;
                roomA.doorSide = 0;

                map[roomA.doorCoordX + 1, roomA.doorCoordY] = tileTypes[0];
                map[roomA.doorCoordX, roomA.doorCoordY] = tileTypes[0];
                map[roomA.doorCoordX + 1, roomA.doorCoordY + 1] = tileTypes[0];
                map[roomA.doorCoordX, roomA.doorCoordY + 1] = tileTypes[0];
            }

            if (roomB.doorSide != 3)
            {
                //Bottom Side B
                int randomPlace = Random.Range(0, roomB.sizeX / 2);
                roomB.doorCoordX = (randomPlace * 2) + doorBX;
                roomB.doorCoordY = doorBY - 2;
                roomB.doorSide = 3;

                map[roomB.doorCoordX, roomB.doorCoordY + 1] = tileTypes[0];
                map[roomB.doorCoordX + 1, roomB.doorCoordY + 1] = tileTypes[0];
                map[roomB.doorCoordX, roomB.doorCoordY] = tileTypes[0];
                map[roomB.doorCoordX + 1, roomB.doorCoordY] = tileTypes[0];
            }

        }

        else if (doorBX >= doorAX && doorBY >= doorAY)
        {
            if (roomA.doorSide != 1)
            {
                //Top Side A
                int randomPlace = Random.Range(0, roomA.sizeX / 2);
                roomA.doorCoordX = (randomPlace * 2) + doorAX;
                roomA.doorCoordY = doorAY + roomA.sizeY + 1;
                roomA.doorSide = 1;

                map[roomA.doorCoordX, roomA.doorCoordY - 1] = tileTypes[0];
                map[roomA.doorCoordX + 1, roomA.doorCoordY - 1] = tileTypes[0];
                map[roomA.doorCoordX, roomA.doorCoordY] = tileTypes[0];
                map[roomA.doorCoordX + 1, roomA.doorCoordY] = tileTypes[0];
            }

            if (roomB.doorSide != 0)
            {
                //Left Side B
                int randomPlace = Random.Range(0, roomB.sizeY / 2);
                roomB.doorCoordX = doorBX - 2;
                roomB.doorCoordY = (randomPlace * 2) + doorBY;
                roomB.doorSide = 0;

                map[roomB.doorCoordX + 1, roomB.doorCoordY] = tileTypes[0];
                map[roomB.doorCoordX, roomB.doorCoordY] = tileTypes[0];
                map[roomB.doorCoordX + 1, roomB.doorCoordY + 1] = tileTypes[0];
                map[roomB.doorCoordX, roomB.doorCoordY + 1] = tileTypes[0];
            }
        }

        else if (doorBX >= doorAX && doorBY <= doorAY)
        {

            if (roomA.doorSide != 2)
            {
                //Right Side A
                int randomPlace = Random.Range(0, roomA.sizeY / 2);
                roomA.doorCoordX = doorAX + roomA.sizeX + 1;
                roomA.doorCoordY = (randomPlace * 2) + doorAY;
                roomA.doorSide = 2;

                map[roomA.doorCoordX - 1, roomA.doorCoordY] = tileTypes[0];
                map[roomA.doorCoordX, roomA.doorCoordY] = tileTypes[0];
                map[roomA.doorCoordX - 1, roomA.doorCoordY + 1] = tileTypes[0];
                map[roomA.doorCoordX, roomA.doorCoordY + 1] = tileTypes[0];
            }

            if (roomB.doorSide != 1)
            {
                //Top Size B
                int randomPlace = Random.Range(0, roomB.sizeX / 2);
                roomB.doorCoordX = (randomPlace * 2) + doorBX;
                roomB.doorCoordY = doorBY + roomB.sizeY + 1;
                roomB.doorSide = 1;

                map[roomB.doorCoordX, roomB.doorCoordY - 1] = tileTypes[0];
                map[roomB.doorCoordX + 1, roomB.doorCoordY - 1] = tileTypes[0];
                map[roomB.doorCoordX, roomB.doorCoordY] = tileTypes[0];
                map[roomB.doorCoordX + 1, roomB.doorCoordY] = tileTypes[0];
            }
        }

        else if (doorBX <= doorAX && doorBY <= doorAY)
        {

            if (roomA.doorSide != 3)
            {
                //Bottom Side A
                int randomPlace = Random.Range(0, roomA.sizeX / 2);
                roomA.doorCoordX = (randomPlace * 2) + doorAX;
                roomA.doorCoordY = doorAY - 2;
                roomA.doorSide = 3;


                map[roomA.doorCoordX, roomA.doorCoordY + 1] = tileTypes[0];
                map[roomA.doorCoordX + 1, roomA.doorCoordY + 1] = tileTypes[0];
                map[roomA.doorCoordX, roomA.doorCoordY] = tileTypes[0];
                map[roomA.doorCoordX + 1, roomA.doorCoordY] = tileTypes[0];
            }

            if (roomB.doorSide != 2)
            {
                //Right Side B
                int randomPlace = Random.Range(0, roomB.sizeY / 2);
                roomB.doorCoordX = doorBX + roomB.sizeX + 1;
                roomB.doorCoordY = (randomPlace * 2) + doorBY;
                roomB.doorSide = 2;

                map[roomB.doorCoordX - 1, roomB.doorCoordY] = tileTypes[0];
                map[roomB.doorCoordX, roomB.doorCoordY] = tileTypes[0];
                map[roomB.doorCoordX - 1, roomB.doorCoordY + 1] = tileTypes[0];
                map[roomB.doorCoordX, roomB.doorCoordY + 1] = tileTypes[0];
            }
        }
    }

    //Create corridors based on room positions
    void GenerateCorridors(Room roomA, Room roomB)
    {
        int doorAX = roomA.doorCoordX;
        int doorAY = roomA.doorCoordY;
        int doorBX = roomB.doorCoordX;
        int doorBY = roomB.doorCoordY;

        if (doorBX <= doorAX && doorBY >= doorAY)
        {
            for (int x = doorBX; x <= doorAX; x++)
            {
                if (map[x, doorAY].type == 0)
                {
                    continue;
                }
                map[x, doorAY] = tileTypes[0];
                map[x, doorAY + 1] = tileTypes[0];

            }

            for (int y = doorAY; y <= doorBY; y++)
            {
                if (map[doorBX, y].type == 0)
                {
                    continue;
                }
                map[doorBX, y] = tileTypes[0];
                map[doorBX + 1, y] = tileTypes[0];

            }

            map[doorBX + 1, doorAY + 1] = tileTypes[0];
            map[doorBX + 1, doorAY] = tileTypes[0];
        }

        else if (doorBX >= doorAX && doorBY >= doorAY)
        {
            for (int x = doorAX; x <= doorBX; x++)
            {
                if (map[x, doorBY].type == 0)
                {
                    continue;
                }
                map[x, doorBY] = tileTypes[0];
                map[x, doorBY + 1] = tileTypes[0];

            }

            for (int y = doorAY; y <= doorBY; y++)
            {
                if (map[doorAX, y].type == 0)
                {
                    continue;
                }
                map[doorAX, y] = tileTypes[0];
                map[doorAX + 1, y] = tileTypes[0];

            }

            map[doorAX + 1, doorBY + 1] = tileTypes[0];
            map[doorAX + 1, doorBY] = tileTypes[0];
        }

        else if (doorBX >= doorAX && doorBY <= doorAY)
        {
            for (int x = doorAX; x <= doorBX; x++)
            {
                if (map[x, doorAY].type == 0)
                {
                    continue;
                }
                map[x, doorAY] = tileTypes[0];
                map[x, doorAY + 1] = tileTypes[0];
            }

            for (int y = doorBY; y <= doorAY; y++)
            {
                if (map[doorBX, y].type == 0)
                {
                    continue;
                }
                map[doorBX, y] = tileTypes[0];
                map[doorBX + 1, y] = tileTypes[0];
            }

            map[doorBX + 1, doorAY + 1] = tileTypes[0];
            map[doorBX + 1, doorAY] = tileTypes[0];
        }

        else if (doorBX <= doorAX && doorBY <= doorAY)
        {
            for (int x = doorBX; x <= doorAX; x++)
            {
                if (map[x, doorBY].type == 0)
                {
                    continue;
                }
                map[x, doorBY] = tileTypes[0];
                map[x, doorBY + 1] = tileTypes[0];
            }

            for (int y = doorBY; y <= doorAY; y++)
            {
                if (map[doorAX, y].type == 0)
                {
                    continue;
                }
                map[doorAX, y] = tileTypes[0];
                map[doorAX + 1, y] = tileTypes[0];
            }

            map[doorAX + 1, doorBY + 1] = tileTypes[0];
            map[doorAX + 1, doorBY] = tileTypes[0];
        }

    }

    //Create walls around the floors
    void CreateWall(float x, float y, int orientation, int team)
    {
        if (orientation == 0)
        {
            int randomTile = Random.Range(0, 2);
            Tile wallTile = tileTypes[3 + team];
            GameObject GO = (GameObject)Instantiate(wallTile.graphic[randomTile], new Vector3(x, 0, y), Quaternion.identity);
            GO.transform.parent = this.transform;
            GO.transform.Rotate(new Vector3(-90, 0, 0));
        }
        else
        {
            int randomTile = Random.Range(0, 2);
            Tile wallTile = tileTypes[3 + team];
            GameObject GO = (GameObject)Instantiate(wallTile.graphic[randomTile], new Vector3(x, 0, y), Quaternion.identity);
            GO.transform.parent = this.transform;
            GO.transform.Rotate(new Vector3(-90, 90, 0));
        }
    }

    //Place door ways
    void createDoorObject(float x, float y, int orientation, int team)
    {
        if (orientation == 0)
        {
            Tile doorTile = tileTypes[7 + team];
            GameObject GO = (GameObject)Instantiate(doorTile.graphic[0], new Vector3(x, 0, y), Quaternion.identity);
            GO.transform.parent = this.transform;
            GO.transform.Rotate(new Vector3(-90, 0, 0));
        }
        else
        {
            Tile wallTile = tileTypes[7 + team];
            GameObject GO = (GameObject)Instantiate(wallTile.graphic[0], new Vector3(x, 0, y), Quaternion.identity);
            GO.transform.parent = this.transform;
            GO.transform.Rotate(new Vector3(-90, 90, 0));
        }
    }

    //Create pillars in required positions
    void createPillar(float x, float y, int team)
    {
        Tile pillarTile = tileTypes[10 + team];
        GameObject GO = (GameObject)Instantiate(pillarTile.graphic[0], new Vector3(x, 0, y), Quaternion.identity);
        GO.transform.parent = this.transform;
        GO.transform.Rotate(new Vector3(-90, 0, 0));
    }

    //Instantiate all game objects in the world
    void DisplayMap()
    {

        //Place all floors needed for the level
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {

                if (IsWall(x, y))
                {
                    map[x, y].walkable = false;
                }

                if (map[x, y].type == 0)
                {
                    GameObject GO;
                    if (map[x,y].name == "Ladder")
                    {
                        GO = (GameObject)Instantiate(map[x, y].graphic[0], new Vector3(x * 5, 0, y * 5), Quaternion.identity);
                    }
                    else
                    {
                        int randomTile = Random.Range(0, 4);
                        GO = (GameObject)Instantiate(map[x, y].graphic[randomTile], new Vector3(x * 5, 0, y * 5), Quaternion.identity);
                    }
                    GO.transform.parent = this.transform;
                    int randomRotation = Random.Range(0, 3);
                    GO.transform.Rotate(new Vector3(-90, 90 * randomRotation, 0));

                    ClickableTile ct = GO.GetComponent<ClickableTile>();
                    ct.x = x;
                    ct.y = y;
                    ct.player = player.GetComponent<Player>();

                    if (x % 2 == 0)
                    {
                        if (map[x + 1, y].type == 0)
                        {
                            if (IsWall(x, y + 1))
                            {
                                CreateWall((x * 5) + 2.5f, (y * 5) + 2.5f, 0, map[x, y].team);
                            }

                            else if (IsWall(x, y - 1))
                            {
                                CreateWall((x * 5) + 2.5f, (y * 5) - 2.5f, 0, map[x, y].team);
                            }
                            else if (map[x, y].partOfRoom == true && map[x, y + 1].partOfRoom == false)
                            {
                                createDoorObject((x * 5) + 2.5f, (y * 5) + 2.5f, 0, map[x, y].team);
                            }

                            else if (map[x, y].partOfRoom == true && map[x, y - 1].partOfRoom == false)
                            {
                                createDoorObject((x * 5) + 2.5f, (y * 5) - 2.5f, 0, map[x, y].team);
                            }
                        }
                    }

                    if (y % 2 == 0)
                    {
                        if (map[x, y + 1].type == 0)
                        {
                            if (IsWall(x + 1, y))
                            {
                                CreateWall((x * 5) + 2.5f, (y * 5) + 2.5f, 1, map[x, y].team);
                            }

                            else if (IsWall(x - 1, y))
                            {
                                CreateWall((x * 5) - 2.5f, (y * 5) + 2.5f, 1, map[x, y].team);
                            }
                            else if (map[x, y].partOfRoom == true && map[x + 1, y].partOfRoom == false)
                            {
                                createDoorObject((x * 5) + 2.5f, (y * 5) + 2.5f, 1, map[x, y].team);
                            }

                            else if (map[x, y].partOfRoom == true && map[x - 1, y].partOfRoom == false)
                            {
                                createDoorObject((x * 5) - 2.5f, (y * 5) + 2.5f, 1, map[x, y].team);
                            }
                        }
                    }
                    if (map[x, y].partOfRoom == true)
                    {
                        if (map[x + 1, y + 1].partOfRoom == false && map[x + 1, y].partOfRoom == false && map[x, y + 1].partOfRoom == false)
                        {
                            createPillar((x * 5) + 2.5f, (y * 5) + 2.5f, map[x, y].team);
                        }

                        else if (map[x - 1, y + 1].partOfRoom == false && map[x - 1, y].partOfRoom == false && map[x, y + 1].partOfRoom == false)
                        {
                            createPillar((x * 5) - 2.5f, (y * 5) + 2.5f, map[x, y].team);
                        }

                        else if (map[x + 1, y - 1].partOfRoom == false && map[x + 1, y].partOfRoom == false && map[x, y - 1].partOfRoom == false)
                        {
                            createPillar((x * 5) + 2.5f, (y * 5) - 2.5f, map[x, y].team);
                        }

                        else if (map[x - 1, y - 1].partOfRoom == false && map[x - 1, y].partOfRoom == false && map[x, y - 1].partOfRoom == false)
                        {
                            createPillar((x * 5) - 2.5f, (y * 5) - 2.5f, map[x, y].team);
                        }
                    }

                    if (map[x + 1, y + 1].type != 0 && map[x, y + 1].type != 0 && map[x + 1, y].type != 0)
                    {
                        createPillar((x * 5) + 2.5f, (y * 5) + 2.5f, map[x, y].team);
                    }

                    else if (map[x - 1, y + 1].type != 0 && map[x, y + 1].type != 0 && map[x - 1, y].type != 0)
                    {
                        createPillar((x * 5) - 2.5f, (y * 5) + 2.5f, map[x, y].team);
                    }

                    else if (map[x + 1, y - 1].type != 0 && map[x, y - 1].type != 0 && map[x + 1, y].type != 0)
                    {
                        createPillar((x * 5) + 2.5f, (y * 5) - 2.5f, map[x, y].team);
                    }

                    else if (map[x - 1, y - 1].type != 0 && map[x, y - 1].type != 0 && map[x - 1, y].type != 0)
                    {
                        createPillar((x * 5) - 2.5f, (y * 5) - 2.5f, map[x, y].team);
                    }
                }

                if (map[x, y].type == 999999)
                {

                    if (!IsWall(x + 1, y + 1) && !IsWall(x, y + 1) && !IsWall(x + 1, y))
                    {
                        createPillar((x * 5) + 2.5f, (y * 5) + 2.5f, map[x, y].team);
                    }

                    else if (!IsWall(x - 1, y + 1) && !IsWall(x, y + 1) && !IsWall(x - 1, y))
                    {
                        createPillar((x * 5) - 2.5f, (y * 5) + 2.5f, map[x, y].team);
                    }

                    else if (!IsWall(x + 1, y - 1) && !IsWall(x, y - 1) && !IsWall(x + 1, y))
                    {
                        createPillar((x * 5) + 2.5f, (y * 5) - 2.5f, map[x, y].team);
                    }

                    else if (!IsWall(x - 1, y - 1) && !IsWall(x, y - 1) && !IsWall(x - 1, y))
                    {
                        createPillar((x * 5) - 2.5f, (y * 5) - 2.5f, map[x, y].team);
                    }
                }

                if (map[x, y].type == 4)
                {
                    GameObject GO = (GameObject)Instantiate(map[x, y].graphic[0], new Vector3(x * 5, 0, y * 5), Quaternion.identity);
                    GO.transform.parent = this.transform;
                    int randomRotation = Random.Range(0, 3);
                    GO.transform.Rotate(new Vector3(-90, 90 * randomRotation, 0));

                    ClickableTile ct = GO.GetComponent<ClickableTile>();
                    ct.x = x;
                    ct.y = y;
                    ct.player = player.GetComponent<Player>();
                }
            }
        }
        //Check each floor tile and its neighbouring tiles and deside if a wall is required

    }

    //Check if the coordinate is a wall (concider out of bounds as a wall as well)
    bool IsWall(int x, int y)
    {
        if (x < 0)
        {
            return true;
        }

        if (y < 0)
        {
            return true;
        }

        if (x >= mapWidth)
        {
            return true;
        }

        if (y >= mapHeight)
        {
            return true;
        }

        if (map[x, y].type == 999999)
        {
            return true;
        }

        return false;
    }

    //Place assets throughout the world
    void CreateAssets()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                assetPresent[x, y] = false;
                if (map[x, y].type == 0)
                {
                    int chance = Random.Range(0, 101);

                    if (chance < percentAreAssets)
                    {
                        GameObject randomAsset = (GameObject)Instantiate(assets[Random.Range(0, assets.Count())], new Vector3(x * 5, 0, y * 5), Quaternion.identity);
                        randomAsset.transform.parent = this.transform;
                        randomAsset.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
                        DestructableAsset DA = randomAsset.GetComponent<DestructableAsset>();
                        DA.x = x;
                        DA.y = y;
                        DA.map = this;
                        DA.player = player;


                        assetPresent[x, y] = true;
                    }
                }

            }
        }
    }
}
