using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MazeConstructor))]
public class GameController : MonoBehaviour
{
    private MazeConstructor constructor;
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    public GameObject playerPrefab;
    public GameObject monsterPrefab;
    private AIController aIController;
    private bool toggle = true;


    void Awake()
    {
        constructor = GetComponent<MazeConstructor>();
        aIController = GetComponent<AIController>();
    }
    void Start()
    {

        constructor.GenerateNewMaze(rows, cols, OnTreasureTrigger);
        aIController.Graph = constructor.graph;
        aIController.Player = CreatePlayer();
        aIController.Monster = CreateMonster();
        aIController.HallWidth = constructor.hallWidth;
        aIController.StartAI();
    }
void Update()
{
    
    if (Input.GetKeyDown(KeyCode.F))
    {
        if(toggle){

        // Get player's position in the grid
        Vector3 playerPos = aIController.Player.transform.position;
        int playerPosX = Mathf.RoundToInt(playerPos.x / constructor.hallWidth);
        int playerPosZ = Mathf.RoundToInt(playerPos.z / constructor.hallWidth);

        // Get goal's position in the grid
        Vector3 goalPos = new Vector3(constructor.goalCol * constructor.hallWidth, 0f, constructor.goalRow * constructor.hallWidth);
        int goalPosX = Mathf.RoundToInt(goalPos.x / constructor.hallWidth);
        int goalPosZ = Mathf.RoundToInt(goalPos.z / constructor.hallWidth);

        // Use AIController to find path
        List<Node> path = aIController.FindPath(playerPosZ, playerPosX, goalPosX, goalPosZ);

   // Generate spheres along the path
    foreach (Node node in path)
    {
        Vector3 potentialSpherePos = new Vector3(node.y * constructor.hallWidth, 1, node.x * constructor.hallWidth);
    // Check if this node's position matches the treasure's position
        if (potentialSpherePos != constructor.Treasure.transform.position)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = potentialSpherePos;
            sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f); // Adjust the size of the sphere if required
            sphere.tag = "PathSphere";
        // Destroy the SphereCollider component to disable collision
            Destroy(sphere.GetComponent<SphereCollider>());
        }
    }       
        toggle = false;
        }
        
        else{
            ClearPath();
            toggle = true;
        }
        // Clear previous path spheres
    }
}

// Clears all spheres from the previous path
void ClearPath()
{
    Debug.Log("Clearing path spheres."); // Add this line for debugging
    GameObject[] pathSpheres = GameObject.FindGameObjectsWithTag("PathSphere");
    foreach (GameObject sphere in pathSpheres)
    {
        Destroy(sphere);
    }
}


// when the monster collides with the player a new maze is created
// if the path is still active it will clear the path and set the toggle to true//
  private void OnMonsterTrigger(GameObject trigger, GameObject other)
    { 
        if(other.gameObject.tag == "Player") 
        {
            Debug.Log("Gotcha!");
            aIController.StopAI();
            constructor.GenerateNewMaze(rows, cols, OnTreasureTrigger);
            aIController.Graph = constructor.graph;
            aIController.Player = CreatePlayer();
            aIController.Monster = CreateMonster();
            aIController.HallWidth = constructor.hallWidth;
            aIController.StartAI();
          
            ClearPath();
              toggle = true;
        }
    }


 
private GameObject CreatePlayer()
{
    // Find existing player and destroy it
    GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
    if (existingPlayer != null)
    {
        Destroy(existingPlayer);
    }

    Vector3 playerStartPosition = new Vector3(constructor.hallWidth, 1, constructor.hallWidth);
    GameObject player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
    player.tag = "Player";

    return player;
}


    private GameObject CreateMonster()
    {
        Vector3 monsterPosition = new Vector3(constructor.goalCol * constructor.hallWidth, 0f, constructor.goalRow * constructor.hallWidth);
        GameObject monster = Instantiate(monsterPrefab, monsterPosition, Quaternion.identity);
        monster.tag = "Monster";
        
        // Step 2
        TriggerEventRouter triggerEventRouter = monster.AddComponent<TriggerEventRouter>();
        triggerEventRouter.callback += OnMonsterTrigger;

        return monster;
    }
    private void OnTreasureTrigger(GameObject trigger, GameObject other)
{ 
    Debug.Log("You Won!");
    aIController.StopAI();
}

}

