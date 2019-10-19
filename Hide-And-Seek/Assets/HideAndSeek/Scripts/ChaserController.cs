using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;


public class ChaserController : Unit2D
{

    public float movementSpeed = 4;
    public float maxDistance = 2;
    Vector2 lookDirection;
    public ChaseState currentState = ChaseState.None;
    public ItState itState = ItState.None;


    public bool seesPlayer = false;

    public bool inHideZone = false;

    public LayerMask obstacleLayer;


    Animator animator;
    CapsuleCollider2D collider;

    HideZone[] hideZones;

    Queue<HideZone> searchedList;

    // From HideAndSeekController
    float spotHideDist;
    LayerMask characterMask;
    LayerMask hideLayer;


    private void Awake()
    {
        base.Awake();
        searchedList = new Queue<HideZone>();
        animator = GetComponentInChildren<Animator>();
        collider = GetComponent<CapsuleCollider2D>();
        hideZones = FindObjectsOfType<HideZone>();
    }

    protected override void Start()
    {
        
        itState = ItState.Chaser;
        currentState = ChaseState.Search;
        SetNearestHide(true);
        spotHideDist = HideAndSeekController.instance.spotHideDist;
        characterMask = HideAndSeekController.instance.characterMask;
        hideLayer = HideAndSeekController.instance.hideLayer;
        base.Start();
    }

    void FixedUpdate()
    {
        //SetLook();
        switch (currentState)
        {
            case ChaseState.None:
                break;
            case ChaseState.Hidden:
                
                break;
            case ChaseState.Search:
                Search();
                break;
            case ChaseState.Flee:
                break;
            case ChaseState.Chase:
                break;
            case ChaseState.Caught:
                break;
        }
        
    }

    private void Update()
    {
        switch(currentState)
        {
            case ChaseState.None:
                break;
            case ChaseState.Hidden:

                break;
            case ChaseState.Search:

                break;
            case ChaseState.Flee:
                break;
            case ChaseState.Chase:
                break;
            case ChaseState.Caught:
                break;
        }
    }

    public void BecomeHider()
    {
        itState = ItState.Hider;
        SetNearestHide();
        currentState = ChaseState.Hide;
    }

    public void Hide()
    {
        if (currentState == ChaseState.Hide)
        {
            currentState = ChaseState.Hidden;
        }
        else if (currentState == ChaseState.Search)
        {
            SetNearestHide(true);
        }
    }

    public void Spotted()
    {
        if (currentState == ChaseState.Hidden)
        {
            // Play the "oh no! I'm found! animation"
            // Start running
            currentState = ChaseState.Flee;
        }
    }

    public void BecomeChaser()
    {
        itState = ItState.Chaser;
        currentState = ChaseState.Search;
    }
    
    public void Search()
    {
        var hit = Physics2D.CircleCast(rigid2d.position,spotHideDist, lookDirection, spotHideDist, characterMask);
        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                Chase(player.gameObject);
            }
        }
    }

    public void Chase(GameObject hider)
    {
        // Play the spotted animation
        currentState = ChaseState.Chase;
        searchedList.Clear();
        ChangeMoveTarget(hider.transform, hider.GetComponent<PlayerController>().bounds); ;
    }

    void ChangeMoveTarget(Transform newTarget, Vector2 bounds)
    {
        target = newTarget;
        targetBounds = bounds;
        //StopCoroutine("UpdatePath");
        StopCoroutine("FollowPath");
        //StartCoroutine("UpdatePath");
    }

    void ChangeMoveTarget(Transform newTarget)
    {
        ChangeMoveTarget(newTarget, Vector2.zero);
    }

    // Find the nearest hiding spot, and set that as the next destination. if the npc is searching, then add it to the queue so they wont just keep staying there
    public void SetNearestHide(bool isSearching = false)
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
            ChangeMoveTarget( closeHide.transform, closeHide.bounds );
            searchedList.Enqueue(closeHide);
        }
        else
        {
            var minDist = hideZones.Min<HideZone>(o => Vector2.Distance(o.position, rigid2d.position));
            var closeHide = hideZones.First(o => Vector2.Distance(o.position, rigid2d.position) == minDist);
            ChangeMoveTarget(closeHide.transform, closeHide.bounds);
        }
    }


    protected override void UpdateAnimationDirection(Vector2 movement)
    {
        base.UpdateAnimationDirection(movement);
        // If your moving, set your look direction as move direction and normalize the vector (set it to magnitude of 1)
        if (!(movement.x == 0f) || !(movement.y == 0f))
        {
            lookDirection.Set(-movement.x, -movement.y);
            lookDirection.Normalize();
        }

        // Set your animator animation direction and speeds
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        //animator.SetFloat("MoveSpeed", movement.magnitude);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            // Someone has been caught. Either this npc or the player
            Debug.Log("Gotcha!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller != null)
        {
            seesPlayer = true;
        }
    }

    // When the player exits the fishing zone, tell it you can't fish no more
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();

        if (controller != null)
        {
            seesPlayer = false;
        }
    }

    #region PATHINGFUNCTIONS
    public override void OnPathFound(Vector3[] waypoints, bool pathSuccessful)
    {
        if (pathSuccessful && 
            (currentState == ChaseState.Chase || currentState == ChaseState.Search ||
            currentState == ChaseState.Flee || currentState == ChaseState.Hide))
        {
            path = new Path2D(waypoints, transform.position, turnDst, stoppingDst);

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }
    #endregion
}
