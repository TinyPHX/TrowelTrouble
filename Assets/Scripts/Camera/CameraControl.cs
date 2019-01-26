using TP.ExtensionMethods;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

/// <summary>
/// This is the script we use to control the camera. It is
/// a modified version of the one provided by Unity standard
/// assets.
/// </summary>
public class CameraControl : MonoBehaviour
{
    [System.Serializable]
    public struct CameraTarget
    {
        [SerializeField, HideInInspector] private string name;   
        [SerializeField] private Transform transform;
        [SerializeField] private bool overrideTarget;
        [SerializeField] private bool useRotation;
        [SerializeField] private float cameraDistance;

        public CameraTarget(CameraTarget cameraTarget)
        {
            name = cameraTarget.transform.gameObject.name;
            transform = cameraTarget.transform;
            overrideTarget = cameraTarget.overrideTarget;
            useRotation = cameraTarget.useRotation;
            cameraDistance = cameraTarget.cameraDistance;
        }

        public CameraTarget(Transform transform, bool overrideTarget = false, bool useRotation = false, float cameraDistance = -1) : this()
        {
            name = transform.gameObject.name;
            this.transform = transform;
            this.overrideTarget = overrideTarget;
            this.useRotation = useRotation;
            this.cameraDistance = cameraDistance;
        }

        public bool IsDefault { get { return transform == null; } }
        public Transform Transform { get { return transform; } }
        public bool OverrideTarget { get { return overrideTarget; } set { overrideTarget = value; } }
        public bool UseRotation { get { return useRotation; } set { useRotation = value; } }
        public float CameraDistance { get { return cameraDistance; } set { cameraDistance = value; } }
    } 
    
    public enum Type { NORMAL, CINIMA };

    public Type cameraType = Type.NORMAL;
    public float cinimaRotationSpeed = .3f;
    public float dampTime = 0.2f;                 // Approximate time for the camera to refocus.
    public float rotationDampTime = 0.2f;   
    public float screenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge.
    public float minSize = 6.5f;                  // The smallest orthographic size the camera can be.
    [SerializeField] private List<CameraTarget> targets = new List<CameraTarget> { };
    [SerializeField] private CameraTarget cameraTargetOverride;
    [SerializeField] private float cameraDistance = 15;

    [Tooltip("Use this to set the rotation target to a different source than the position target")]
    [SerializeField] private Transform altRotationTarget;
    [SerializeField] private Vector3 desiredRotation;
    [SerializeField] private Vector3 offsetRotation;
    public bool moveEnabled = true;
    [SerializeField] private Vector3 offsetPosition;
    [SerializeField] bool autoDepthOfField = true;

    [SerializeField, ReadOnly] private new Camera camera;                        // Used for referencing the camera.
    [SerializeField, ReadOnly] private float zoomSpeed;                      // Reference speed for the smooth damping of the orthographic size.
    [SerializeField, ReadOnly] private Vector3 moveVelocity;                 // Reference velocity for the smooth damping of the position.
    [SerializeField, ReadOnly] private Vector3 desiredPosition;              // The position the camera is moving towards.
    [SerializeField, ReadOnly] private new Rigidbody rigidbody;
    [SerializeField, ReadOnly] public DepthOfField depthOfField;
    
    private float startCameraDistance;
    private Vector3 startRotaion;

