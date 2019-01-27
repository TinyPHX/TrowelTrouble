using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.VR.WSA.Input;
using Random = UnityEngine.Random;

public class EnemyBrick : MonoBehaviour
{

    [Header(" --- Brick Attributes --- ")]
    public int health = 5;
    public int maxHealth = 5;
    public bool isDead = false;
    public bool inTower = false;
    public int respawnCount = 0;
    public float maxSpeed = 1f;
    public float invincibleTime = 1;

    [Header(" --- Models --- ")]
    [SerializeField] private GameObject aliveModel;
    [SerializeField] private GameObject deadModel;
    
    [Header(" --- Animation --- ")]
    [SerializeField] private Animator characterAnimator;
    [SerializeField, ReadOnly] private const string ANIM_SPEED_MULTIPLIER = "SpeedMultiplier";
    [SerializeField, ReadOnly] private const string ANIM_GROUNDED = "Grounded";
    [SerializeField, ReadOnly] private const string ANIM_JUMP = "Jump";
    [SerializeField, ReadOnly] private const string ANIM_ATTACK = "Attack";

    [Header(" --- Other --- ")]
    [SerializeField, ReadOnly] public NavMeshAgent navMeshAgent;
    [SerializeField, ReadOnly] public new Rigidbody rigidbody;
    private BrickTower brickTower;
    private GameObject cube;
    private float lastHitTime;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Collider aliveHitBox;
    [SerializeField] private Collider deadHitBox;
    [SerializeField, ReadOnly] private bool grounded;
    [SerializeField, ReadOnly] private bool previousGrounded;
    public Holdable holdable;

    public bool IsHeld
    {
        get { return holdable.BeingHeld; }
    }

    public void Hit()
    {
        if (this.health > 0)
        {
            if (Time.time - lastHitTime > invincibleTime)
            {
                this.health -= 1;
                lastHitTime = Time.time;
            }
        }
        
        if (this.health == 0 && this.isDead == false)
        {
            this.isDead = true;
            Die();
        }
    }

    public void Die()
    {
        aliveModel.SetActive(false);
        deadModel.SetActive(true);
        rigidbody.mass = 5;
        navMeshAgent.speed *= .1f;
    }

    public void PickedUp()
    {
        holdable.BeingHeld = true;
        navMeshAgent.enabled = false;

        Debug.Log("PickedUp");
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
        rigidbody = GetComponent<Rigidbody>();
        brickTower = FindObjectOfType<BrickTower>();
        holdable = GetComponent<Holdable>();
        
        navMeshAgent.speed = maxSpeed;
    }

    public void Unstack()
    {
        this.inTower = false;
        this.navMeshAgent.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (navMeshAgent != null && navMeshAgent.enabled && !IsHeld && grounded)
        {
            navMeshAgent.SetDestination(brickTower.transform.position);

            if (!isDead)
            {
                
            }
        }

        UpdateAnimation();
        
        CheckIfGrounded();
        
        EnableDisableNavAgent();
    }

    private void EnableDisableNavAgent()
    {
        if (grounded && !IsHeld && !inTower && !navMeshAgent.enabled)
        {
//            navMeshAgent.pos
            navMeshAgent.enabled = true;
        }
    }
	
    private void CheckIfGrounded()
    {
        previousGrounded = grounded;
        
        bool groundCheck = false;
        float distanceToGround = .02f;

        RaycastHit raycastHit = new RaycastHit();
        groundCheck = Physics.Raycast(
            new Ray(HitBox.bounds.center, Vector3.down),
            out raycastHit,
            HitBox.bounds.size.y / 2 + distanceToGround,
            groundMask);

        grounded = groundCheck;
    }

    private void UpdateAnimation()
    {
        if (!inTower)
        {
            float speedRatio = maxSpeed / navMeshAgent.velocity.magnitude;
            characterAnimator.SetFloat(ANIM_SPEED_MULTIPLIER, speedRatio);
        }
    }
    
    private void MoveRandomDirection(float distance)
    {
        Vector3 randomPositionOffset = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)) * distance;
        navMeshAgent.SetDestination(transform.position + randomPositionOffset);
    }

    private void MoveTo(Vector3 position)
    {
        navMeshAgent.SetDestination(position);
    }

    private void MoveAwayFrom(Vector3 position, float distance)
    {
        Vector3 direction = (transform.position - position).normalized;
        navMeshAgent.SetDestination(transform.position + (direction * distance));
    }

    private void StopMoving()
    {
        navMeshAgent.SetDestination(transform.position);
        navMeshAgent.isStopped = true;
    }

    private Collider HitBox
    {
        get { return isDead ? deadHitBox : aliveHitBox; }
    }
}
