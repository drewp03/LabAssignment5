using UnityEngine;

public class Avoider : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;

    public GameObject player;
    public float maxDistance;
	
    public bool canSee;

    [SerializeField] private int visionAmount = 12;

    void Update()
    {
        LookAt();
        EstablishVision();
    }

    private void LookAt()
    {
        transform.LookAt(player.transform.position);
    }

    private void EstablishVision()
    {
        canSee = false; 

        for (int i = 0; i < visionAmount; i++)
        {
            float angle = i * (360f / visionAmount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Shoot(direction);
        }
    }

    private void Shoot(Vector3 direction)
    {
        Vector3 worldDir = transform.TransformDirection(direction);

        if (Physics.Raycast(transform.position, worldDir, out RaycastHit hit, maxDistance))
        {
            Debug.DrawRay(transform.position, worldDir * hit.distance, Color.red);

            if (hit.collider.gameObject == player)
                canSee = true;
        }
        else
        {
            Debug.DrawRay(transform.position, worldDir * maxDistance, Color.green);
        }
    }
}