using UnityEngine;
using System.Collections;

namespace Pathfinding
{
    public class Unit2D : MonoBehaviour
    {

        const float minPathUpdateTime = .2f;
        const float pathUpdateMoveThreshold = .5f;

        public Transform target;
        public Vector2 targetBounds = Vector2.zero;
        public float speed = 20;
        public float turnSpeed = 3;
        public float turnDst = 5;
        public float stoppingDst = 10;

        protected Path2D path;
        protected Rigidbody2D rigid2d;

        protected virtual void Awake()
        {
            rigid2d = GetComponent<Rigidbody2D>();
        }

        protected virtual void Start()
        {
            
            StartCoroutine(UpdatePath());
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

        protected IEnumerator UpdatePath()
        {

            if (Time.timeSinceLevelLoad < 0.5f)
            {
                yield return new WaitForSeconds(0.5f);
            }
            if (rigid2d)
            {
                PathRequestManager2D.RequestPath(new PathRequest(rigid2d.position, target.position, targetBounds, OnPathFound));
            }
            else
            {
                PathRequestManager2D.RequestPath(new PathRequest(transform.position, target.position, targetBounds, OnPathFound));
            }

            float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
            Vector3 targetPosOld = target.position;

            while (true)
            {
                yield return new WaitForSeconds(minPathUpdateTime);
                //print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
                if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    if (rigid2d)
                    {
                        PathRequestManager2D.RequestPath(new PathRequest(rigid2d.position, target.position, targetBounds, OnPathFound));
                    }
                    else
                    {
                        PathRequestManager2D.RequestPath(new PathRequest(transform.position, target.position, targetBounds, OnPathFound));
                    }
                    targetPosOld = target.position;
                }
            }
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
                    UpdateAnimationDirection(move);
                }

                yield return null;

            }
        }

        protected virtual void UpdateAnimationDirection(Vector2 movement)
        {

        }

        public void OnDrawGizmos()
        {
            if (path != null)
            {
                path.DrawWithGizmos();
            }
        }
    }
}