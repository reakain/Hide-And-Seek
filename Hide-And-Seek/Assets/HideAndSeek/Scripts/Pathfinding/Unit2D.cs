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

        protected Animator anim;
        protected Rigidbody2D rigid2d;



        protected virtual void Start()
        {
            rigid2d = GetComponent<Rigidbody2D>();
            anim = GetComponentInChildren<Animator>();
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

            if (Time.timeSinceLevelLoad < 1f)
            {
                yield return new WaitForSeconds(1f);
            }
            PathRequestManager2D.RequestPath(new PathRequest(transform.position, target.position,  targetBounds, OnPathFound));

            float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
            Vector3 targetPosOld = target.position;

            while (true)
            {
                yield return new WaitForSeconds(minPathUpdateTime);
                //print (((target.position - targetPosOld).sqrMagnitude) + "    " + sqrMoveThreshold);
                if ((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
                    PathRequestManager2D.RequestPath(new PathRequest(transform.position, target.position, targetBounds , OnPathFound));
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
                Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
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
                    transform.position = Vector2.MoveTowards(transform.position, path.lookPoints[pathIndex], Time.deltaTime * speed * speedPercent);
                }

                yield return null;

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
}