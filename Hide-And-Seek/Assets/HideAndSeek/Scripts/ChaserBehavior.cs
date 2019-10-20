using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using System.Linq;
using Pathfinding;

public class ChaserBehavior : ChaseBase
{
    public GameObject fleePos;

    Animator animator;

    HideZone[] hideZones;

    Queue<HideZone> searchedList;


    [SerializeField]
    private BehaviorTree _tree;

    

    // For Pathing
    public GameObject target;
    public Vector2 targetBounds = Vector2.zero;
    public float speed = 20;
    public float turnSpeed = 3;
    public float turnDst = 5;
    public float stoppingDst = 10;
    protected Path2D path;
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;


    private void Awake()
    {
        hideZones = FindObjectsOfType<HideZone>();
        searchedList = new Queue<HideZone>();
        animator = GetComponentInChildren<Animator>();
        

        _tree = new BehaviorTreeBuilder(gameObject)
            .Selector()
                //.Condition(() => hidden)
                .Sequence("It")
                    .Condition("Not It", () => isIt)
                    .Selector("Search")
                        //.Condition("Found Hider", () => !chase)
                        .Do("Check Hiding Spot", () =>
                        {
                            if (CheckForHider())
                            {
                                return TaskStatus.Success;
                            }
                            HideZone hidespot = target.GetComponent<HideZone>();
                            if (!hidespot || Physics2D.OverlapCircle(rigid2d.position,hidespot.bounds.magnitude,hideLayer))
                            {
                                GetHidingSpot(true);
                            }
                            return TaskStatus.Failure;
                        })
                    .End()
                    .Sequence("Chase")
                        .Condition("Found Hider", () => chase)
                        .Do("Chase Hider", () =>
                        {
                            if (spottedList != null && !spottedList.Contains(target) && spottedList.Count > 0)
                            {
                                var min = spottedList.Min(g => Vector2.Distance(g.transform.position, rigid2d.position));
                                var targ = spottedList.Find(g => Vector2.Distance(g.transform.position, rigid2d.position) <= min);
                                SetDestination(targ,targ.GetComponent<ChaseBase>().bounds);
                            }

                            return TaskStatus.Success;
                        })
                    .End()
                .End()
                .Sequence("Not It")
                    .Condition("Not It", () => !isIt)
                    .Selector()
                        .Sequence("Hide")
                            .Condition("Spotted", () => !spotted)
                            //.Condition("Hidden", () => hidden)
                            .Do("Find Hiding Spot", () =>
                            {
                                HideZone hidespot = target.GetComponent<HideZone>();
                                if(!hidespot)
                                {
                                    GetHidingSpot();
                                }
                                return TaskStatus.Success;
                            })
                        .End()
                        .Sequence("Flee")
                            .Condition("Spotted", () => spotted)
                            .Do("Run Away", () =>
                            {
                                if (target != fleePos || Vector2.Distance(rigid2d.position, target.transform.position) < 1f)
                                {
                                    var side = (Random.value > 0.5f) ? 1 : -1;
                                    Vector2 newPos = rigid2d.position + gameControl.currentIt.lookDirection * spotHideDist + Vector2.Perpendicular(gameControl.currentIt.lookDirection) * Random.Range(side*spotHideViewRadius*2f, side*spotHideViewRadius);
                                    if (!Physics2D.OverlapCircle(newPos,bounds.magnitude, obstacleLayer))
                                    {
                                        fleePos.transform.position = newPos;
                                        SetDestination(fleePos, bounds);
                                    }
                                }

                                return TaskStatus.Success;
                            })
                        .End()
                    .End()
                .End()
            .End()
            .Build();

    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        StartCoroutine(UpdatePath());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        // Update our tree every frame
        _tree.Tick();
    }

