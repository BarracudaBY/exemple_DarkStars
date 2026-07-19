using System.Collections.Generic;
using UnityEngine;

public class Inventory_Storage : Inventory_Base
{
    public Inventory_Player playerInventory { get; private set; }
    public List<Inventory_Item> materialStash;


    public void CraftItem(Inventory_Item itemToCraft)
    {
        ConsumeMaterials(itemToCraft);
        playerInventory.AddItem(itemToCraft);
    }

    public bool CanCraftItem(Inventory_Item itemToCraft)
    {
        return HasEnoughMaterials(itemToCraft) && playerInventory.CanAddItem(itemToCraft);
    }

    private void ConsumeMaterials(Inventory_Item itemToCraft)
    {
        foreach (var requiredItem in itemToCraft.itemData.craftRecipe)
        {
            int amountToConsume = requiredItem.stackSize;

            amountToConsume = amountToConsume - ConsumedMaterialsAmount(playerInventory.itemList, requiredItem);

            if(amountToConsume > 0)
                amountToConsume = amountToConsume - ConsumedMaterialsAmount(itemList, requiredItem);

            if(amountToConsume > 0)
                amountToConsume = amountToConsume - ConsumedMaterialsAmount(materialStash, requiredItem);
        }
    }

    private int ConsumedMaterialsAmount(List<Inventory_Item> itemList, Inventory_Item neededItem)
    {
        
        int amountNeeded = neededItem.stackSize;
        int consumedAmount = 0;

        foreach (var item in itemList)
        {
            if (item.itemData != neededItem.itemData)
                continue;

            int removeAmount = Mathf.Min(item.stackSize, amountNeeded - consumedAmount);
            item.stackSize = item.stackSize - removeAmount;
            consumedAmount = consumedAmount + removeAmount;

            if(item.stackSize <= 0)
                itemList.Remove(item);

            if (consumedAmount >= amountNeeded)
                break;
        }

        return consumedAmount;
    }

    private bool HasEnoughMaterials(Inventory_Item itemToCraft)
    {
        foreach (var requiredMaterial in itemToCraft.itemData.craftRecipe)
        {
            if (GetAvailableAmountOf(requiredMaterial.itemData) < requiredMaterial.stackSize)
                return false;
        }

        return true;
    }

    public int GetAvailableAmountOf(ItemDataSO requiredItem)
    {
        int amount = 0; 

        foreach (var item in playerInventory.itemList)
        {
            if (item.itemData == requiredItem)
                amount = amount + item.stackSize;
        }

        foreach (var item in itemList)
        {
            if (item.itemData == requiredItem)
                amount = amount + item.stackSize;
        }

        foreach (var item in materialStash)
        {
            if (item.itemData == requiredItem)
                amount = amount + item.stackSize;
        }

        return amount;
    }

    public void AddMaterialToStash(Inventory_Item itemToAdd)
    {
        var stackableItem = StackableInStash(itemToAdd);

        if (stackableItem != null)
            stackableItem.AddStack();
        else
            materialStash.Add(itemToAdd);

        TriggerUpdateUI();
    }

    public Inventory_Item StackableInStash(Inventory_Item itemToAdd)
    {
        List<Inventory_Item> stackableItems = materialStash.FindAll(item => item.itemData == itemToAdd.itemData);

        foreach (var stackable in stackableItems)
        {
            if (stackable.CanAddStack())
                return stackable;
        }

        return null;
    }

    public void SetInventory(Inventory_Player inventory) => this.playerInventory = inventory;

    public void FromPlayerToStorage(Inventory_Item item, bool transferFullStack)
    {
        int transferAmount = transferFullStack ? item.stackSize : 1;

        for (int i = 0; i < transferAmount; i++)
        {
            if (CanAddItem(item))
            {
                var itemToAdd = new Inventory_Item(item.itemData);

                playerInventory.RemoveOneItem(item);
                AddItem(itemToAdd);
            }
        }

        TriggerUpdateUI();
    }

    public void FromStorageToPlayer(Inventory_Item item, bool transferFullStack)
    {
        int transferAmount = transferFullStack ? item.stackSize : 1;

        for (int i = 0; i < transferAmount; i++)
        {
            if (playerInventory.CanAddItem(item))
            {
                var itemToAdd = new Inventory_Item(item.itemData);

                RemoveOneItem(item);
                playerInventory.AddItem(itemToAdd);
            }
        }


        TriggerUpdateUI();
    }
}
