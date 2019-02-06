using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyBrick : MonoBehaviour
{

    [Header(" --- Brick Attributes --- ")]
    public int health = 5;
    public int maxHealth = 5;
    public bool isDead = false;
    public bool inTower = false;
    public int respawnCount = 0;
    private float safeZoneDistance = 20f;
    public float maxSpeed = 1f;
    public float invincibleTime = 1;
    public float damagePerSecond = .01f; 
    private float nextActionTime = 0.0f;
    public float actionPeriod = 1f;
    public float attackDistance = 2f;

    [Header(" --- Models --- ")]
    [SerializeField]
    private GameObject aliveModel;
    [SerializeField] private GameObject deadModel;

    [Header(" --- Animation --- ")]
    [SerializeField]
    private Animator characterAnimator;
    [SerializeField, ReadOnly] private const string ANIM_SPEED_MULTIPLIER = "SpeedMultiplier";
    [SerializeField, ReadOnly] private const string ANIM_GROUNDED = "Grounded";
    [SerializeField, ReadOnly] private const string ANIM_JUMP = "Jump";
    [SerializeField, ReadOnly] private const string ANIM_ATTACK = "Attack";

    [Header(" --- Other --- ")]
    [SerializeField, ReadOnly]
    public NavMeshAgent navMeshAgent;
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

    private Vector2 walkingDirection = Vector2.zero;

    public bool IsHeld
    {
        get
        {
            if (holdable == null)
            { return false; }

            return holdable.BeingHeld;
        }
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
        transform.position += Vector3.up * 0.427f;
        rigidbody.velocity = Vector3.up * 1;
    }

    public void PickedUp()
    {
        holdable.BeingHeld = true;
        navMeshAgent.enabled = false;
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

        holdable = GetComponent<Holdable>();

        navMeshAgent.speed = maxSpeed;
        navMeshAgent.nextPosition = new Vector3(Random.Range(-3.0f, 20.0f), 0, Random.Range(-3.0f, 20.0f));
    }

    public void Unstack()
    {
        this.inTower = false;
        this.navMeshAgent.enabled = true;
    }
    
    // Update is called once per frame
    void Update()   
    {
        if (!isDead)
        {
            rigidbody.velocity *= .1f;
        }

        if (navMeshAgent != null && navMeshAgent.enabled && !IsHeld)
        {
            if (TowerReached() && !isDead)
            {
                StopMoving();
                if (Time.time > nextActionTime && !brickTower.IsTowerEmpty())
                {
                    nextActionTime += actionPeriod;
                    brickTower.DamageTower(damagePerSecond);
                    characterAnimator.SetTrigger(ANIM_ATTACK);
                }
            }
            else 
            {
                if (isDead && !inTower)
                {
                    MoveAwayFrom(brickTower.transform.position, 19);
                }
                else if (!isDead && !inTower)
                {
                    if (brickTower.BrickCount > 0)
                    {
                        MoveTo(brickTower.transform.position);                        
                    }
                    else
                    {

                        if (Random.value > .999f || walkingDirection == Vector2.zero)
                        {
                            walkingDirection = new Vector2(Random.value - .5f, Random.value - .5f);
                        }
                        
                        if (Random.value > .2f)
                        {
                            Vector3 newPosition = transform.position;
                            newPosition.x += walkingDirection.x;
                            newPosition.z += walkingDirection.y;
                            
                            MoveTo(newPosition);
                        }
                    }
                }
                else
                {
                    StopMoving();
                }
            }
        }

        UpdateAnimation();

        CheckIfGrounded();

        EnableDisableNavAgent();
    }

    private void EnableDisableNavAgent()
    {
        if (Grounded && !IsHeld && !inTower && !navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }
    }

    private void CheckIfGrounded()
    {
        previousGrounded = Grounded;

        bool groundCheck = false;
        float distanceToGround = .02f;

        RaycastHit raycastHit = new RaycastHit();
        groundCheck = Physics.Raycast(
            new Ray(HitBox.bounds.center, Vector3.down),
            out raycastHit,
            HitBox.bounds.size.y / 2 + distanceToGround,
            groundMask);

        Grounded = groundCheck;
    }

    private void UpdateAnimation()
    {
        if (!inTower)
        {
            float speedRatio = navMeshAgent.velocity.magnitude / 2.5f;
            if (!isDead)
            {
                characterAnimator.SetFloat(ANIM_SPEED_MULTIPLIER, speedRatio);                
            }
        }
    }

    private void MoveRandomDirection(float distance)
    {
        Vector3 randomPositionOffset = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)) * distance;
        navMeshAgent.SetDestination(transform.position + randomPositionOffset);

        if (navMeshAgent != null && navMeshAgent.enabled)
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
        navMeshAgent.isStopped = true;
    }

    private Collider HitBox
    {
        get { return isDead ? deadHitBox : aliveHitBox; }
    }

    public bool Grounded
    {
        get { return grounded; }
        private set { grounded = value; }
    }

    public bool IsInSafeZone()
    {
        return GetDistanceFromTower() >= safeZoneDistance;
    }

    private float GetDistanceFromTower()
    {
        Vector3 brickTowerPosition = brickTower.transform.position;
        Vector3 brickPosition = transform.position;
        Vector3 distance = brickTowerPosition - brickPosition;
        return Vector3.Distance(brickPosition, brickTowerPosition);
    }

    private bool TowerReached()
    {
        return GetDistanceFromTower() <= attackDistance;
    }

    public bool Destroy()
    {
        if (IsInSafeZone() && isDead && inTower == false)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
