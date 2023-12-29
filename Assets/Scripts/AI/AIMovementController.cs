using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class AIMovementController : MonoBehaviour
{
    public float movePriority = 0;
    public float waitPriority = 0;
    public float leavePriority = 0;

    public const float PATHFINDING_PRIORITY = 3f;
    public const float WALLAVOIDANCE_PRIORITY = 1f;
    public const float OBSTACLEAVOIDANCE_PRIORITY = 5f;

    public LayerMask wallMask;
    public LayerMask agentMask;
    public float movementSpeed;
    public List<GameObject> removeList;

    public bool hasTarget = false;
    public Vector2 target = Vector2.zero;
    public Vector2 currentTarget = Vector2.zero;
    public List<Vector2> targetPath = new List<Vector2>();

    AIBaseState state;
    Vector2 prevMovementVector;
    Vector2 movementVector;

    private void Start()
    {
        state = new AIMoveState(this);
        state.Start();
    }

    private void Update()
    {
        switch (state.Update())
        {
            case 1:
                switch (GetCurrentPriority())
                {
                    case AIPriority.Wait:
                        state = new AIWaitState(this);
                        break;
                    case AIPriority.Leave:
                        state = new AILeaveState(this);
                        break;
                    case AIPriority.Move:
                        state = new AIMoveState(this);
                        break;
                }
                state.Start();
                break;
            case -1:
                Debug.Log("State Error Occurred");
                removeList.Remove(gameObject);
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }

    public void SetTarget(List<Vector2> _target)
    {
        target = _target.Last();
        hasTarget = true;
        targetPath = _target;
        if (targetPath.Count == 0) StartCoroutine("TargetPathFailsafe");
        else
        {
            CalculateCurrentTarget();
            InvokeRepeating("CalculateCurrentTarget", 0.2f, 0.2f);
        }
    }

    public void AddMovementInput(Vector2 input)
    {
        if (movementVector.magnitude > 10f) return;
        movementVector += input;
    }

    public void CalculateInput()
    {
        GetComponent<Rigidbody2D>().position += movementVector.normalized * Time.deltaTime * movementSpeed;
        prevMovementVector = movementVector;
        movementVector = Vector3.zero;
    }

    public Vector2 GetPathSteeringForce()
    {
        Vector3 transformedTarget = new Vector3(currentTarget.x, currentTarget.y) - transform.position;
        return transformedTarget.normalized * (1 + transformedTarget.magnitude / 10) * PATHFINDING_PRIORITY;
    }

    public Vector2 GetWallAvoidanceSteeringForce()
    {
        Vector2 rv  = Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, prevMovementVector.normalized, 3f, wallMask);
        rv += hit.normal * (3f - hit.distance);

        hit = Physics2D.Raycast(transform.position, RotateVector2(prevMovementVector.normalized, 90), 1f, wallMask);
        rv += hit.normal * (1f - hit.distance);

        hit = Physics2D.Raycast(transform.position, RotateVector2(prevMovementVector.normalized, -90),1f, wallMask);
        rv += hit.normal * (1f - hit.distance);

        return rv.normalized * WALLAVOIDANCE_PRIORITY;
    }

    public Vector2 GetObstacleAvoidanceSteeringForce()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = agentMask;

        Physics2D.BoxCast(transform.position, Vector2.one, 0f, prevMovementVector.normalized, filter, hits);
        hits = hits.OrderBy(x  => x.distance).ToList();
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.distance > 3f) return Vector2.zero;
            if (hit.distance == 0f) continue;
            float multiplier = 1 + (3f - hit.distance);
            Vector2 rv = RotateVector2(hit.point - new Vector2(transform.position.x, transform.position.y), 90);
            Vector2 oppositerv = RotateVector2(new Vector2(transform.position.x, transform.position.y) - hit.point, 90);
            hit.collider.GetComponent<AIMovementController>().AddMovementInput(oppositerv.normalized * multiplier * OBSTACLEAVOIDANCE_PRIORITY);
            return rv.normalized * multiplier * OBSTACLEAVOIDANCE_PRIORITY;
        }

        return Vector2.zero;
    }

    Vector2 RotateVector2(Vector2 _input, int _amount)
    {
        Vector3 rv = new Vector3(_input.x, _input.y);
        rv = Quaternion.Euler(new Vector3(0, 0, -_amount)) * rv;
        return new Vector2Int(Mathf.RoundToInt(rv.x), Mathf.RoundToInt(rv.y));
    }

    public void CalculateCurrentTarget()
    {
        for (int i = targetPath.Count - 1; i >= 0; i--)
        {
            Vector3 direction = new Vector3(targetPath[i].x, targetPath[i].y) - transform.position;
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one, 0f, direction.normalized, direction.magnitude, wallMask);

            if (hit.collider == null)
            {
                currentTarget = targetPath[i];
                for (int j = i; j != 0; j--) targetPath.RemoveAt(0);
                return;
            }
        }

        if (targetPath.Count > 0) currentTarget = targetPath[0];
    }

    public AIPriority GetCurrentPriority()
    {
        if (leavePriority >= waitPriority)
        {
            if (leavePriority >= movePriority)
            {
                return AIPriority.Leave;
            }

            return AIPriority.Move;
        }

        if (movePriority >= waitPriority) return AIPriority.Move;

        return AIPriority.Wait;
    }

    
    public enum AIPriority
    {
        Wait,
        Move,
        Leave
    }

    IEnumerator DestroySelf()
    {
        removeList.Remove(gameObject);
        Destroy(gameObject);
        yield return new WaitForSeconds(2);
    }
}