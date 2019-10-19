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

    Vector2 lastPosition;

    public bool seesPlayer = false;

    public bool inHideZone = false;

    public LayerMask obstacleLayer;


    Rigidbody2D rigidbody2d;
    Animator animator;
    CapsuleCollider2D collider;

    public HideZone[] hideZones;

    Queue<HideZone> searchedList;

    // From HideAndSeekController
    float spotHideDist;
    LayerMask characterMask;
    LayerMask hideLayer;


    private void Awake()
    {
        searchedList = new Queue<HideZone>();

        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        collider = GetComponent<CapsuleCollider2D>();
        lastPosition = new Vector2(rigidbody2d.position.x, rigidbody2d.position.y);
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
        SetLook();
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
                ChaseFlee();
                break;
            case ChaseState.Chase:
                //ChaseFlee();
                //pather.canMove = true;
                
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
        //pather.canMove = true;
    }

    public void Hide()
    {
        if (currentState == ChaseState.Hide)
        {
            currentState = ChaseState.Hidden;
            //pather.canMove = false;
        }
        else if (currentState == ChaseState.Search)
        {
            Debug.Log("Go to new point!");
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
        var hit = Physics2D.CircleCast(rigidbody2d.position,spotHideDist, lookDirection, spotHideDist, characterMask);
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
        ChangeMoveTarget(hider.transform);
    }

    void ChangeMoveTarget(Transform newTarget, Vector2 bounds)
    {
        target = newTarget;
        targetBounds = bounds;
        StopCoroutine("UpdatePath");
        StopCoroutine("FollowPath");
        StartCoroutine("UpdatePath");
    }
    void ChangeMoveTarget(Transform newTarget)
    {
        ChangeMoveTarget(newTarget, Vector2.zero);
    }

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
            var minDist = tempHide.Min<HideZone>(o => Vector2.Distance(o.position, rigidbody2d.position));
            var closeHide = tempHide.First(o => Vector2.Distance(o.position, rigidbody2d.position) == minDist);
            ChangeMoveTarget( closeHide.transform, closeHide.bounds );
            searchedList.Enqueue(closeHide);
        }
        else
        {
            var minDist = hideZones.Min<HideZone>(o => Vector2.Distance(o.position, rigidbody2d.position));
            var closeHide = hideZones.First(o => Vector2.Distance(o.position, rigidbody2d.position) == minDist);
            ChangeMoveTarget(closeHide.transform, closeHide.bounds);
        }
    }

    public void ChaseFlee()
    {
        Vector2 move;
        var speed = movementSpeed * Time.deltaTime;

        if (!seesPlayer)
        {
            //Check if there is a collider in a certain distance of the object if not then do the following
            if (!Physics2D.Raycast(collider.bounds.center, lookDirection, maxDistance, obstacleLayer))
            {
                // Move forward
                move = Vector2.MoveTowards(rigidbody2d.position, lookDirection* maxDistance, speed);
                //transform.Translate(Vector3.forward * speed * Time.smoothDeltaTime);
            }
            else
            {
                int dIrection = -1;
                // If there is a object at the right side of the object then give a random direction
                if (Physics.Raycast(rigidbody2d.position, lookDirection.Rotate(-90), maxDistance))
                {
                    dIrection = Random.Range(-1, 2);
                }
                // If there is a object at the left side of the object then give a random direction
                if (Physics.Raycast(collider.bounds.center, lookDirection.Rotate(90), maxDistance))
                {
                    dIrection = Random.Range(-1, 2);
                }
                //rotate 90 degrees in the random direction
               move = Vector2Extension.Rotate(new Vector2(rigidbody2d.position.x, rigidbody2d.position.y), 90 * speed * dIrection);
            }
        }
        // If current distance is smaller than the given ditance, then rotate towards player, and translate the rotation into forward motion times the given speed
        else 
        {
            speed = ((currentState == ChaseState.Chase) ? 1 : -1) * speed; // Move towards or away?
            move = Vector2.MoveTowards(rigidbody2d.position, PlayerController.instance.transform.position, speed);
        }

        
        SetLook(move);

        // Get your current position
        //Vector2 position = rigidbody2d.position;

        // Set your position as your position plus your movement vector, times your speed multiplier, and the current game timestep;
        //position = position + move * speed * Time.deltaTime;

        // Tell the rigidbody to move to the positon specified
        rigidbody2d.MovePosition(move);

    }

    void SetLook(Vector2 move)
    {
        // If your moving, set your look direction as move direction and normalize the vector (set it to magnitude of 1)
        if (!(move.x == 0f) || !(move.y == 0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        // Set your animator animation direction and speeds
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        //animator.SetFloat("Speed", move.magnitude);
    }

    void SetLook()
    {
        var move = Vector2.MoveTowards(lastPosition, rigidbody2d.position, 1f);

        SetLook(move);

        lastPosition = new Vector2(rigidbody2d.position.x, rigidbody2d.position.y);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            // Someone has been caught. Either this npc or the player
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
