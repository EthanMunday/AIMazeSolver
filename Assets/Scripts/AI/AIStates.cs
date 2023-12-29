using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class AIBaseState
{
    public AIMovementController pawn;

    public virtual void Start() { }
    

    public virtual int Update()
    {
        return -1;
    }

    public AIBaseState(AIMovementController _pawn)
    {
        pawn = _pawn;
    }
}

public class AIWaitState : AIBaseState
{
    float waitTimer;

    public override void Start()
    {
        waitTimer = UnityEngine.Random.value * 10;
    }

    public override int Update()
    { 
        pawn.waitPriority = Mathf.Clamp(pawn.waitPriority - Time.deltaTime, 0, 15);
        pawn.movePriority = Mathf.Clamp(pawn.movePriority + Time.deltaTime, 0, 15);
        pawn.leavePriority = Mathf.Clamp(pawn.leavePriority + Time.deltaTime / 4f, 0, 15);
        waitTimer -= Time.deltaTime;
        if (waitTimer < 0) return 1;
        return 0;
    }

    public AIWaitState(AIMovementController _pawn) : base(_pawn) { }
}

public class AIMoveState : AIBaseState
{

    public override void Start()
    {
        int safety = 0;
        while (true)
        {
            if (safety == 1000) break;
            float x = GridCursor.gridXSize - 1;
            float y = GridCursor.gridYSize - 1;
            Vector2 target = new Vector2(UnityEngine.Random.Range(1f, x), UnityEngine.Random.Range(1f, y));
            List<Vector2> path = CustomNavmesh._ref.FindPathWithAStar(pawn.transform.position, target);
            if (path.Count != 0)
            {
                pawn.SetTarget(path);
                return;
            }
            safety++;
        }
    }

    public override int Update()
    {
        pawn.waitPriority = Mathf.Clamp(pawn.waitPriority + Time.deltaTime, 0, 15);
        pawn.movePriority = Mathf.Clamp(pawn.movePriority - Time.deltaTime, 0, 15);
        pawn.leavePriority = Mathf.Clamp(pawn.leavePriority + Time.deltaTime / 4f, 0, 15);

        if (!pawn.hasTarget) return -1;

        pawn.AddMovementInput(pawn.GetPathSteeringForce());
        pawn.AddMovementInput(pawn.GetWallAvoidanceSteeringForce());
        pawn.CalculateInput();
        if ((pawn.transform.position - new Vector3(pawn.currentTarget.x, pawn.currentTarget.y)).magnitude < 0.1f && pawn.currentTarget != pawn.target)
        {
            pawn.targetPath.RemoveAt(0);
            if (pawn.targetPath.Count > 0)
            {
                pawn.currentTarget = pawn.targetPath[0];
            }
        }

        if ((pawn.transform.position - new Vector3(pawn.target.x, pawn.target.y)).magnitude < 0.2f)
        {
            pawn.hasTarget = false;
            return 1;
        }
        return 0;
    }

    public AIMoveState(AIMovementController _pawn) : base(_pawn) { }
}

class AILeaveState : AIBaseState
{
    public override int Update()
    {
        pawn.removeList.Remove(pawn.gameObject);
        GameObject.Destroy(pawn.gameObject);
        return 0;
    }

    public AILeaveState(AIMovementController _pawn) : base(_pawn) { }

}