    [Header(" --- Trowel Trouble Variables --- ")]
    [SerializeField] private float minDistance = 1;
    [SerializeField] private float maxDistance = 30;
    [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private float minAngle = 10;
    [SerializeField] private float maxAngle = 45;
    [SerializeField, ReadOnly] PlayerController playerController;
    [SerializeField, ReadOnly] BrickTower brickTower;
    private void Start()
    {
        camera = GetComponentInChildren<Camera>();

        startRotaion = transform.eulerAngles;
        startCameraDistance = cameraDistance;
        desiredRotation = transform.eulerAngles;

        rigidbody = GetComponent<Rigidbody>();
        depthOfField = GetComponent<DepthOfField>();

        playerController = FindObjectOfType<PlayerController>();
        brickTower = FindObjectOfType<BrickTower>();
    }

    private void Update()
    {
        if (cameraType == Type.CINIMA)
        {
            desiredRotation = new Vector3(
                desiredRotation.x,
                desiredRotation.y + cinimaRotationSpeed,
                desiredRotation.z);
        }

        TrowelTroubleBehavior();
        
        if (moveEnabled)
        {
            Move();
            Rotate();

            if (desiredPosition != Vector3.zero)
            {
                Zoom();
            }
        }
    }

    public void TrowelTroubleBehavior()
    {
        Vector3 towerPosition = brickTower.transform.position;
        Vector3 playerPostion = playerController.transform.position;

        towerPosition.y = 0;
        playerPostion.y = 0;

        float newDistance = Vector3.Distance(playerController.transform.position, brickTower.transform.position);
        cameraDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        float distanceRatio = (cameraDistance - minDistance) / (maxDistance - minDistance);
        Debug.Log("distanceRatio: " + distanceRatio);
        cameraDistance = cameraDistance * distanceCurve.Evaluate(distanceRatio);

        rotationDampTime = 1 / newDistance * 2;
            
        if (newDistance < 3)
        {
            dampTime = .05f;
            rotationDampTime = 1f;
        }
        else
        {
            dampTime = .2f;
        }
        
        Vector3 angle = Quaternion.LookRotation(towerPosition - playerPostion).eulerAngles;
        desiredRotation = angle;

        desiredRotation.x = (1 - distanceRatio) * (maxAngle - minAngle) + minAngle;
    }

    public void AddTarget(Transform transform, bool overrideTarget = false, bool useRotation = false, float cameraDistance = -1)
    {
        CameraTarget cameraTarget = new CameraTarget(transform, overrideTarget, useRotation, cameraDistance);

        AddTarget(cameraTarget);
    }

    public void AddTarget(CameraTarget cameraTarget)
    {
        if (cameraTarget.OverrideTarget)
        {
            cameraTargetOverride = cameraTarget;
        }
        
        targets.AddUnique(cameraTarget);
    }

    public void RemoveTarget(Transform transform)
    {
        CameraTarget targetFound = targets.FirstOrDefault(target => target.Transform == transform);
        if (!targetFound.IsDefault)
        {
            targets.Remove(targetFound);

            if (targetFound.OverrideTarget && targetFound.Equals(cameraTargetOverride))
            {
                cameraTargetOverride = default(CameraTarget);
            }
        }
    }

    private void Move()
    {
        
        if (!cameraTargetOverride.IsDefault)
        {
            desiredPosition = cameraTargetOverride.Transform.position;
        }
        else
        {
            desiredPosition = FindAveragePosition();    
        }

        desiredPosition = ApplyDistance(desiredPosition);

        if (desiredPosition != Vector3.zero)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVelocity, dampTime);
        }
    }

    void Rotate()
    {
        Vector3 newRotation = desiredRotation + offsetRotation;
        
        if (newRotation != transform.eulerAngles && !newRotation.Equals(transform.eulerAngles))
        {
            transform.rotation = transform.rotation.RotateTowardsSnap(Quaternion.Euler(newRotation), 1 / rotationDampTime);
        }
    }
    
    private Vector3 FindAveragePosition()
    {
        Vector3 averagePos = Vector3.zero;
        int numTargets = 0;

        for (int i = 0; i < targets.Count; i++)
        {
            CameraTarget target = targets[i];
            
            if (target.IsDefault || !target.Transform.gameObject.activeInHierarchy)
            {
                continue;
            }
            
            averagePos += target.Transform.position;
            numTargets++;
        }
        
        if (numTargets > 0)
        {
            averagePos /= numTargets;

            // Keep the same y value.
            //averagePos.y = transform.position.y;
        }
        
        return averagePos + offsetPosition;
    }

    private Vector3 ApplyDistance(Vector3 position)
    {
//        if (!cameraTargetOverride.IsDefault)
//        {
//            CameraDistance = cameraTargetOverride.CameraDistance;
//        }
//        else
//        {
//            CameraDistance = startCameraDistance;
//        }
        
        return position + transform.rotation * (Vector3.back * CameraDistance);
    }

    private void Zoom()
    {
        float requiredSize = FindRequiredSize();
        if (camera.orthographic)
        {
            camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, requiredSize, ref zoomSpeed, dampTime);
        }
        else
        {
            camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, requiredSize, ref zoomSpeed, dampTime) * 3;

            if (depthOfField != null && autoDepthOfField)
            {
                if (CameraDistance > 10)
                {
                    depthOfField.focalLength = CameraDistance + 15;
                    depthOfField.aperture = 15;
                    depthOfField.focalSize = 2;
                }
                else
                {
                    depthOfField.focalLength = cameraDistance;
                    depthOfField.aperture = 2;
                    depthOfField.focalSize = 50;
                }
            }
        }
    }

    private float FindRequiredSize()
    {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(desiredPosition);
        float size = 0f;
        
        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].Transform.gameObject.activeSelf)
            {
                continue;                
            }
            
            Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].Transform.position);
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / camera.aspect);
        }

        size += screenEdgeBuffer;
        size = Mathf.Max(size, minSize);

        return size;
    }

    public void SetPositionNoDamp(Vector3 position)
    {
       transform.position = position + (transform.rotation * (Vector3.back * CameraDistance)); // Sets camera position back from given position by camera distance
    }
    
    public float CameraDistance
    {
        get { return cameraDistance; }
        set { cameraDistance = value; }
    }
}