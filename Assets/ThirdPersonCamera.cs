
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
	
	#endregion
	
	

	void Start ()
	{
		followXform = GameObject.FindWithTag ("Player").transform;
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


		Vector3 characterOffset = followXform.position + offset;
		lookDir = characterOffset - transform.position;
		lookDir.y = 0;
		lookDir.Normalize ();
		Debug.DrawRay (transform.position, lookDir, Color.green);
		//targetPosition = followXform.position + followXform.up * distanceUp - followXform.forward * distanceAway;
		targetPosition = characterOffset + followXform.up * distanceUp - lookDir * distanceAway;
		Debug.DrawRay (followXform.position, -1f*followXform.forward * distanceAway, Color.blue);
		Debug.DrawRay (followXform.position, followXform.up * distanceUp - followXform.forward * distanceAway, Color.yellow);


		smoothPosition (transform.position, targetPosition);
		//transform.position = Vector3.Lerp (transform.position, targetPosition, Time.deltaTime*smooth);

		transform.LookAt (followXform);
	}

	private void smoothPosition(Vector3 fromPos, Vector3 toPos){
		this.transform.position = Vector3.SmoothDamp (fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
	}

}
