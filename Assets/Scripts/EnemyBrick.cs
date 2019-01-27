using UnityEngine;
using UnityEngine.AI;

public class EnemyBrick : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;
    public new Rigidbody rigidbody;
    private BrickTower brickTower;
    private GameObject cube;
    public int maxHealth = 5;
    public int health = 5;
    public bool isDead = false;

    // Use this for initialization
    public void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(brickTower.transform.position);
        }
    }

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

}
