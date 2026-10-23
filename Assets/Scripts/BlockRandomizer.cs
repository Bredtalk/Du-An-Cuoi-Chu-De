

using System;
using System.Collections.Generic;
public class BlockRandomizer
{
    public enum BlockType
{
    Cross, Square, Line, TShape, LShape, JShape
    // Add your own custom block shapes here

}
    private List<BlockType> currentBag = new List<BlockType>();
    private Random rng = new Random();

    // The 3 slots for available blocks
    public BlockType[] AvailableBlocks { get; private set; }

    public BlockRandomizer()
    {
        AvailableBlocks = new BlockType[3];
        
        // Fill initial 3 slots
        for (int i = 0; i < AvailableBlocks.Length; i++)
        {
            AvailableBlocks[i] = PullFromBag();
        }
    }

    /// <summary>
    /// Call this when the player places a block from a specific slot (0, 1, or 2).
    /// </summary>
    public BlockType UseBlock(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= AvailableBlocks.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(slotIndex), "Slot index must be between 0 and 2.");
        }

        BlockType usedBlock = AvailableBlocks[slotIndex];
        
        // Refresh the used slot with a new block immediately
        AvailableBlocks[slotIndex] = PullFromBag();

        return usedBlock;
    }

    /// <summary>
    /// Pulls the next block using a Tetris-style "bag" system to avoid long droughts of specific pieces.
    /// </summary>
    private BlockType PullFromBag()
    {
        // If the bag is empty, refill and shuffle it
        if (currentBag.Count == 0)
        {
            RefillBag();
        }

        // Take the first item from the shuffled bag
        BlockType next = currentBag[0];
        currentBag.RemoveAt(0);
        return next;
    }

    private void RefillBag()
    {
        // Add all available block types to the bag
        foreach (BlockType type in Enum.GetValues(typeof(BlockType)))
        {
            currentBag.Add(type);
        }

        // Shuffle the bag (Fisher-Yates shuffle)
        int n = currentBag.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            BlockType value = currentBag[k];
            currentBag[k] = currentBag[n];
            currentBag[n] = value;
        }
    }
}

