using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TP.ExtensionMethods;
using UnityEngine;

public class PlayerController : MonoBehaviour, IHolder
{
	[Header(" --- Player Attributes--- ")]
	[SerializeField] private float moveSpeed;
	[SerializeField] private float speedWhildHoldingMultiplier;
	[SerializeField] private float jumpForce = 50f;
	[SerializeField] private float jumpForceTime = .1f;
	[SerializeField] private float rotationSpeed = .5f;
	[SerializeField] private float spinAcceleration = 1;
	[SerializeField] private float spinSpeedMax = 1;
	[SerializeField] private float releaseForce = 1;
	[SerializeField] private float releaseAngle = 12;

	[Header(" --- Input Mappings --- ")]
	[SerializeField] private InputDevice.GenericInputs axisMoveX;
	[SerializeField] private InputDevice.GenericInputs axisMoveY;
	[SerializeField] private InputDevice.GenericInputs axisJump;
	[SerializeField] private InputDevice.GenericInputs axisAction2;
	[SerializeField] private InputDevice.GenericInputs axisAction3;
	[SerializeField] private InputDevice.GenericInputs axisAction4;
	
	[Header(" --- Input Values --- ")]
	[SerializeField, ReadOnly] private Vector3 move;
	[SerializeField, ReadOnly] private float jump;
	[SerializeField, ReadOnly] private float action2;
	[SerializeField, ReadOnly] private float action3;
	[SerializeField, ReadOnly] private float action4;
	[SerializeField, ReadOnly] private Vector3 previousMove;
	[SerializeField, ReadOnly] private float previousAction2;
	[SerializeField, ReadOnly] private float previousAction3;
	[SerializeField, ReadOnly] private float previousAction4;
	
	[Header(" --- Animator --- ")]
	[SerializeField] private Animator characterAnimator;
	[SerializeField, ReadOnly] private const string ANIM_SPEED_MULTIPLIER = "SpeedMultiplier";
	[SerializeField, ReadOnly] private const string ANIM_GROUNDED = "Grounded";
	[SerializeField, ReadOnly] private const string ANIM_JUMP = "Jump";
	[SerializeField, ReadOnly] private const string ANIM_ATTACK = "Attack";

	[Header(" --- Holding --- ")]
	[SerializeField] private float dropDelaySeconds = .5f;
	[SerializeField] private Transform jointAnchor;
	[SerializeField] private Transform rightHand;
	[SerializeField] private IKControl ikControl;
	[SerializeField, ReadOnly] private Holdable held;
	[SerializeField, ReadOnly] private long lastDropTime;
	
	[Header(" --- Other Stuff --- ")]
	[SerializeField] private GameObject character;
	[SerializeField] private Rigidbody rotationRigidBody;
	[SerializeField] private GameObject trowel;
	[SerializeField] private AnimationClip trowelAnimation;
	[SerializeField] private Collider hitBox;
	[SerializeField] private LayerMask jumpMask;
	[SerializeField, ReadOnly] private bool grounded;
	[SerializeField, ReadOnly] private InputDevice inputDevice;
	private List<InputDevice> inputDevices = new List<InputDevice>();
	private new Rigidbody rigidbody;
	private new CameraControl cameraControl;
	private BrickTower brickTower;
	private float timeSinceJump = 0;
	private float spinAngleChangeAverage;

	private void Awake()
	{
		SetupInputDevices();
		rigidbody = GetComponent<Rigidbody>();
		cameraControl = FindObjectOfType<CameraControl>();
		brickTower = FindObjectOfType<BrickTower>();
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		AssignInputDevice();

		GetUserInput();

		UpdateAnimator();

		UpdateIK();

		UpdateBrickRelease();

		UpdateSlash();
	}

	private void FixedUpdate()
	{
		Rotate();
		Move();
		Jump();
	}

	private void SetupInputDevices()
	{
		inputDevices.Add(new InputDevice(InputDevice.ID.K1));
		//inputDevices.Add(new InputDevice(InputDevice.ID.M1));

		int controllerCount = InputDevice.CONTROLLERS.Count;
		string[] joyNames = Input.GetJoystickNames();
		for (int i = 0; i < controllerCount; i++)
		{
			InputDevice.ID inputDeviceID = InputDevice.CONTROLLERS[i];
			InputDevice inputDevice = new InputDevice(inputDeviceID);

			inputDevices.Add(inputDevice);
		}
	}
	
	private void AssignInputDevice()
	{
		foreach (InputDevice inputDevice in inputDevices)
        {
            inputDevice.Refresh();

            if (inputDevice.Valid)
            {
				if (inputDevice.GetAxis(InputDevice.GenericInputs.ACTION_1) > 0)
				{
					this.inputDevice = inputDevice;
				}
            }
        }
	}

