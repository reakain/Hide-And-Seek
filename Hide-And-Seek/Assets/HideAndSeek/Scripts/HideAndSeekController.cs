using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


public class HideAndSeekController : MonoBehaviour
{
    public static HideAndSeekController instance { get; private set; }

    public float spotHideDist = 3f;
    public float spotHideViewRadius = 1f;
    public float itSwapTimer = 0.5f;
    public LayerMask characterMask;
    public LayerMask hideLayer;
    public LayerMask obstacleLayer;

    ChaseBase[] players;
    public ChaseBase currentIt;

    public ChaseState[] npcState;
    public ItState[] npcRole;
    public ChaseState playerState;
    public ItState playerRole;

    private void Awake()
    {
        instance = this;
        //player = PlayerController.instance;
        //npcs = FindObjectsOfType<ChaserController>();
        //playerRole = ItState.Hider;
        //npcRole = new ItState[npcs.Length];
        //npcRole.Populate(ItState.Hider);

        //npcRole[Random.Range(0, npcs.Length - 1)] = ItState.Chaser;

        players = FindObjectsOfType<ChaseBase>();
        foreach (var player in players)
        {
            player.SetGameVals(spotHideDist, spotHideViewRadius, itSwapTimer, characterMask, hideLayer, obstacleLayer);
            if(player.isIt)
            {
                SetIt(player);
            }
        } 
    }

    public void SetIt(ChaseBase newIt)
    {
        currentIt = newIt;
        foreach (var player in players)
        {
            player.SetIt(newIt);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

