using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChaseBase : MonoBehaviour
{
    public bool notIt = true;
    protected bool chase = false;
    protected bool hidden = false;
    public bool spotted = false;

    protected Vector2 lookDirection;
    protected Rigidbody2D rigid2d;

    protected List<GameObject> spottedList;

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
        rigid2d = GetComponent<Rigidbody2D>();
        spottedList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected bool CheckForHider()
    {
        chase = false;
        ClearSpottedList();
        //spottedList = new List<GameObject>();
        var hits = Physics2D.CircleCastAll(rigid2d.position, spotHideDist, lookDirection, spotHideDist, characterMask).Where(h => h.collider != null && h.collider != collider);
        foreach (var hit in hits)
        {
            var hider = hit.collider.GetComponent<ChaseBase>();
            if (hider != null)
            {
                chase = true;
                hider.spotted = true;
                if (!spottedList.Contains(hider.gameObject))
                {
                    spottedList.Add(hider.gameObject);
                }
            }
        }
        return chase;
    }

    protected void ClearSpottedList()
    {
        foreach (var spot in spottedList)
        {
            spot.GetComponent<ChaseBase>().spotted = false;
        }
        spottedList.Clear();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        ChaseBase hider = other.gameObject.GetComponent<ChaseBase>();

        if (hider != null && (!notIt || !hider.notIt))
        {
            // Someone has been caught. Either this npc or the player
            Debug.Log("Gotcha!");
            notIt = !notIt;
            hider.notIt = !hider.notIt;

        }
    }
}