	private void GetUserInput()
	{
		previousMove = move;
		previousAction2 = action2;
		previousAction3 = action3;
		previousAction4 = action4;
		
		move = ConvertToCameraSpace(new Vector3(inputDevice.GetAxis(axisMoveX), inputDevice.GetAxis(axisMoveY)));
		move *= HoldingMultiplier;
		jump = inputDevice.GetAxis(axisJump);
		action2 = inputDevice.GetAxis(axisAction2);
		action3 = inputDevice.GetAxis(axisAction3);
		action4 = inputDevice.GetAxis(axisAction4);
	}

	private void Move()
	{
		if (action2 == 0 || held == null)
		{
			float brickTowerDistance = Vector3.Distance(brickTower.transform.position, transform.position);
			float distanceMultiplier = 10 + brickTowerDistance;

			Vector3 newVelocity = move * moveSpeed * Time.fixedDeltaTime * distanceMultiplier;
			newVelocity.y = rigidbody.velocity.y; //Preserve vertical velocity from gravity.
			rigidbody.velocity = newVelocity;
		}
	}

	private void Jump()
	{
		grounded = CheckIfGrounded();
		bool applyJumpForce = false;

		if (jump > 0)
		{
			if (grounded)
			{
				timeSinceJump = 0;
				rigidbody.AddForce(Vector3.up * jumpForce * HoldingMultiplier * 3);
				characterAnimator.SetTrigger(ANIM_JUMP);
			}

			if (timeSinceJump < jumpForceTime)
			{
				timeSinceJump += Time.fixedDeltaTime;
				rigidbody.AddForce(Vector3.up * jumpForce * HoldingMultiplier);
			}
		}
	}
	
	private bool CheckIfGrounded()
	{
		bool groundCheck = false;
		float distanceToGround = .02f;

		RaycastHit raycastHit = new RaycastHit();
		groundCheck = Physics.Raycast(
			new Ray(hitBox.bounds.center, Vector3.down),
			out raycastHit,
			hitBox.bounds.size.y / 2 + distanceToGround,
			jumpMask);

		return groundCheck;
	}

	private void UpdateAnimator()
	{
		characterAnimator.SetFloat(ANIM_SPEED_MULTIPLIER, move.magnitude);
		characterAnimator.SetBool(ANIM_GROUNDED, grounded);
	}

	private void Rotate()
	{
		if (held != null && action3 > 0)
		{
			float currentSpinSpeed = rotationRigidBody.angularVelocity.y;
			float speedChange = -spinAcceleration * .2f;
			float newSpinSpeed = currentSpinSpeed + speedChange;
			Vector3 newAngularVelocity = Vector3.up * Mathf.Clamp(newSpinSpeed, -Mathf.Abs(spinSpeedMax), Mathf.Abs(spinSpeedMax));
			rotationRigidBody.angularVelocity = newAngularVelocity;  
			rotationRigidBody.maxAngularVelocity = spinSpeedMax;
		}
		else if (held != null && action2 > 0)
		{
			float lookAngle = 0;
            float previousLookAngle = 0;
            float spinAngleChange = 0;
            float facingAngle = rotationRigidBody.transform.rotation.eulerAngles.y;
			float facingAngleChange = 0;
            if (move != Vector3.zero)
            {
                lookAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
                previousLookAngle = Mathf.Atan2(previousMove.x, previousMove.z) * Mathf.Rad2Deg;
                
                spinAngleChange = lookAngle.AngleChange(previousLookAngle);
            
                float fractionOfASecond = 1f / Time.fixedDeltaTime / 20f;
                spinAngleChangeAverage -= spinAngleChangeAverage / fractionOfASecond; 
                spinAngleChangeAverage += spinAngleChange / fractionOfASecond;
	            
	            facingAngleChange = lookAngle.AngleChange(facingAngle);
	            
//	            Debug.Log(
//		            "lookAngle: " + lookAngle + ", " +
//		            "previousLookAngle: " + previousLookAngle + ", " + 
//		            "spinAngleChange: " + spinAngleChange + ", " + 
//		            "spinAngleChangeAverage: " + spinAngleChangeAverage + ", " + 
//		            "facingAngleChange: " + facingAngleChange + ", " 
//		            );
            }
			
            float currentSpinSpeed = rotationRigidBody.angularVelocity.y;
            
            float speedChange = spinAcceleration * (spinAngleChange * Mathf.Deg2Rad);
            float newSpinSpeed = currentSpinSpeed + speedChange;

            //Direction changing
            if (currentSpinSpeed > 1 && speedChange.Polartiy() != currentSpinSpeed.Polartiy())
            {
                float slowDownRate = 5;
                speedChange = Mathf.Min(Mathf.Abs(speedChange), Mathf.Abs(currentSpinSpeed)) * speedChange.Polartiy() * slowDownRate;
                newSpinSpeed = currentSpinSpeed + speedChange;
            }

            if (Mathf.Abs(spinAngleChangeAverage) < 2)
            {
                if (Mathf.Abs(facingAngleChange) < 10)
                {
                    newSpinSpeed = facingAngleChange * Mathf.Deg2Rad;
                }
                else
                {
                    newSpinSpeed = (facingAngleChange * Mathf.Deg2Rad) / Time.fixedDeltaTime * 2;
                }
            }

            Vector3 newAngularVelocity = Vector3.up * Mathf.Clamp(newSpinSpeed, -Mathf.Abs(spinSpeedMax), Mathf.Abs(spinSpeedMax));
            rotationRigidBody.angularVelocity = newAngularVelocity;  
            rotationRigidBody.maxAngularVelocity = spinSpeedMax;
		}
		else
		{
			rotationRigidBody.angularVelocity = Vector3.zero;
			
			if (move.LongerThan(new Vector3(.01f, .01f, .01f)))
			{
				Vector3 newRotation = Quaternion.LookRotation(move, Vector3.up).eulerAngles;
				newRotation.x = 0;
				newRotation.z = 0;
				character.transform.rotation = Quaternion.RotateTowards(character.transform.rotation, Quaternion.Euler(newRotation), rotationSpeed);
			}

			spinAngleChangeAverage = 0;
		} 
	}

