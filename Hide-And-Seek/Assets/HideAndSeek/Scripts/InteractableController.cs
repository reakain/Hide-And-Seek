using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;

// IInteractable type class for controlling interactable objects from tiled
public class InteractableController : MonoBehaviour, IInteractable
{
    public bool isYarnActor = false; // Does it have a Yarn script?
    public bool isAnimationFlip = false; // Does it have an animation on/off state?
    public bool isMovingActor = false; // Is it moving?

    public string actorName; // Name of the object
    int animationFlipId; // obsolete
    Sprite defaultSprite; // The default "off" state of the on/off animation

    void Awake()
    {
        // Using the assigned properties, set up the object
        SetScripts();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Setup and Turn on Yarn capability
    // Get or create the object's YarnActor script
    // Assign the actor name to the yarn actor script
    // Tell it yes, it has a script
    // Turn it on
    // Tell it to load it's dialogue script int the DialogueRunner
    //void ActivateYarn()
    //{
    //    var actor = this.gameObject.GetComponent<YarnActor>();
    //    if (actor == null)
    //    {
    //        actor = this.gameObject.AddComponent<YarnActor>();
    //    }
    //    actor.actorName = actorName;
    //    actor.hasScript = true;
    //    actor.enabled = true;
    //    actor.LoadDialogue();
    //}


    // Not implemented
    /*void ActivateMoving()
    {
        var moving = this.gameObject.GetComponent<MovingCharacter>();
        if (moving == null)
        {
            moving = this.gameObject.AddComponent<MovingCharacter>();
        }
        moving.actorName = actorName;
        moving.enabled = true;
    }*/

    // Function called by the Tiled Custom Importer to setup the object according to it's properties
    public void SetPropertyValues(SuperObject marker)
    {
        // First get all the properties we care about
        isYarnActor = marker.gameObject.GetSuperPropertyValueBool("isYarnActor", false);
        actorName = marker.gameObject.GetSuperPropertyValueString("ActorName", "");
        isAnimationFlip = marker.gameObject.GetSuperPropertyValueBool("isAnimationFlip", false);
        animationFlipId = marker.gameObject.GetSuperPropertyValueInt("animationFlipId", 0);

        // Using the assigned properties, set up the object
        //SetScripts();
    }

    // Setup the object to run as defined in its properties
    void SetScripts()
    {
        // Check if it's a yarn actor and if so, setup yarn actor script
        if (isYarnActor)
        {
            //ActivateYarn();
        }

        // Not implemented
        if (isMovingActor)
        {
            //ActivateMoving();
        }

        // If it has an animation on off state, set the default sprite from it's zero position sprite index. (The tile you define an animation for in Tiled)
        // In the Tileset file, if you look at an animation with this value, you'll see all the animation frames are assigned as an animation on the tile
        // that looks like an "off" state
        if (isAnimationFlip)
        {
            defaultSprite = this.GetComponent<SpriteRenderer>().sprite;
        }
    }

    // Required function for the IInteractable interface class. Called by the player
    public bool Interact()
    {
        // Debugging note
        UnityEngine.Debug.Log("Successful interaction!");
        // If it's a yarn actor, tell it to run the dialogue
        if (isYarnActor)
        {
            //this.GetComponent<YarnActor>().RunDialogue();
        }
        // If it's an animation flip, check if the object's animator is enabled or disabled
        if (isAnimationFlip)
        {
            var objAnim = this.GetComponent<TileObjectAnimator>();
            if (objAnim.enabled)    // Object is currently "running" so turn it off
            {
                objAnim.enabled = false;    // Disable the animator
                GetComponent<SpriteRenderer>().sprite = defaultSprite;  // Set the current sprite to the default sprite
            }
            else    // Object is currently "off" so turn it on
            {
                objAnim.enabled = true; // Enable the animator
            }
        }
        return true;
    }
}
