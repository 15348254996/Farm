using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        public ItemTooltip itemTooltip;
        [Header("拖拽图片")] 
        public Image dragItem;
        [Header("玩家背包UI")]
        [SerializeField] private GameObject bagUI;//不想让其他组件调用，但是又想在编辑器上使用

        private bool bagOpened;
        
        [SerializeField] private SlotUI[] PlayerSlots;
        
        private void OnEnable()
        {
            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        }

        

        private void OnDisable()
        {
            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        }
        
        private void OnBeforeSceneUnloadEvent()
        {
            UpdateSlotHighlight(-1);
        }

        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
        {
            switch (location)
            {
                case InventoryLocation.Player:
                    for (int i = 0; i < PlayerSlots.Length; i++)
                    {
                        if (list[i].itemAmount>0)
                        {
                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
                            PlayerSlots[i].UpdateSlot(item,list[i].itemAmount);
                        }
                        else
                        {
                            PlayerSlots[i].UpdateEmptySlot();
                        }
                    }
                    break;
            }
        }


        private void Start()
        {
            for (int i = 0; i < PlayerSlots.Length; i++)
            {
                PlayerSlots[i].slotIndex = i;
            }

            bagOpened = bagUI.activeInHierarchy;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                OpenBagUI();
            }
        }

        public void OpenBagUI()
        {
            bagOpened = !bagOpened;
            bagUI.SetActive(bagOpened);
        }

        public void UpdateSlotHighlight(int index)
        {
            foreach (var slot in PlayerSlots)
            {
                if (slot.isSelected && slot.slotIndex == index)
                {
                    slot.highLightSlot.gameObject.SetActive(true);
                }
                else
                {
                    slot.isSelected = false;
                    slot.highLightSlot.gameObject.SetActive(false);
                }
            }
        }
    }
}

