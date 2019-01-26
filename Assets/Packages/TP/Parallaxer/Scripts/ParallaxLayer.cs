using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TP.ExtensionMethods;
using ExtensionMethods = TP.ExtensionMethods.ExtensionMethods;

namespace TP.Parallaxer
{
    //[ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class ParallaxLayer : MonoBehaviour
    {
        public float fieldOfView = 30;
        public float positionToRotation = 10;
        public bool useCanvasDistance;
        public float distance = 0;
        public Vector3 constantMovement;
        public bool useMainCamera;
        public Transform movementSource;

        private List<ParallaxObject> parallaxObjects = new List<ParallaxObject> { };
        private Canvas canvas;
        private RectTransform rectTransform;
        private Vector3 previousRotation;

        private Vector2 viewSize;
        private Vector2 maxRotationalPosition;
        private Vector2 maxScreenPosition;
        private float pixelsPerDegree;
        
        void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            parallaxObjects = GetComponentsInChildren<ParallaxObject>().ToList();
            canvas = GetComponent<Canvas>();
            if (movementSource == null)
            {
                if (useMainCamera)
                {
                    movementSource = Camera.main.transform;
                }
                else
                {
                    movementSource = canvas.worldCamera.transform;
                }
            }
            if (useCanvasDistance)
            {
                distance = canvas.planeDistance;
            }
            rectTransform = GetComponent<RectTransform>();
            ViewSize = rectTransform.rect.size;
            pixelsPerDegree = ViewSize.y / fieldOfView;
            maxRotationalPosition = (180 * pixelsPerDegree).ToVector3();
            maxScreenPosition = ViewSize / 2;
            
            foreach (ParallaxObject parallaxObject in parallaxObjects)
            {
                parallaxObject.Initialize(this);
            }
        }
        
        void Update()
        {      
            UpdatePositions();
        }

        public void UpdatePositions()
        {      
            Vector3 rotation = movementSource.eulerAngles;
            if (previousRotation != null)
            {
                rotation = rotation.SmoothRotation(previousRotation);
            }
            previousRotation = rotation;

            foreach (ParallaxObject parallaxObject in parallaxObjects)
            {
                Vector3 rotationChange = new Vector3(
                    -rotation.y * pixelsPerDegree,
                    rotation.x * pixelsPerDegree,
                    0
                    );
                Vector3 positionChange = Quaternion.Inverse(movementSource.rotation) * -movementSource.position;
                positionChange += constantMovement * Time.time;
                positionChange *= (positionToRotation / distance) * pixelsPerDegree;
                
                Vector3 newPosition = rotationChange + positionChange + parallaxObject.positionOffset;
                newPosition.z = 0;
                
                parallaxObject.Position = newPosition;
            }
        }

        public Vector2 ViewSize
        {
            private set { viewSize = value; }
            get { return viewSize; }
        }

        public Vector2 MaxRotationalPosition
        {
            get
            {
                return maxRotationalPosition;
            }

            set
            {
                maxRotationalPosition = value;
            }
        }

        public Vector2 MaxScreenPosition
        {
            get
            {
                return maxScreenPosition;
            }

            set
            {
                maxScreenPosition = value;
            }
        }

        public float PixelsPerDegree
        {
            get
            {
                return pixelsPerDegree;
            }

            set
            {
                pixelsPerDegree = value;
            }
        }
    }
}
