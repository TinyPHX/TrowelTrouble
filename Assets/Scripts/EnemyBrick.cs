using UnityEngine;
using UnityEngine.AI;

public class EnemyBrick : MonoBehaviour
{

    [Header(" --- Brick Attributes --- ")]
    public int health = 5;
    public int maxHealth = 5;
    public bool isDead = false;
    public bool inTower = false;
    public int respawnCount = 0;

    [Header(" --- Other --- ")]
    public NavMeshAgent navMeshAgent;

    public new Rigidbody rigidbody;
    private BrickTower brickTower;
    private GameObject cube;

    public void Hit()
    {
        if (this.health > 0)
        {
            this.health -= 1;
        }
        if (this.health == 0 && this.isDead == false)
        {
            this.isDead = true;
        }
    }

    public void ResetToDefaults()
    {
        this.health = maxHealth;
        this.isDead = false;
        this.inTower = false;
        respawnCount++;
    }

    public void Stack()
    {
        if (health == 0)
        {
            this.inTower = true;
            this.navMeshAgent.enabled = false;
        }
    }

    // Use this for initialization
    public void Start()
    {
    }

    public void Unstack()
    {
        this.inTower = false;
        this.navMeshAgent.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(brickTower.transform.position);
        }
    }
}
