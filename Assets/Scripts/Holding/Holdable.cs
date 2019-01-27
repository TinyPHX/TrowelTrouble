using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using TP.ExtensionMethods;

[RequireComponent(typeof(Rigidbody))]
public class Holdable : MonoBehaviour
{
    [Header(" --- HOLDABLE SETTINGS --- ")]

    public Transform leftHandGoal;
    public Transform rightHandGoal;
    public Transform lookGoal;
    [SerializeField] private Transform anchor;
    public HoldableJoint jointPrefab;
    public bool beingHeld = false;
    public bool autoPickup = false;
    public Collider pickupTrigger;
    public Collider handCollider;
    public bool mountToHand;

    private HoldableJoint holdableJoint;
    private IHolder holder;
    private bool initialized;
    
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Ray leftHandRay;
    private Ray rightHandRay;
    private Ray lookRay;
    private Rigidbody rigidBody;

    private void Awake()
    {
        if (pickupTrigger == null)
        {
            pickupTrigger = GetComponent<Collider>();
        }
    }

    public void Initialize()
    {
        if (!initialized)
        { 
            initialized = true;
            if (leftHandGoal == null)
            {
                leftHandGoal = new GameObject("Left Hand Goal").transform;
                leftHandGoal.transform.parent = transform;
            }
            if (rightHandGoal == null)
            {
                rightHandGoal = new GameObject("Right Hand Goal").transform;
                rightHandGoal.transform.parent = transform;
            } 
            if (lookGoal == null)
            {
                lookGoal = new GameObject("Look Goal").transform;
                lookGoal.transform.parent = transform;
            }
            if (handCollider == null)
            {
                handCollider = GetComponent<Collider>();
            }
                        
            if (anchor == null)
            {
                anchor = new GameObject("Anchor").transform;
                anchor.transform.parent = transform;
            }

            rigidBody = GetComponent<Rigidbody>();
        }
    }

    public void ConnectToJoint(IHolder holder)
    {
        
        
        Initialize();

        startPosition = anchor.position;
        startRotation = anchor.rotation;
            
        this.holder = holder;

        if (HoldableJoint == null)
        {
            Transform jointParent;

            if (mountToHand)
            {
                jointParent = holder.RightHandAttach;
            }
            else
            {
                jointParent = holder.JointAnchor;
            }

            holdableJoint = Instantiate(jointPrefab, jointParent);
//            holdableJoint.transform.rotation = startRotation;
//            holdableJoint.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }

//        if (handCollider != null)
//        {
//            float anchorLength = handCollider.bounds.extents.z;
//
//            anchor.transform.localPosition = new Vector3(
//                0,
//                0,
//                -anchorLength * .5f);
//        }

        holdableJoint.Connect(this);

//        if (handCollider != null)
//        {
//            Vector3 anchorDirection = (anchor.transform.position - handCollider.bounds.center).normalized;
//            Vector3 leftHandDirection = Quaternion.Euler(Vector3.up * 90) * anchorDirection;
//            Vector3 rightHandDirection = Quaternion.Euler(Vector3.up * -90) * anchorDirection;
//            Vector3 lookDirection = Quaternion.Euler(Vector3.right * 90) * Quaternion.Inverse(transform.rotation) * anchorDirection;
//
//            float maxDistanceFromCenter = Mathf.Max(handCollider.bounds.size.x, handCollider.bounds.size.y, handCollider.bounds.size.z);
//
//            leftHandRay = new Ray(handCollider.bounds.center + leftHandDirection * maxDistanceFromCenter, -leftHandDirection);
//            rightHandRay = new Ray(handCollider.bounds.center + rightHandDirection * maxDistanceFromCenter, -rightHandDirection);
//            lookRay = new Ray(handCollider.bounds.center + lookDirection * maxDistanceFromCenter, -lookDirection * maxDistanceFromCenter);
//
//            RaycastHit raycastHit = new RaycastHit();
//
//            if (handCollider.Raycast(leftHandRay, out raycastHit, maxDistanceFromCenter))
//            {
//                leftHandGoal.transform.position = raycastHit.point;
//                leftHandGoal.transform.localEulerAngles = new Vector3(0, 0, 90);
//            }
//
//            raycastHit = new RaycastHit();
//
//            if (handCollider.Raycast(rightHandRay, out raycastHit, maxDistanceFromCenter))
//            {
//                rightHandGoal.transform.position = raycastHit.point;
//                rightHandGoal.transform.localEulerAngles = new Vector3(0, 0, -90);
//            }
//
//            raycastHit = new RaycastHit();
//
//            if (handCollider.Raycast(lookRay, out raycastHit, maxDistanceFromCenter))
//            {
//                lookGoal.transform.position = raycastHit.point;
//            }
//        }

        if (holder.Rigidbody != null && rigidBody != null)
        {
            holder.Rigidbody.IgnoreCollision(rigidBody, true);
        }
    }

    public void BreakJoint()
    {
        holdableJoint.Break();
        beingHeld = false;
        OnHoldableJointBreak(0);
    }

    public virtual void Use(IHolder holder)
    {
        
    }

    public Vector3 Anchor
    {
        get
        {
            if (anchor == null)
            {
                Initialize();
            }

            return anchor.position;
        }
    }
    
    public Vector3 LocalAnchor
    {
        get
        {
            if (anchor == null)
            {
                Initialize();
            }

            return anchor.localPosition;
        }
    }

    public HoldableJoint HoldableJoint
    {
        get
        {
            return holdableJoint;
        }
    }

    public virtual void OnHoldableJointBreak(float breakForce)
    {
        holder.JointBroken(breakForce);

        if (holder.Rigidbody != null && rigidBody != null)
        {
            holder.Rigidbody.IgnoreCollision(rigidBody, false);
        }
    }
    
    void OnDrawGizmosSelected()
    { 
        float sphereSize = .05f;

        if (leftHandGoal != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(leftHandGoal.transform.position, sphereSize);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(rightHandGoal.transform.position, sphereSize);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(lookGoal.transform.position, sphereSize);
            
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(anchor.transform.position, sphereSize);
        }
    }
}