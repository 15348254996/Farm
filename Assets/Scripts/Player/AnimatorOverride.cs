using System.Collections;
using System.Collections.Generic;
using Farm.Inventory;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;
    public SpriteRenderer holdItem;

    [Header("各部分动画列表")] 
    public List<AnimatorType> animatorTypes;
    
    private Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();
    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name,anim);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPosition;
    }

    

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPosition;
    }
    

    private void OnBeforeSceneUnloadEvent()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        PartType currentType = itemDetails.itemType switch
        {
            //WORKFLOW 物品信息实际使用功能
            ItemType.Seed=>PartType.Carry,
            ItemType.HoeTool=>PartType.Hoe,
            ItemType.Water=>PartType.Water,
            ItemType.ChopTool=>PartType.Chop,
            ItemType.CollectTool=>PartType.Collect,
            ItemType.Commodity=>PartType.Carry,
            ItemType.BreakTool=>PartType.Break,
            ItemType.ReapTool=>PartType.Reap,
            _=>PartType.None
        };
        if (isSelected==false)
        {
            currentType = PartType.None;
            SwitchAnimator(currentType);
            holdItem.enabled = false;
        }
        else
        {
            SwitchAnimator(currentType);
            if (currentType==PartType.Carry)
            {
                if (itemDetails.GetitemOnWorldSprite)
                {
                    holdItem.sprite = itemDetails.GetitemOnWorldSprite;
                }
                else
                {
                    holdItem.sprite = itemDetails.itemIcon;
                }
                holdItem.enabled = true;
            }
            else
            {
                holdItem.enabled = false;
                holdItem.sprite = null;
            }
        }
        
    }
    
    private void OnHarvestAtPlayerPosition(int itemID)
    {
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(itemID).GetitemOnWorldSprite;
        if (holdItem.enabled==false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(1f);
        holdItem.enabled = false;
    }
    private void SwitchAnimator(PartType partType)
    {
        foreach (var item in animatorTypes)
        {
            if (item.partType==partType)
            {
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.overrideController;
            }
        }
    }
}
