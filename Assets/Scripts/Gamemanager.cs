using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    
    void Update()
    {
        
    }
        // Drag your shape prefabs into this array in the Unity Inspector.
    // IMPORTANT: Make sure they are in the exact same order as the BlockType enum!
    public GameObject[] blockPrefabs; 
    
    
    private BlockRandomizer randomizer;
    void Start()
    {
        // This creates the randomizer and automatically fills the 3 slots
        randomizer = new BlockRandomizer(); 
    }
    // Call this when the player clicks/selects one of the 3 slots
    public void SpawnBlockFromSlot(int slotIndex)
    {
        // 1. Get the type of block from the slot (it will auto-refill the slot behind the scenes)
        BlockRandomizer.BlockType typeToSpawn = randomizer.UseBlock(slotIndex);
        
        // 2. Convert the enum to an integer to get the matching prefab from our array
        GameObject prefabToSpawn = blockPrefabs[(int)typeToSpawn];
        
        // 3. Spawn the actual Unity Prefab in the world
        Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
    }
}
