using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header(" --- Player Attributed--- ")]
	[SerializeField] private float moveSpeed;

	[Header(" --- Input Mappings --- ")]
	[SerializeField] private InputDevice.GenericInputs axisMoveX;
	[SerializeField] private InputDevice.GenericInputs axisMoveY;
	[SerializeField] private InputDevice.GenericInputs axisJump;
	
	[Header(" --- Input Values --- ")]
	[SerializeField, ReadOnly] private Vector3 move;
	[SerializeField, ReadOnly] private float jump;
	
	
	[Header(" --- Animator --- ")]
	[SerializeField] private Animator characterAnimator;
	[SerializeField, ReadOnly] private const string ANIM_SPEED_MULTIPLIER = "SpeedMultiplier";

	[Header(" --- Other Stuff --- ")]
	[SerializeField] private GameObject character;
	[SerializeField, ReadOnly] private InputDevice inputDevice;
	private List<InputDevice> inputDevices = new List<InputDevice>();
	private new Rigidbody rigidbody;
	private new CameraControl cameraControl;
	private BrickTower brickTower;

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

		UpdateRotation();
	}

	private void FixedUpdate()
	{
		Move();
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
		move = ConvertToCameraSpace(new Vector3(inputDevice.GetAxis(axisMoveX), inputDevice.GetAxis(axisMoveY)));
		jump = inputDevice.GetAxis(axisJump);
	}

	private void Move()
	{
		float brickTowerDistance = Vector3.Distance(brickTower.transform.position, transform.position);
		float distanceMultiplier = 10 + brickTowerDistance;
		                           
		Vector3 newVelocity = move * moveSpeed * Time.fixedDeltaTime * distanceMultiplier;
		newVelocity.y = rigidbody.velocity.y; //Preserve vertical velocity from gravity.
		rigidbody.velocity = newVelocity;
	}

	private void Jump()
	{
		
	}

	private void UpdateAnimator()
	{
		characterAnimator.SetFloat(ANIM_SPEED_MULTIPLIER, move.magnitude);
	}

	private void UpdateRotation()
	{
//		Quaternion.AngleAxis()
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
}
