
using UnityEngine;
using System.Collections;

struct CameraPosition {
	private Vector3 position;
	private Transform xForm;
	public Vector3 Position {get {return position;} set {position = value;}}
	public Transform XForm {get {return xForm;} set{xForm = value;}}
	public void Init(string camName, Vector3 pos, Transform transform, Transform parent){
		position = pos;
		xForm = transform;
		xForm.name = camName;
		xForm.parent = parent;
		xForm.localPosition = Vector3.zero;
		xForm.localPosition = position;
	}
}

public class ThirdPersonCamera : MonoBehaviour
{
	#region Variables (private)
	
	private const float TARGETING_THRESHOLD = 0.01f;
	[SerializeField]
	private float distanceAway = 5f;
	[SerializeField]
	private float fPSRotationDegreePerSecond = 120f;
	[SerializeField]
	private float distanceUp = 2f;
	[SerializeField]
	private Transform followXform;
	[SerializeField]
	private float smooth = 3f;
	[SerializeField]
	private Vector3 offset = new Vector3(0f, 1.5f, 0f);
	[SerializeField]
	private float firstPersonThreshold = 0.5f;
	[SerializeField]
	private float firstPersonLookSpeed = 3.0f;
	[SerializeField]
	private Vector2 firstPersonXAxisClamp = new Vector2(-70.0f, 45.0f);
	private Vector3 targetPosition;
	private Vector3 lookDir;
	[SerializeField]
	private CharacterControllerLogic follow;
	private Vector3 velocityCamSmooth = Vector3.zero;
	[SerializeField]
	private float camSmoothDampTime = 0.1f;
	[SerializeField]
	private float wideScreen = 0.2f;
	[SerializeField]
	private float targetingTime = 0.5f;
	private Vector3 velocityLookDir = Vector3.zero;
	[SerializeField]
	private float lookDirDampTime = 0.1f;
	private CamState camState = CamState.Behind;
	private CameraPosition firstPersonCamPos;
	private float xAxisRot = 0.0f;
	private float lookWeight;
	private Vector3 curLookDir;
	#endregion
	
	public CamState CameraState{get {return camState;}}

	public enum CamState{
		Behind,
		FirstPerson,
		Target,
		Free
	}

	void Start ()
	{
		follow = GameObject.FindWithTag ("Player").GetComponent<CharacterControllerLogic>();
		followXform = GameObject.FindWithTag ("Player").transform;

		lookDir = followXform.forward;
		firstPersonCamPos = new CameraPosition ();
		firstPersonCamPos.Init (
			"FirstPersonCamera",
			new Vector3 (0f, 1.6f, 0.2f),
			new GameObject ().transform,
			followXform
		);

	}


	void Update ()
	{
		
	}
	
	/// <summary>
	/// Debugging information should be put here.
	/// </summary>
	void OnDrawGizmos ()
	{	
		
	}
	
