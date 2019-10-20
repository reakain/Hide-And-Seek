using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChaseBase : MonoBehaviour
{
    public bool isIt = true;
    protected bool chase = false;
    protected bool hidden = false;
    public bool spotted = false;

    public Vector2 lookDirection;
    protected Rigidbody2D rigid2d;
    protected HideAndSeekController gameControl;
    protected List<GameObject> spottedList;

    [SerializeField]
    public Vector2 bounds { get; private set; }

    public ChaseBase currentIt;

    protected Collider2D collider;

    // From HideAndSeekController
    protected float spotHideDist;
    protected float spotHideViewRadius;
    protected float itSwapTimer;
    protected LayerMask characterMask;
    protected LayerMask hideLayer;
    protected LayerMask obstacleLayer;

    public float itResetTime = 0f;

    public void SetGameVals(float hideDist,float hideRadius,float itTimer, LayerMask charL, LayerMask hideL, LayerMask obsL)
    {
        spotHideDist = hideDist;
        spotHideViewRadius = hideRadius;
        itSwapTimer = itTimer;
        characterMask = charL;
        hideLayer = hideL;
        obstacleLayer = obsL;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        collider = GetComponent<Collider2D>();
        bounds = collider.bounds.size;
        gameControl = HideAndSeekController.instance;
        //spotHideDist = HideAndSeekController.instance.spotHideDist;
        //characterMask = HideAndSeekController.instance.characterMask;
        //hideLayer = HideAndSeekController.instance.hideLayer;
        rigid2d = GetComponent<Rigidbody2D>();
        spottedList = new List<GameObject>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        itResetTime += Time.deltaTime;
    }

    protected bool CheckForHider()
    {
        chase = false;
        ClearSpottedList();
        //spottedList = new List<GameObject>();
        var hits = Physics2D.CircleCastAll(rigid2d.position, spotHideViewRadius, lookDirection, spotHideDist, characterMask).Where(h => h.collider != null && h.collider != collider);
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

        if (hider != null && itResetTime > itSwapTimer)
        {
            if (isIt)
            {
                // Someone has been caught. Either this npc or the player
                Debug.Log("Gotcha! New it is: " + hider.gameObject.name);

                gameControl.SetIt(hider);
            }
        }
    }

    public void SetIt(ChaseBase newIt)
    {
        currentIt = newIt;
        isIt = (this == currentIt) ? true : false;
        spotted = false;
        ClearSpottedList();
        itResetTime = 0f;
    }
}
