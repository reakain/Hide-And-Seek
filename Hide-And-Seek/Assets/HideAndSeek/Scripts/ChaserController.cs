using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChaseState
{
    None,
    Hide,
    Found,
    Flee,
    Chase,
    Caught
}

public class ChaserController : MonoBehaviour
{
    public float movementSpeed = 4;
    public float maxDistance = 2;
    Vector2 lookDirection;
    public ChaseState currentState = ChaseState.None;

    public bool seesPlayer = false;

    Rigidbody2D rigidbody2d;
    Animator animator;
    BoxCollider2D collider;

    private void Awake()
    {
        currentState = ChaseState.None;
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        switch(currentState)
        {
            case ChaseState.None:
                break;
            case ChaseState.Hide:
                break;
            case ChaseState.Found:
                break;
            case ChaseState.Flee:
                ChaseFlee();
                break;
            case ChaseState.Chase:
                ChaseFlee();
                break;
            case ChaseState.Caught:
                break;
        }
        
    }

    public void ChaseFlee()
    {
        Vector2 move;
        var speed = movementSpeed * Time.deltaTime;

        if (!seesPlayer)
        {
            //Check if there is a collider in a certain distance of the object if not then do the following
            if (!Physics.Raycast(collider.bounds.center, lookDirection, maxDistance))
            {
                // Move forward
                move = Vector2.MoveTowards(rigidbody2d.position, lookDirection* maxDistance, speed);
                //transform.Translate(Vector3.forward * speed * Time.smoothDeltaTime);
            }
            else
            {
                // If there is a object at the right side of the object then give a random direction
                //if (Physics.Raycast(collider.bounds.center, transform.right, directionDistance))
                //{
                //    dIrection = Random.Range(-1, 2);
                //}
                //// If there is a object at the left side of the object then give a random direction
                //if (Physics.Raycast(collider.bounds.center, -transform.right, directionDistance))
                //{
                //    dIrection = Random.Range(-1, 2);
                //}
                // rotate 90 degrees in the random direction 
                move = new Vector2(rigidbody2d.position.x, rigidbody2d.position.y);//.Rotate(Vector3.up, 90 * rotateSpeed * Time.smoothDeltaTime * dIrection);
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
        if (!Mathf.Approximately(transform.position.x, 0.0f) || !Mathf.Approximately(transform.position.y, 0.0f))
        {
            lookDirection.Set(transform.position.x, transform.position.y);
            lookDirection.Normalize();
        }

        // Set your animator animation direction and speeds
        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);
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
