using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
using System.Linq;
using UnityEditor;
using System;

public class TiledCustomImporter : CustomTmxImporter
{
    private TmxAssetImportedArgs m_ImportedArgs;

    // What to do right after your assets are imported!
    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        m_ImportedArgs = args; // Set the information we get internally so we can reference it
        
            InstantiateInteractables(); // Our call to instantiate all our interactable objects
        
    }

    private void InstantiateInteractables()
    {
        // Get a list of all SuperObjects from our imported map that have the custom type Interactable set in the tileset
        var objects = m_ImportedArgs.ImportedSuperMap.GetComponentsInChildren<SuperObject>().Where(o => o.m_Type == "Interactable");

        // Now we take each interactable type object and set it up!
        foreach (var item in objects)
        {
            InstantiateObject(item);
        }
    }

    private void InstantiateObject(SuperObject marker)
    {
        try
        {
            // We assume each interactable also has a collider type object added to it's tile. 
            // If it does we take that collider object and set it's layer to Collider, separately from the rest of the interactable object
            var markCollider = marker.GetComponentInChildren<Collider2D>();
            markCollider.gameObject.layer = LayerMask.NameToLayer("Collider");
        }
        catch(Exception e)
        {
            // If it doesn't have a collider, we output a message and keep on rolling
            UnityEngine.Debug.Log("No collider object found. Exception is: " + e.ToString()) ;
        }
        // Get the object's sprite renderer
        var spriteObj = marker.GetComponentInChildren<SpriteRenderer>();
        // On the sprite renderer level game object, ad a polygon collider and our interactable controller script
        // This gives the object an interaction collider and the ability to interact with it
        var interactCollider = spriteObj.gameObject.AddComponent<PolygonCollider2D>();
        var objController = spriteObj.gameObject.AddComponent<InteractableController>();
        // Feed the Interactable Controller our object custom property values so it can provide the appropriate itneraction
        objController.SetPropertyValues(marker);
    }
}
