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
    public LayerMask characterMask;
    public LayerMask hideLayer;

    PlayerController player;
    ChaserController[] npcs;
    GameObject chaser;

    public ChaseState[] npcState;
    public ItState[] npcRole;
    public ChaseState playerState;
    public ItState playerRole;

    private void Awake()
    {
        player = PlayerController.instance;
        npcs = FindObjectsOfType<ChaserController>();
        playerRole = ItState.Hider;
        npcRole = new ItState[npcs.Length];
        npcRole.Populate(ItState.Hider);

        npcRole[Random.Range(0, npcs.Length - 1)] = ItState.Chaser;

        instance = this;
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

