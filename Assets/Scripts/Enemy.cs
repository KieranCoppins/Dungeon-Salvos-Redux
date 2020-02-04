using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    public MapGeneration mapScript;
    public Player playerScript;

    public int posX;
    public int posY;

    public float movementSpeed;
    public float rotationSpeed;
    
    float remainingMovement = 1;
    float timer = 1;

    public List<Node> enemyPath;

    public GameObject target;
    public float maxHealth;
    public float health;
    public int damage;
    public float attackCooldown;
    public int viewingRange;
    float attackTimer;

    public string name;

    public GameObject healthBar;
    public TextMesh healthText;

    
	// Use this for initialization
	void Start ()
    {
        mapScript = GameObject.FindWithTag("Map").GetComponent<MapGeneration>();
        playerScript = GameObject.FindWithTag("Player").GetComponent<Player>();
	}

    void OnMouseUp()
    {
        playerScript.GeneratePathTo(posX, posY);
        playerScript.playerPath.RemoveAt(playerScript.playerPath.Count - 1);
        playerScript.target = this.gameObject;
        Destroy(GameObject.FindGameObjectWithTag("Marker"));
        Instantiate(playerScript.redMarker, mapScript.TileCoordToWorldCoord(posX, posY) + new Vector3(0, 0.2f, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update ()
    {
        CheckForTarget();
        MovementController();

        if (target != null)
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

        if (health <= 0)
        {
            Destroy(this.gameObject);
        }
        UIController();
    }

    void UIController()
    {
        float healthBarScale = health / maxHealth;
        healthBar.transform.localScale = new Vector3(healthBarScale, 1, 1);
        string healthTextValue = health.ToString();
        healthText.text = healthTextValue;
    }

    void CheckForTarget()
    {
        for(int x = posX - viewingRange; x <= posX + viewingRange; x++)
        {
            for (int y = posY - viewingRange; y <= posY + viewingRange; y++)
            {
                if (playerScript.posX == x && playerScript.posY == y)
                {
                    target = playerScript.gameObject;
                    GeneratePathTo(playerScript.posX, playerScript.posY);
                    break;
                }
            }
        }
    }

    void CombatController()
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= 10f)
        {
            Player PS = target.GetComponent<Player>();
            PS.TakeDamage(damage);
            if (PS.target == null)
            {
                PS.target = this.gameObject;
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
            if (enemyPath == null)
            {
                return;
            }

            transform.position = mapScript.TileCoordToWorldCoord(posX, posY);

            remainingMovement -= mapScript.CostToEnterTile(enemyPath[0].x, enemyPath[0].y);

            posX = enemyPath[1].x;
            posY = enemyPath[1].y;

            enemyPath.RemoveAt(0);

            if (enemyPath.Count == 1)
            {
                enemyPath = null;
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

        while (unvisited.Count > 0)
        {
            Node u = null;

            foreach (Node possibleU in unvisited)
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

            foreach (Node v in u.neighbours)
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

        currentPath.RemoveAt(0);
        currentPath.Reverse();
        enemyPath = currentPath;
    }
}
