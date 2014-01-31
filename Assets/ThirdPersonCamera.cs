
using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	#region Variables (private)
	

	[SerializeField]
	private float distanceAway = 5f;
	[SerializeField]
	private float distanceUp = 2f;
	[SerializeField]
	private Transform followXform;
	[SerializeField]
	private float smooth = 3f;
	[SerializeField]
	private Vector3 offset = new Vector3(0f, 1.5f, 0f);
	private Vector3 targetPosition;
	private Vector3 lookDir;

	private Vector3 velocityCamSmooth = Vector3.zero;
	[SerializeField]
	private float camSmoothDampTime = 0.1f;
	[SerializeField]
	private float wideScreen = 0.2f;
	[SerializeField]
	private float targetingTime = 0.5f;
	private CamState camState = CamState.Behind;
	#endregion
	
	
	public enum CamState{
		Behind,
		FirstPerson,
		Target,
		Free
	}

	void Start ()
	{
		followXform = GameObject.FindWithTag ("Player").transform;
		lookDir = followXform.forward;

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
		if (Input.GetAxis ("Target") > 0.01f) camState = CamState.Target;
		else camState = CamState.Behind;
		Vector3 characterOffset = followXform.position + new Vector3(0f, distanceUp, 0f);
		switch (camState) {
		case CamState.Behind:

			lookDir = characterOffset - transform.position;
			lookDir.y = 0;
			lookDir.Normalize ();
			Debug.DrawRay (transform.position, lookDir, Color.green);


			Debug.DrawLine (followXform.position, targetPosition, Color.blue);
			break;
		case CamState.Target:
			lookDir = followXform.forward;
			break;
		}


		targetPosition = characterOffset + followXform.up * distanceUp - lookDir * distanceAway;
		CompensateForWalls (characterOffset, ref targetPosition);
		smoothPosition (transform.position, targetPosition);
		//transform.position = Vector3.Lerp (transform.position, targetPosition, Time.deltaTime*smooth);

		transform.LookAt (followXform);
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
}
