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
    Vector2 lookDirection;
    public ChaseState currentState = ChaseState.None;

    Rigidbody2D rigidbody2d;
    Animator animator;

    private void Awake()
    {
        currentState = ChaseState.Flee;
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
        var speed = movementSpeed * Time.deltaTime;
        speed = ((currentState == ChaseState.Chase)? 1 : -1)*speed; // Move towards or away?
        Vector2 move = Vector2.MoveTowards(rigidbody2d.position, CharacterController.instance.transform.position, speed);
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
}
