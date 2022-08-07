using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farm.Inventory
{
    public class SlotUI : MonoBehaviour,IPointerClickHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("组件获取")] 
        [SerializeField] private Image slotImage;

        [SerializeField] private TextMeshProUGUI amounttext;
        [SerializeField] public Image highLightSlot;
        [SerializeField] private Button button;
        [Header("格子类型")]
        public SlotType slotType;

        public bool isSelected;
        public int slotIndex;
    
        //物品信息
        public ItemDetails itemDetails;
        public int itemAmount;

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();

        private void Start()
        {
            isSelected = false;
            if (itemDetails.itemID==0)
            {
                UpdateEmptySlot();
            }
        }

        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amounttext.text = amount.ToString();
            slotImage.enabled = true;
            button.interactable = true;
        }

        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;
                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails,isSelected);
            }
            slotImage.enabled = false;
            slotImage.sprite = null;
            itemAmount=0;
            amounttext.text = string.Empty;
            button.interactable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemAmount == 0) return; 
            isSelected=!isSelected;
            
            inventoryUI.UpdateSlotHighlight(slotIndex);

            if (slotType == SlotType.Bag)
            {
                EventHandler.CallItemSelectedEvent(itemDetails,isSelected);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemAmount != 0)
            {
                inventoryUI.dragItem.enabled = true;
                inventoryUI.dragItem.sprite = slotImage.sprite;
                inventoryUI.dragItem.SetNativeSize();

                isSelected = true;
                inventoryUI.UpdateSlotHighlight(slotIndex);
                UpdateEmptySlot();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.transform.position = Input.mousePosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragItem.enabled = false;

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>()==null)
                {
                    return;
                }

                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                var targeIndex = targetSlot.slotIndex;

                if (slotType==SlotType.Bag&&targetSlot.slotType==SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex,targeIndex);
                }
                inventoryUI.UpdateSlotHighlight(-1);
            }
            // else
            // {
            //     if (itemDetails.canDropped)
            //     {
            //         var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            //             -Camera.main.transform.position.z));
            //         EventHandler.CallInstantiateItemInScene(itemDetails.itemID, pos);
            //     }
            // }
        }
    }
}

