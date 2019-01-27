using UnityEngine;
using System;
using System.Collections;
using TP.ExtensionMethods;

[RequireComponent(typeof(Animator))]

/// <summary>
/// Handles the IKControl for animators.
/// </summary>
public class IKControl : MonoBehaviour
{
    protected Animator animator;

    public bool ikActive = false;
    [SerializeField] public Transform rightHandGoal = null;
    [SerializeField] public Transform leftHandGoal = null;
    [SerializeField] public Transform lookGoal = null;

    [SerializeField] private float weight = 1;
    [SerializeField] private float maxWeightDelta = .01f;
    
    [SerializeField] private bool useCurve = false;
    [SerializeField] private AnimationCurve curve; //x = distance, y = speed
    [SerializeField] private float minDistance = 0;
    [SerializeField] private float maxDistance = 1;
    [SerializeField] private float minSpeed = 0;
    [SerializeField] private float maxSpeed = 1;

    private Vector3 lookPosition;
    private Quaternion lookRotation;
    private Vector3 rightHandPosition;
    private Quaternion rightHandRotation;
    private Vector3 leftHandPosition;
    private Quaternion leftHandRotation;

    private Vector3 lookPositionGoal;
    private Quaternion lookRotationGoal;
    private Vector3 rightHandPositionGoal;
    private Quaternion rightHandRotationGoal;
    private Vector3 leftHandPositionGoal;
    private Quaternion leftHandRotationGoal;

    [SerializeField, ReadOnly] private float currentWeight = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive)
            {
                currentWeight = Mathf.MoveTowards(currentWeight, weight, maxWeightDelta);
                
                // Set the look target position, if one has been assigned
                if (lookGoal != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookGoal.position);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandGoal != null)
                {
                    ApplyIK(AvatarIKGoal.RightHand, rightHandGoal.position, rightHandGoal.rotation, ref rightHandPosition, ref rightHandRotation);
                }

                // Set the left hand target position and rotation, if one has been assigned
                if (leftHandGoal != null)
                {
                    ApplyIK(AvatarIKGoal.LeftHand, leftHandGoal.position, leftHandGoal.rotation, ref leftHandPosition, ref leftHandRotation);
                }

            }
            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                currentWeight = Mathf.MoveTowards(currentWeight, 0, maxWeightDelta);
                
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, currentWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, currentWeight);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, currentWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, currentWeight);
                animator.SetLookAtWeight(0);
            }
        }
    }

    void ApplyIK(AvatarIKGoal avitarIkGoal, Vector3 goalPosition, Quaternion goalRotation, ref Vector3 currentPosition, ref Quaternion currentRotation)
    {
        animator.SetIKPositionWeight(avitarIkGoal, currentWeight);
        animator.SetIKRotationWeight(avitarIkGoal, currentWeight);
        
        if (useCurve)
        {
            float distance = Vector3.Distance(currentPosition, goalPosition);
            float rotation = Quaternion.Angle(currentRotation, goalRotation);
            float speed = EvaluateCurve(distance, minDistance, maxDistance, minSpeed, maxSpeed);
            float rotationSpeed = EvaluateCurve(rotation, minDistance * 100, maxDistance * 100, minSpeed * 100, maxSpeed * 100);
            
            currentPosition = Vector3.MoveTowards(currentPosition, goalPosition, speed);
            currentRotation = Quaternion.RotateTowards(currentRotation, goalRotation, rotationSpeed);
            
            animator.SetIKPosition(avitarIkGoal, goalPosition);
            animator.SetIKRotation(avitarIkGoal, goalRotation);
        }
        else
        {
            animator.SetIKPosition(avitarIkGoal, goalPosition);
            animator.SetIKRotation(avitarIkGoal, goalRotation);
        }
    }

    float EvaluateCurve(float x, float minX, float maxX, float minY, float maxY)
    {
        float ratioX = (x - minX) / (maxX - minX);
        if (ratioX > 1)
        {
            ratioX = 1;
        }
        float ratioY = curve.Evaluate(ratioX);
        float y = (maxY - minY) * ratioY + minY;

        return y;
    }
}