	private void UpdateBrickRelease()
	{
		if (held != null && (previousAction2 > 0 && action2 == 0 || previousAction3 > 0 && action3 == 0))
		{
			Rigidbody heldRigidBody = held.GetComponent<Rigidbody>();
			Vector3 newVelocity = heldRigidBody.velocity * releaseForce;
			newVelocity.y = newVelocity.magnitude * releaseAngle;
			heldRigidBody.velocity = newVelocity;
			Debug.Log("Throwing brick! newVelocity: " + newVelocity);
			BreakJoint();
		}
	}

	private void UpdateSlash()
	{
		if (action4 != 0 && previousAction4 == 0)
		{
			characterAnimator.SetTrigger(ANIM_ATTACK);
		}
	}

	public void TrowelSlashStart()
	{
		trowel.SetActive(true);
		Animation animation = trowel.GetComponent<Animation>();
		animation.AddClip(trowelAnimation, trowelAnimation.name);
		animation.Play(trowelAnimation.name);
	}

	public void TrowelSlashEnd()
	{
		trowel.SetActive(false);
	}

	private Vector3 ConvertToCameraSpace(Vector2 axis)
	{
		if (cameraControl == null)
		{
			Debug.LogWarning("Trying to convert to camera space but playerCamera is null.", cameraControl);

			return axis;
		}

		Vector3 cameraSpaceAxis = new Vector3(axis.x, 0, axis.y);
		Quaternion cameraRotation = Quaternion.AngleAxis(cameraControl.transform.rotation.eulerAngles.y, Vector3.up);
		cameraSpaceAxis = cameraRotation * cameraSpaceAxis;

		return cameraSpaceAxis;
	}

	public void UpdateIK()
	{
		if (held != null)
		{
			ikControl.rightHandGoal = held.rightHandGoal;
			ikControl.leftHandGoal = held.leftHandGoal;
			ikControl.lookGoal = held.lookGoal;
			ikControl.ikActive = true;
		}
		else
		{
			ikControl.ikActive = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody otherRigidBody = other.attachedRigidbody;

		if (otherRigidBody)
		{
			Holdable holdable = otherRigidBody.GetComponent<Holdable>();

			if (holdable != null && ActivelyHeld == null)
			{
				EnemyBrick enemyBrick = otherRigidBody.GetComponent<EnemyBrick>();
				if (enemyBrick)
				{
					if (enemyBrick.isDead && !enemyBrick.inTower && !enemyBrick.IsHeld)
					{
						var ellapsed = new TimeSpan(DateTime.Now.Ticks - lastDropTime);
						if (ellapsed.Seconds > dropDelaySeconds)
						{
							enemyBrick.PickedUp();
							ActivelyHeld = holdable;
						}
					}
				}
			}
		}
	}

	public void BreakJoint()
	{
		held.BreakJoint();
		trowel.SetActive(true);
	}

	public void JointBroken(float breakForce)
	{
		if (held != null)
		{
			held.BeingHeld = false;
			held = null;
		}

		lastDropTime = DateTime.Now.Ticks;
	}
	
	public Holdable ActivelyHeld
	{
		set
		{
			var ellapsed = new TimeSpan(DateTime.Now.Ticks - lastDropTime);
			if (ellapsed.Seconds > dropDelaySeconds || value == null)
			{
				held = value;

				if (held != null)
				{
					held.ConnectToJoint(this);
					trowel.SetActive(false);
				}
				else
				{
					trowel.SetActive(true);
				}
			}
		}
		get
		{
			return held;
		}
	}

	public Transform JointAnchor
	{
		get
		{
			return jointAnchor;
		}
	}

	public Transform RightHandAttach
	{
		get
		{
			return rightHand;
		}
	}

	public Rigidbody Rigidbody
	{
		get
		{
			return rigidbody;
		}
	}

	public float HoldingMultiplier
	{
		get { return held == null ? 1 : speedWhildHoldingMultiplier; }
	}
}
