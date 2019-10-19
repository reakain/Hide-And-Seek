using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HideZone : MonoBehaviour
{
    public Vector2 position { get; private set; }
    public Vector2 bounds { get; private set; }

    private void Awake()
    {
        position = transform.position;
        bounds = GetComponent<Collider2D>().bounds.size;
    }

    public float Distance(Vector2 target)
    {
        return Vector2.Distance(position, target);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        ChaserController NPCControl = other.GetComponent<ChaserController>();

        if (controller != null)
        {
            controller.Hide();
        }
        else if (NPCControl != null)
        {
            Debug.Log("In hide zone!");
            NPCControl.Hide();
            NPCControl.inHideZone = true;

        }
    }

    // When the player exits the fishing zone, tell it you can't fish no more
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        ChaserController NPCControl = other.GetComponent<ChaserController>();

        if (controller != null)
        {
            //controller.Hide();
        }
        else if (NPCControl != null)
        {
            NPCControl.inHideZone = false;

        }
    }
}
