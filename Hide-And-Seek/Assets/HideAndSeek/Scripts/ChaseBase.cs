using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseBase : MonoBehaviour
{
    public bool notIt = true;

    protected bool hidden = false;
    public bool spotted = false;

    public Vector2 bounds { get; private set; }

    protected Collider2D collider;

    // From HideAndSeekController
    protected float spotHideDist;
    protected LayerMask characterMask;
    protected LayerMask hideLayer;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        collider = GetComponent<Collider2D>();
        bounds = collider.bounds.size;
        spotHideDist = HideAndSeekController.instance.spotHideDist;
        characterMask = HideAndSeekController.instance.characterMask;
        hideLayer = HideAndSeekController.instance.hideLayer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
