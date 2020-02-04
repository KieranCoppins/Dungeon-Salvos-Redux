using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public MapGeneration mapScript;
    public GameObject popUpText;

    public int posX;
    public int posY;

    public float movementSpeed;
    public float rotationSpeed;

    float remainingMovement = 1;

    float timer = 1;

    public List<Node> playerPath = null;

    public GameObject target;
    public float maxHealth = 100;
    public float health = 100;
    public int minDamage = 0;
    public int maxDamage = 0;
    public int defence = 0;
    public Text desc;
    public Image headImage;
    public Image chestImage;
    public Image legsImage;
    public Image weaponImage;

    public float attackCooldown = 5;
    float attackTimer = 5;

    public string name;
    public Slider healthBar;
    public Text healthText;
    public Slider skillCooldown;
    public Text skillText;

    public Dictionary<string, Item> armour;
    public Item weapon;

    public List<GameObject> inventory = new List<GameObject>();
    public int inventorySize;
    public ListController inventoryController;

    public GameObject redMarker;
    public GameObject greenMarker;
    public GameObject gameOverScene;

    public AudioSource pickUpSound;

    void Start()
    {
        attackTimer = attackCooldown;
        armour = new Dictionary<string, Item>();
        //inventory = new List<GameObject>();
        armour.Add("head", null);
        armour.Add("chest", null);
        armour.Add("legs", null);
        weapon = null;
    }

    void Update()
    {
        if (health <= 0)
        {
            this.gameObject.SetActive(false);
            gameOverScene.SetActive(true);
        }
        DebugLineController();
        MovementController();
        UpdateStats();
        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.transform.position) <= 10f)
            {
                if (target.tag == "Enemy" || target.tag == "Asset")
                {


                    if (attackTimer < 0)
                    {
                        CombatController();
                        attackTimer = attackCooldown;
                    }
                    else
                    {
                        attackTimer -= Time.deltaTime;
                    }
                }

                else if (target.tag == "Item")
                {
                    if (inventory.Count < inventorySize)
                    {
                        PickUp(target);
                        //Destroy(target);
                        target.SetActive(false);
                        target = null;
                        inventoryController.UpdateInventory();
                    }
                    else
                    {
                        GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                            Quaternion.identity);
                            TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                        TM.text = "Inventory too full!";
                        TM.color = Color.red;
                        target = null;
                    }
                }
                else if (target.tag == "Ladder")
                {
                    mapScript.CreateDungeon();
                    Debug.Log("At ladder");
                }
            }
        }
        else
        {
            attackTimer = attackCooldown;
        }
        UIController();
    }

    public void DestroyItem(int inventoryIndex)
    {
        inventory.RemoveAt(inventoryIndex);
        inventoryController.UpdateInventory();
    }

    public void EquiptItem(int inventoryIndex)
    {
        GameObject itemObject = this.inventory[inventoryIndex];
        Item item = itemObject.GetComponent<Item>();
        inventory.RemoveAt(inventoryIndex);
        if (item.itemClass == Item.ItemClass.Weapon)
        {
            weapon = item;
        }
        else if (item.itemClass == Item.ItemClass.Armour)
        {
            if (item.itemType == (int)Item.ArmourType.Head)
            {
                armour["head"] = item;
            }
            else if (item.itemType == (int)Item.ArmourType.Chest)
            {
                armour["chest"] = item;
            }
            else if (item.itemType == (int)Item.ArmourType.Legs)
            {
                armour["legs"] = item;
            }
        }
        else if (item.itemClass == Item.ItemClass.Consumable)
        {
            if (item.itemType == (int)Item.Consumeable.Health)
            {
                health += item.modifier;
                if (health > maxHealth)
                {
                    health = maxHealth;
                }
            }
        }
        inventoryController.UpdateInventory();  
    }

    void UpdateStats()
    {
        if (weapon != null)
        {
            minDamage = weapon.minDamage;
            maxDamage = weapon.maxDamage;
            weaponImage.sprite = weapon.GetComponent<Item>().icon;
        }
        defence = 0;
        if (armour["head"] != null)
        {
            defence += armour["head"].defence;
            headImage.sprite = armour["head"].GetComponent<Item>().icon;
        }
        if (armour["chest"] != null)
        {
            defence += armour["chest"].defence;
            chestImage.sprite = armour["chest"].GetComponent<Item>().icon;

        }
        if (armour["legs"] != null)
        {
            defence += armour["legs"].defence;
            legsImage.sprite = armour["legs"].GetComponent<Item>().icon;
        }

        desc.text = string.Format("Min Damage: {0}\nMax Damage: {1}\nDefence: {2}\nDungeon Level: {3}", 
            minDamage, maxDamage, defence, mapScript.dungeonLevel - 1);
    }

    public void PickUp(GameObject itemObject)
    {
        pickUpSound.Play();
        Item item = itemObject.GetComponent<Item>();
        if (item.itemClass == Item.ItemClass.Armour)
        {
            if (item.itemType == (int)Item.ArmourType.Head)
            {
                if (armour["head"] == null)
                {
                    armour["head"] = item;
                    GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                        Quaternion.identity);
                    TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                    TM.text = "Equipted!";
                    TM.color = Color.white;
                }
                else
                {
                    inventory.Add(itemObject);
                    GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                        Quaternion.identity);
                    TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                    TM.text = "Picked Up!";
                    TM.color = Color.white;
                }
            }
            else if (item.itemType == (int)Item.ArmourType.Chest)
            {
                if (armour["chest"] == null)
                {
                    armour["chest"] = item; GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                        Quaternion.identity);
                    TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                    TM.text = "Equipted!";
                    TM.color = Color.white;
                }
                else
                {
                    inventory.Add(itemObject);
                    GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                        Quaternion.identity);
                    TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                    TM.text = "Picked Up!";
                    TM.color = Color.white;
                }
            }
            else
            {
                if (armour["legs"] == null)
                {
                    armour["legs"] = item;
                    GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                        Quaternion.identity);
                    TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                    TM.text = "Equipted!";
                    TM.color = Color.white;
                }
                else
                {
                    inventory.Add(itemObject);
                    GameObject GO = (GameObject)Instantiate(popUpText,
                        new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                        Quaternion.identity);
                    TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                    TM.text = "Picked Up!";
                    TM.color = Color.white;
                }
            }
        }


        else if (item.itemClass == Item.ItemClass.Weapon)
        {
            if (weapon == null)
            {
                weapon = item;
                GameObject GO = (GameObject)Instantiate(popUpText,
                    new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                    Quaternion.identity);
                TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                TM.text = "Equipted!";
                TM.color = Color.white;
            }
            else
            {
                inventory.Add(itemObject);
                GameObject GO = (GameObject)Instantiate(popUpText,
                    new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                    Quaternion.identity);
                TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                TM.text = "Picked Up!";
                TM.color = Color.white;
            }
        }
        else
        {
            inventory.Add(itemObject);
            GameObject GO = (GameObject)Instantiate(popUpText,
                new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                Quaternion.identity);
            TextMesh TM = GO.GetComponentInChildren<TextMesh>();
            TM.text = "Picked Up!";
            TM.color = Color.white;
        }
    }

    void UIController()
    {
        float healthBarValue = health / maxHealth;
        healthBar.value = healthBarValue;
        string healthTextValue = health.ToString() + "/" + maxHealth.ToString();
        healthText.text = healthTextValue;
        float attackTimerValue = attackTimer / attackCooldown;
        skillCooldown.value = attackTimerValue;
        string skillTextValue = attackTimer.ToString() + "S";
        skillText.text = skillTextValue;

        
    }

    void CombatController()
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= 10f)
        {
            int damage = Random.Range(minDamage, maxDamage);

            if (target.tag == "Asset")
            {
                DestructableAsset DA = target.GetComponent<DestructableAsset>();
                DA.health -= damage;
                GameObject GO = (GameObject)Instantiate(popUpText, 
                    new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z), 
                    Quaternion.identity);
                TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                TM.text = damage.ToString();
                TM.color = Color.red;
            }

            else if (target.tag == "Enemy")
            {
                Enemy ES = target.GetComponent<Enemy>();
                ES.health -= damage;
                GameObject GO = (GameObject)Instantiate(popUpText,
                    new Vector3(target.transform.position.x, target.transform.position.y + 5, target.transform.position.z),
                    Quaternion.identity);
                TextMesh TM = GO.GetComponentInChildren<TextMesh>();
                TM.text = damage.ToString();
                TM.color = Color.red;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = damage - defence;
        if (damageTaken <= 0)
        {
            damageTaken = 0;
        }
        health -= damageTaken;
        GameObject GO = (GameObject)Instantiate(popUpText, 
            new Vector3(transform.position.x, transform.position.y + 5, transform.position.z), 
            Quaternion.identity);
        TextMesh TM = GO.GetComponentInChildren<TextMesh>();
        TM.text = damageTaken.ToString();
        TM.color = Color.blue;
    }

    void DebugLineController()
    {
        if (playerPath != null)
        {
            int currNode = 0;

            while (currNode < playerPath.Count - 1)
            {
                Vector3 start = mapScript.TileCoordToWorldCoord(playerPath[currNode].x, playerPath[currNode].y);
                Vector3 end = mapScript.TileCoordToWorldCoord(playerPath[currNode + 1].x, playerPath[currNode + 1].y);

                Debug.DrawLine(start, end, Color.green);

                currNode++;
            }
        }
    }

    void MovementController()
    {
        if (Vector3.Distance(transform.position, mapScript.TileCoordToWorldCoord(posX, posY)) < 0.1f)
        {
            MoveTextTile();
        }
        Vector3 direction = (mapScript.TileCoordToWorldCoord(posX, posY) - transform.position).normalized;
        if (direction == new Vector3(0, 0, 0))
        {
            if (target != null)
            {
                direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        //transform.position = Vector3.Lerp(transform.position, mapScript.TileCoordToWorldCoord(posX, posY), movementSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, mapScript.TileCoordToWorldCoord(posX, posY), movementSpeed * Time.deltaTime);
        /*
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            remainingMovement = 1;
            timer = 1;
        }
        */
        remainingMovement = 1;
    }

    void MoveTextTile()
    {
        while (remainingMovement > 0)
        {
            if (playerPath == null)
            {
                return;
            }


            transform.position = mapScript.TileCoordToWorldCoord(posX, posY);

            remainingMovement -= mapScript.CostToEnterTile(playerPath[0].x, playerPath[0].y);

            posX = playerPath[1].x;
            posY = playerPath[1].y;

            playerPath.RemoveAt(0);

            if (playerPath.Count == 1)
            {
                playerPath = null;
            }
        }
    }

    public void GeneratePathTo(int x, int y)
    {
        if (mapScript.UnitCanEnterTile(x, y) == false)
        {
            return;
        }

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        List<Node> unvisited = new List<Node>();

        Node source = mapScript.graph[posX, posY];
        Node target = mapScript.graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        foreach (Node v in mapScript.graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }
            unvisited.Add(v);
        }

        while(unvisited.Count > 0)
        {
            Node u = null;

            foreach(Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach(Node v in u.neighbours)
            {
                float alt = dist[u] + mapScript.graph[u.x, u.y].movementCost;
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }
        }

        if (prev[target] == null)
        {
            return;
        }

        List<Node> currentPath = new List<Node>();

        Node curr = target;

        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        currentPath.Reverse();
        playerPath = currentPath;
    }	
}
