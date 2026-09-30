using UnityEngine;

public class Avoider : MonoBehaviour
{
	public UnityEngine.AI.NavMeshAgent agent;

	public GameObject player;
	public float maxDistance;

	public bool canSee;

	public void Update()
	{
		LookAt();
		Shoot();
	}

	public void Detection()
	{
		while (canSee)
		{
			//run away //else //stay still
		}
	}

	private void LookAt()
	{
		transform.LookAt(player.transform.position);
	}

	private void Shoot()
	{
		RaycastHit hit;

		if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, maxDistance))
		{
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
			canSee = true;
		}
		else {canSee = false;}

	}
}