	void LateUpdate()
	{		
		float rightX = Input.GetAxis("RightStickX");
		float rightY = Input.GetAxis("RightStickY");

		float leftX = Input.GetAxis("Horizontal");
		float leftY = Input.GetAxis("Vertical");

		Vector3 characterOffset = followXform.position + new Vector3(0f, distanceUp, 0f);
		Vector3 lookAt = characterOffset;
		Vector3 targetPosition = Vector3.zero;

		if (Input.GetAxis ("Target") > TARGETING_THRESHOLD)
			camState = CamState.Target;
		else {
			// * First Person *
			if (rightY > firstPersonThreshold && !follow.IsInLocomotion())
			{
				// Reset look before entering the first person mode
				xAxisRot = 0;
				lookWeight = 0f;
				camState = CamState.FirstPerson;
			}
			// * Behind the back *
			if ((camState == CamState.FirstPerson && Input.GetButton("ExitFPV")) || 
			    (camState == CamState.Target && (Input.GetAxis("Target") <= TARGETING_THRESHOLD)))
			{
				camState = CamState.Behind;	
			}
		}

		follow.Animator.SetLookAtWeight (lookWeight);
		switch (camState) {
		case CamState.Behind:
			ResetCamera();
			// Only update camera look direction if moving
			if (follow.Speed > follow.LocomotionThreshold && follow.IsInLocomotion())
			{
				lookDir = Vector3.Lerp(followXform.right * (leftX < 0 ? 1f : -1f), followXform.forward * (leftY < 0 ? -1f : 1f), Mathf.Abs(Vector3.Dot(this.transform.forward, followXform.forward)));
				Debug.DrawRay(this.transform.position, lookDir, Color.white);
				
				// Calculate direction from camera to player, kill Y, and normalize to give a valid direction with unit magnitude
				curLookDir = Vector3.Normalize(characterOffset - this.transform.position);
				curLookDir.y = 0;
				Debug.DrawRay(this.transform.position, curLookDir, Color.green);
				
				// Damping makes it so we don't update targetPosition while pivoting; camera shouldn't rotate around player
				curLookDir = Vector3.SmoothDamp(curLookDir, lookDir, ref velocityLookDir, lookDirDampTime);
			}				
			
			targetPosition = characterOffset + followXform.up * distanceUp - Vector3.Normalize(curLookDir) * distanceAway;
			Debug.DrawLine(followXform.position, targetPosition, Color.magenta);

			break;
		case CamState.Target:
			ResetCamera();
			lookDir = followXform.forward;
			targetPosition = characterOffset + followXform.up * distanceUp - lookDir * distanceAway;
			break;
		case CamState.FirstPerson:
			// Looking up and down
			// Calculate the amount of rotation and apply to the firstPersonCamPos GameObject
			xAxisRot += (leftY * 0.5f * firstPersonLookSpeed);			
			xAxisRot = Mathf.Clamp(xAxisRot, firstPersonXAxisClamp.x, firstPersonXAxisClamp.y); 
			firstPersonCamPos.XForm.localRotation = Quaternion.Euler(xAxisRot, 0, 0);

			// Superimpose firstPersonCamPos GameObject's rotation on camera
			Quaternion rotationShift = Quaternion.FromToRotation(this.transform.forward, firstPersonCamPos.XForm.forward);		
			this.transform.rotation = rotationShift * this.transform.rotation;

			// Move character model's head
			follow.Animator.SetLookAtPosition(firstPersonCamPos.XForm.position + firstPersonCamPos.XForm.forward);
			lookWeight = Mathf.Lerp(lookWeight, 1.0f, Time.deltaTime * firstPersonLookSpeed);

			// Looking left and right
			// Similarly to how character is rotated while in locomotion, use Quaternion * to add rotation to character
			Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, fPSRotationDegreePerSecond * (leftX < 0f ? -1f : 1f), 0f), Mathf.Abs(leftX));
			Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
			follow.transform.rotation = (follow.transform.rotation * deltaRotation);
				
			// Move camera to firstPersonCamPos
			targetPosition = firstPersonCamPos.XForm.position;
			// Choose lookAt target based on distance
			lookAt = (Vector3.Lerp(this.transform.position + this.transform.forward, lookAt, Vector3.Distance(this.transform.position, firstPersonCamPos.XForm.position)));

			break;

		}



		CompensateForWalls (characterOffset, ref targetPosition);
		smoothPosition (transform.position, targetPosition);
		//transform.position = Vector3.Lerp (transform.position, targetPosition, Time.deltaTime*smooth);

		transform.LookAt (lookAt);
	}

	private void smoothPosition(Vector3 fromPos, Vector3 toPos){
		this.transform.position = Vector3.SmoothDamp (fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
	}

	private void CompensateForWalls(Vector3 fromObject, ref Vector3 toTarget)
	{
		Debug.DrawLine(fromObject, toTarget, Color.cyan);
		// Compensate for walls between camera
		RaycastHit wallHit = new RaycastHit();		
		if (Physics.Linecast(fromObject, toTarget, out wallHit)) 
		{
			Debug.DrawRay(wallHit.point, wallHit.normal, Color.red);
			toTarget = new Vector3(wallHit.point.x, toTarget.y, wallHit.point.z);
		}
	}

	private void ResetCamera()
	{
		lookWeight = Mathf.Lerp(lookWeight, 0.0f, Time.deltaTime * firstPersonLookSpeed);
		transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
	}
}
