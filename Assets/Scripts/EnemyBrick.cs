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
        navMeshAgent = GetComponent<NavMeshAgent>();
        brickTower = FindObjectOfType<BrickTower>();
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Unstack()
    {
        this.inTower = false;
        this.navMeshAgent.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (navMeshAgent != null&& navMeshAgent.enabled)
        {
            if (isDead && inTower == false)
            {
                MoveAwayFrom(brickTower.transform.position, 19);
            }
            else if (isDead == false && inTower == false)
            {
                MoveTo(brickTower.transform.position);
            }
            else
            {
                StopMoving();
            }
        }
    }

    private void MoveAwayFrom(Vector3 position, float distance)
    {
        Vector3 direction = (transform.position - position).normalized;
        Vector3 newPosition = transform.position + (direction * distance);
        MoveTo(newPosition);
    }

    private void MoveTo(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }

    private void StopMoving()
    {
        navMeshAgent.SetDestination(transform.position);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
