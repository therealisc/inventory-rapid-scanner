using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class InventoryRotationAlgorithm
{
    public string GetNextInventoryForExitProcess(Dictionary<string, decimal> inventories, OperationalInventoryModel previousExit)
    {
        var inventoryList = inventories.Select(x => x.Key).ToList();
        var lastExitIndexInList = inventoryList.IndexOf(previousExit.gestiune);
        var nextIndexInList = lastExitIndexInList + 1;

        if (nextIndexInList == inventories.Count)
        {
            nextIndexInList = 0;
        }

        while (true)
        {
            var nextInventory = inventoryList[nextIndexInList];

            if (inventories[nextInventory] > 0)
            {
                return nextInventory;
            }
            else
            {
                nextIndexInList += 1;
                if (nextIndexInList == inventoryList.Count)
                {
                    nextIndexInList = 0;
                }
            }
        }
    }
}
