using UnityEngine;
using System.Collections.Generic;

public class Avoider : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;

    public GameObject player;
    public float maxDistance;
	
    public bool canSee;

    [SerializeField] private int visionAmount = 12;

    [Header("Poisson")]
    [SerializeField] private float sampleWidth = 20f;
    [SerializeField] private float sampleHeight = 20f;
    [SerializeField] private float sampleRadius = 1f;

    [Header("Escape")]
    [SerializeField] private float escapeCheckTime = 0.5f;
    [SerializeField] private LayerMask visionBlockerMask;

    private float escapeTimer;

    void Update()
    {
        LookAt();
        EstablishVision();
        Escape();
    }

    private void LookAt()
    {
        transform.LookAt(player.transform.position);
    }

    private void EstablishVision()
    {
        canSee = false; 

        //shoots rays in a 360 view
        for (int i = 0; i < visionAmount; i++)
        {
            float angle = i * (360f / visionAmount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Shoot(direction);
        }
    }

    private void Shoot(Vector3 direction)
    {
        //finds the direction
        Vector3 worldDir = transform.TransformDirection(direction);

        //shoots rays and changes color based on hit
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

    private void Escape()
    {
        //if you cant see the player, you not moving
        if(!canSee)
        {
            agent.ResetPath();
            return;
        }

        //if the agent is moving, dont look for new points
        if(agent.pathPending)
            {return;}

        if(agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
            {return;}

        FindEscapePoint();
    }

    private void FindEscapePoint()
    {
        //getting our sampler
        PoissonDiscSampler sampler = new PoissonDiscSampler(sampleWidth,sampleHeight,sampleRadius);

        //making a list of the safe points
        List<Vector3> safePoints = new List<Vector3>();

        foreach (Vector2 sample in sampler.Samples())
        {
            //finding the point on the poisson sampler
            Vector3 point = transform.position + new Vector3(sample.x - sampleWidth / 2f,0f,sample.y - sampleHeight / 2f);

            //makesake sure the point is actually on the navmesh
            if (!UnityEngine.AI.NavMesh.SamplePosition(point,out UnityEngine.AI.NavMeshHit hit,2f,UnityEngine.AI.NavMesh.AllAreas))
            {continue;}

            //checks to see if the player can see the point
            bool playerCanSee = PlayerCanSeePoint(hit.position);

            // Debug point
            Debug.DrawLine(hit.position,hit.position + Vector3.up * 0.5f,playerCanSee ? Color.red : Color.green,0.5f);

            //do not add any points the player can see
            if (playerCanSee)
            {continue;}

            //if you cant escape here, do not add the point
            if(!IsValidEscapePoint(hit.position))
                {continue;}


            //if you make it here, this is a safe point that can be transported to
            safePoints.Add(hit.position);
        }

        //no safe locations were found
        if (safePoints.Count == 0)
        {
            return;
        }

        //figure out what the closes safe point to you is
        Vector3 closestPoint = safePoints[0];
        float closestDistance = Vector3.Distance(transform.position,closestPoint);

        for (int i = 1; i < safePoints.Count; i++)
        {
            float distance = Vector3.Distance(transform.position,safePoints[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = safePoints[i];
            }
        }

        // Only safe points can reach this line.
        agent.SetDestination(closestPoint);
    }

    private bool PlayerCanSeePoint(Vector3 point)
    {
        //raycast origin
        Vector3 origin = player.transform.position + Vector3.up;
        //direction of raycast
        Vector3 direction = point - origin;
        //distance between player and point
        float distance = direction.magnitude;

        //shoot a ray from player to point, if ray hits something, it is blocked, ! means that it only returns true when nothing is blocking the players view
        return !Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance,visionBlockerMask);
    }

    private bool IsValidEscapePoint(Vector3 point)
    {
        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        //if we cant calculate a path from enemy to escape point, there is no path;
        if(!agent.CalculatePath(point,path))
        {return false;}

        //makes sure that calculated path reaches the escape point
        return path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
    }
}