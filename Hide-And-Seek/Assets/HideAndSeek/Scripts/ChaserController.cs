using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public enum ChaseState
{
    None,
    Hide,
    Hidden,
    Found,
    Flee,
    Search,
    Chase,
    Caught
}

public enum ItState
{
    None,
    Chaser,
    Hider
}

public class ChaserController : MonoBehaviour
{
    public float movementSpeed = 4;
    public float maxDistance = 2;
    Vector2 lookDirection;
    public ChaseState currentState = ChaseState.None;
    public ItState itState = ItState.None;

    Vector2 lastPosition;

    public bool seesPlayer = false;

    public LayerMask obstacleLayer;
    public LayerMask hideLayer;

    Rigidbody2D rigidbody2d;
    Animator animator;
    BoxCollider2D collider;
    AIPath pather;
    AIDestinationSetter targetSet;

    HideZone[] hideZones;



    private void Awake()
    {
        //currentState = ChaseState.None;
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
        pather = GetComponent<AIPath>();
        targetSet = GetComponent<AIDestinationSetter>();
        pather.canMove = false;
        lastPosition = new Vector2(rigidbody2d.position.x, rigidbody2d.position.y);
        hideZones = FindObjectsOfType<HideZone>();
    }

    void FixedUpdate()
    {
        switch(currentState)
        {
            case ChaseState.None:
                break;
            case ChaseState.Hidden:
                
                break;
            case ChaseState.Found:
                break;
            case ChaseState.Flee:
                ChaseFlee();
                break;
            case ChaseState.Chase:
                //ChaseFlee();
                pather.canMove = true;
                SetLook();
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
        pather.canMove = true;
    }

    public void StartHiding()
    {
        if (currentState == ChaseState.Hide)
        {
            currentState = ChaseState.Hidden;
            pather.canMove = false;
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
    
    public void SearchForHider()
    {

    }

    public void SetNearestHide()
    {
        var minDist = hideZones.Min<HideZone>(o => Vector2.Distance(o.position, rigidbody2d.position));
        var closeHide = hideZones.First(o => Vector2.Distance(o.position, rigidbody2d.position) == minDist);
        targetSet.target = closeHide.transform;
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
        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
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
}
