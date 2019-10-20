using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CleverCrow.Fluid.BTs.Tasks;
using CleverCrow.Fluid.BTs.Trees;
using System.Linq;

public class ChaserBehavior : MonoBehaviour
{

    public bool notIt = false;

    private bool hidden = false;
    private bool chase = false;
    private bool spotted = false;

    GameObject target;
    Rigidbody2D rigid2d;

    HideZone[] hideZones;

    Queue<HideZone> searchedList;

    Vector2 lookDirection;

    List<GameObject> spottedList;

    [SerializeField]
    private BehaviorTree _tree;

    // From HideAndSeekController
    float spotHideDist;
    LayerMask characterMask;
    LayerMask hideLayer;

    private void Awake()
    {
        rigid2d = GetComponent<Rigidbody2D>();
        hideZones = FindObjectsOfType<HideZone>();
        spottedList = new List<GameObject>();

        _tree = new BehaviorTreeBuilder(gameObject)
            .Selector()
                .Condition(() => hidden)
                .Sequence("It")
                    .Condition("Not It", () => !notIt)
                    .Sequence("Search")
                        .Condition("Found Hider", () => !chase)
                        .Do("Check Hiding Spot", () =>
                        {
                            HideZone hidespot = target.GetComponent<HideZone>();
                            if (!hidespot || Vector2.Distance(rigid2d.position,hidespot.position) <= .2f)
                            {
                                GetHidingSpot(true);
                            }
                            CheckForHider();
                            return TaskStatus.Success;
                        })
                    .End()
                    .Sequence("Chase")
                        .Condition("Found Hider", () => chase)
                        .Do("Chase Hider", () =>
                        {
                            return TaskStatus.Success;
                        })
                    .End()
                .End()
                .Sequence("Not It")
                    .Condition("Not It", () => !notIt)
                    .Sequence("Hide")
                        .Condition("Spotted", () => !spotted)
                        .Condition("Hidden", () => hidden)
                        .Do("Find Hiding Spot", () =>
                        {
                            return TaskStatus.Success;
                        })
                    .End()
                    .Sequence("Flee")
                        .Condition("Spotted", () => spotted)
                        .Do("Run Away", () =>
                        {
                            return TaskStatus.Success;
                        })
                    .End()
                .End()
            .End()
            .Build();

    }
    // Start is called before the first frame update
    void Start()
    {
        spotHideDist = HideAndSeekController.instance.spotHideDist;
        characterMask = HideAndSeekController.instance.characterMask;
        hideLayer = HideAndSeekController.instance.hideLayer;
    }

    // Update is called once per frame
    void Update()
    {
        // Update our tree every frame
        _tree.Tick();
    }

    void GetHidingSpot(bool isSearching = false)
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
            var minDist = tempHide.Min<HideZone>(o => Vector2.Distance(o.position, rigid2d.position));
            var closeHide = tempHide.First(o => Vector2.Distance(o.position, rigid2d.position) == minDist);
            SetDestination(closeHide.transform, closeHide.bounds);
            searchedList.Enqueue(closeHide);
        }
        else
        {
            var minDist = hideZones.Min<HideZone>(o => Vector2.Distance(o.position, rigid2d.position));
            var closeHide = hideZones.First(o => Vector2.Distance(o.position, rigid2d.position) == minDist);
            SetDestination(closeHide.transform, closeHide.bounds);
        }
    }

    void SetDestination(Transform pos, Vector2 bounds)
    {

    }

    void CheckForHider()
    {
        var hit = Physics2D.CircleCast(rigid2d.position, spotHideDist, lookDirection, spotHideDist, characterMask);
        if (hit.collider != null)
        {
            var player = hit.collider.GetComponent<PlayerController>();
            var npc = hit.collider.GetComponent<ChaserBehavior>();
            if (player != null)
            {
                chase = true;
                //player.spotted = true;
                spottedList.Add(player.gameObject);
            }
            else if(npc != null)
            {
                chase = true;
                npc.spotted = true;
                spottedList.Add(npc.gameObject);
            }
        }
    }
}
