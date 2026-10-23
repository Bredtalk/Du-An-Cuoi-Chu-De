using UnityEngine;

public class BlockSlotsManager : MonoBehaviour
{
    private BlockRandomizer blockRandomizer;
    
    [Header("UI Containers")]
    // Assign the 3 empty UI Panels at the bottom of your screen here
    public Transform[] slotContainers; 
    
    [Header("Block Prefabs")]
    // Assign your block UI prefabs in the exact order of your BlockType enum (I, J, L, O, S, T, Z)
    public GameObject[] blockPrefabs; 
    private void Start()
    {
        blockRandomizer = new BlockRandomizer();
        
        // Populate the 3 containers when the game starts
        for (int i = 0; i < 3; i++)
        {
            SpawnBlockInContainer(i);
        }
    }
    // Called by the DraggableBlock when it successfully drops onto the grid
    public void OnBlockSuccessfullyPlaced(int slotIndex)
    {
        // 1. Consume the block in your underlying logic class
        blockRandomizer.UseBlock(slotIndex);
        
        // 2. Visually spawn the new block into that empty container
        SpawnBlockInContainer(slotIndex);
    }
    private void SpawnBlockInContainer(int slotIndex)
    {
        BlockRandomizer.BlockType nextBlockType = blockRandomizer.AvailableBlocks[slotIndex];
        
        // Instantiate the visual block prefab
        GameObject newBlock = Instantiate(blockPrefabs[(int)nextBlockType], slotContainers[slotIndex]);
        newBlock.transform.localPosition = Vector3.zero; // Center it in the container
        
        // Pass essential info to the draggable script so it knows where it came from
        DraggableBlock dragScript = newBlock.GetComponent<DraggableBlock>();
        dragScript.slotIndex = slotIndex;
        dragScript.slotManager = this;
    }
}