using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        [Header("物品数据")]
        public ItemDataList_SO itemDataList_SO;

        [Header("背包数据")] 
        public InventoryBag_SO PlayerBag;
        public ItemDetails GetItemDetails(int ID)
        {
            return itemDataList_SO.itemDetailsList.Find(i => i.itemID == ID);
        }

        private void Start()
        {
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,PlayerBag.itemList);
        }

        #region 添加物品到背包
        /// <summary>
        /// 添加物品到背包
        /// </summary>
        /// <param name="item"></param>
        /// <param name="toDestroy"></param>
        public void AddItem(Item item,bool toDestroy)
        {
            //是否已经有改物品
            var index = GetItemInderInBag(item.itemID);
            
            AddItemAtIndex(item.itemID,index,1);
            if (toDestroy)
            {
                Destroy(item.gameObject);
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,PlayerBag.itemList);
        }

        private bool CheckBagCapacity()
        {
            for (int i = 0; i < PlayerBag.itemList.Count; i++)
            {
                if (PlayerBag.itemList[i].itemID==0)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetItemInderInBag(int ID)
        {
            for (int i = 0; i < PlayerBag.itemList.Count; i++)
            {
                if (PlayerBag.itemList[i].itemID==ID)
                {
                    return i;
                }
            }

            return -1;
        }

        private void AddItemAtIndex(int ID, int index, int amount)
        {
            if (index==-1&&CheckBagCapacity())
            {
                var item = new InventoryItem { itemID = ID, itemAmount = amount };
                for (int i = 0; i < PlayerBag.itemList.Count;i++)
                {
                    if (PlayerBag.itemList[i].itemID==0)
                    {
                        PlayerBag.itemList[i] = item;
                        break;
                    }
                }
            }
            else
            {
                int currentAmount = PlayerBag.itemList[index].itemAmount + amount;
                var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
                PlayerBag.itemList[index] = item;
            }
        }

        #endregion


        public void SwapItem(int fromIndex, int targetIndex)
        {
            InventoryItem currentItem = PlayerBag.itemList[fromIndex];
            InventoryItem targetItem = PlayerBag.itemList[targetIndex];

            if (targetItem.itemID!=0)
            {
                PlayerBag.itemList[fromIndex] = targetItem;
                PlayerBag.itemList[targetIndex] = currentItem;
            }
            else
            {
                PlayerBag.itemList[targetIndex] = currentItem;
                PlayerBag.itemList[fromIndex] = new InventoryItem();
            }
            EventHandler.CallUpdateInventoryUI(InventoryLocation.Player,PlayerBag.itemList);
        }
    }
}