    void GetHidingSpot(bool isSearching = false)
    {
        if (isSearching)
        {
            var tempHide = hideZones.Where(o => !searchedList.Contains(o));
            if (tempHide.Count() <= 0)
            {
                searchedList.Clear();
                searchedList.Enqueue(target.gameObject.GetComponent<HideZone>());
                tempHide = hideZones.Where(o => !searchedList.Contains(o));
            }
            var minDist = tempHide.Min<HideZone>(o => Vector2.Distance(o.position, rigid2d.position));
            var closeHide = tempHide.First(o => Vector2.Distance(o.position, rigid2d.position) == minDist);
            SetDestination(closeHide.gameObject, closeHide.bounds);
            searchedList.Enqueue(closeHide);
        }
        else
        {
            var minDist = hideZones.Min<HideZone>(o => Vector2.Distance(o.position, rigid2d.position));
            var closeHide = hideZones.First(o => Vector2.Distance(o.position, rigid2d.position) == minDist);
            SetDestination(closeHide.gameObject, closeHide.bounds);
        }
    }

    void SetDestination(GameObject pos, Vector2 newbounds)
    {
        if(target == pos)
        {
            return;
        }
        target = pos;
        targetBounds = newbounds;
        StopCoroutine("FollowPath");
    }

    


    protected IEnumerator FollowPath()
    {

        bool followingPath = true;
        int pathIndex = 0;
        //transform.LookAt (path.lookPoints [0],Vector3.up);

        float speedPercent = 1;

        while (followingPath)
        {
            Vector2 pos2D;

            if (rigid2d)
            {
                pos2D = new Vector2(rigid2d.position.x, rigid2d.position.y);
            }
            else
            {
                pos2D = new Vector2(transform.position.x, transform.position.y);
            }

            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }

            if (followingPath)
            {

                if (pathIndex >= path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.01f)
                    {
                        followingPath = false;
                    }
                }

                //Quaternion targetRotation = Quaternion.LookRotation (Vector3.zero,path.lookPoints [pathIndex] - transform.position);
                //transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                //transform.Translate (Vector3.up * Time.deltaTime * speed * speedPercent, Space.Self);
                Vector2 move;
                if (rigid2d)
                {
                    move = Vector2.MoveTowards(rigid2d.position, path.lookPoints[pathIndex], Time.deltaTime * speed * speedPercent);
                    rigid2d.MovePosition(move);
                }
                else
                {
                    move = Vector2.MoveTowards(transform.position, path.lookPoints[pathIndex], Time.deltaTime * speed * speedPercent);
                    transform.position = move;
                }
                UpdateAnimationDirection(Vector2.MoveTowards(rigid2d.position, path.lookPoints[pathIndex],speed));
            }

            yield return null;

        }
    }

    protected IEnumerator UpdatePath()
    {

        if (Time.timeSinceLevelLoad < 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (rigid2d)
        {
            PathRequestManager2D.RequestPath(new PathRequest(rigid2d.position, target.transform.position, targetBounds, OnPathFound));
        }
        else
        {
            PathRequestManager2D.RequestPath(new PathRequest(transform.position, target.transform.position, targetBounds, OnPathFound));
        }

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.transform.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            //print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
            if ((target.transform.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                if (rigid2d)
                {
                    PathRequestManager2D.RequestPath(new PathRequest(rigid2d.position, target.transform.position, targetBounds, OnPathFound));
                }
                else
                {
                    PathRequestManager2D.RequestPath(new PathRequest(transform.position, target.transform.position, targetBounds, OnPathFound));
                }
                targetPosOld = target.transform.position;
            }
        }
    }

    public virtual void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = new Path2D(waypoints, transform.position, turnDst, stoppingDst);

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    protected void UpdateAnimationDirection(Vector2 movement)
    {
        // If your moving, set your look direction as move direction and normalize the vector (set it to magnitude of 1)
        if (!Mathf.Approximately(movement.x, 0.0f) || !Mathf.Approximately(movement.y, 0.0f))
        {
            lookDirection.Set(-movement.x, -movement.y);
            lookDirection.Normalize();
        }

        if (animator)
        {
            // Set your animator animation direction and speeds
            animator.SetFloat("Look X", lookDirection.x);
            animator.SetFloat("Look Y", lookDirection.y);
            //animator.SetFloat("MoveSpeed", movement.magnitude);
        }
    }
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    }

}
