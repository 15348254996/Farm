using Farm.Crops;
using Farm.Inventory;
using Farm.Map;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool,item, seed;
    private Sprite currentSprite;
    private Image cursorImage;
    private RectTransform cursorCanvas;
    
    //鼠标检测
    public Camera mainCamera;
    private Grid currentGrid;
    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;

    private bool cursorPositionValid;

    private ItemDetails currentItem;

    private Transform PlayerTransform => FindObjectOfType<Player>().transform;
    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
    }
    
    

    private void Start()
    {
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        currentSprite = normal;
        SetCursorImage(normal);
        
        mainCamera=Camera.main;
    }

    private void Update()
    {
        if (cursorCanvas==null)
        {
            return;
        }

        cursorImage.transform.position = Input.mousePosition;
        if (!InteractWithUI()&&cursorEnable==true)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(normal);
        }
    } 

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0)&& cursorPositionValid)
        {
            EventHandler.CallMouseClickedEvent(mouseWorldPos,currentItem);
        }
    }

    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }


    private void SetCursorValid() 
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void SetCursorInValid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.4f);
    }
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelevted)
    {
        if (!isSelevted)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
        }
        else
        {
            //WORKFLOW:物品信息实际使用功能
            currentItem = itemDetails;
            currentSprite = itemDetails.itemType switch
            {
                ItemType.Seed=>seed,
                ItemType.ChopTool=>tool,
                ItemType.BreakTool=>tool,
                ItemType.CollectTool=>tool,
                ItemType.HoeTool=>tool,
                ItemType.ReapTool=>tool,
                ItemType.Water=>tool,
                ItemType.Commodity=>item,
                _=>normal
            };
            currentItem = itemDetails;
            cursorEnable = true;
        }
        
    }

    private bool InteractWithUI()
    {
        if (EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        return false;
    }

    private void CheckCursorValid()
    {
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,mainCamera.transform.position.z));
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        var playerGridPos = currentGrid.WorldToCell(PlayerTransform.position);
        if (Mathf.Abs(mouseGridPos.x-playerGridPos.x)>currentItem.itemUseRadius||Mathf.Abs(mouseGridPos.y-playerGridPos.y)>currentItem.itemUseRadius)
        {
            SetCursorInValid();
            return;
        }
        
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
        if (currentTile!=null)
        {
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemId);
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
            //WORKFLOW:物品信息实际使用功能
            switch (currentItem.itemType)
            {
                case ItemType.Seed:
                    if (currentTile.daySinceDug > -1 && currentTile.seedItemId == -1)
                        SetCursorValid();
                    else
                        SetCursorInValid();
                    break;
                case ItemType.Commodity:
                    if (currentTile.canDropItem)
                        SetCursorValid();
                    else
                        SetCursorInValid();
                    break;
                case ItemType.HoeTool:
                    if (currentTile.canDig)
                        SetCursorValid();
                    else
                        SetCursorInValid();
                    break;
                case ItemType.Water:
                    if (currentTile.daySinceDug > -1 && currentTile.daysSinceWatered == -1)
                        SetCursorValid();
                    else
                        SetCursorInValid();
                    break;
                case ItemType.CollectTool:
                    if (currentCrop != null)
                    {
                        if (currentCrop.CheckToolAvailable(currentItem.itemID))
                            if (currentTile.growthDays >= currentCrop.TotalGrowthDays)
                                SetCursorValid();
                            else
                                SetCursorInValid();
                    }
                    else
                        SetCursorInValid();

                    break;
                case ItemType.ChopTool:
                    if (crop != null)
                    {
                        if (crop.CanHarvest&&crop.cropDetails.CheckToolAvailable(currentItem.itemID))
                        {
                            SetCursorValid();
                        }
                        else
                        {
                            SetCursorInValid();
                        }
                    }
                    else
                    {
                        SetCursorInValid();
                    }
                    break;
                case ItemType.BreakTool:
                    if (crop!=null&&(crop.cropDetails.seedItemID==1034||crop.cropDetails.seedItemID==1035)
                                  &&crop.cropDetails.CheckToolAvailable(currentItem.itemID))
                    {
                        SetCursorValid();
                    }
                    else
                    {
                        SetCursorInValid();
                    }
                    break;
                case ItemType.ReapTool:
                    if (GridMapManager.Instance.HaveReapableItemsInRadius(mouseWorldPos,currentItem))
                    {
                        SetCursorValid();
                    }
                    else
                    {
                        SetCursorInValid();
                    }
                    break;
            }
        }
        else
        {
            SetCursorInValid();
        }
    }
    private void OnAfterSceneLoadedEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
    }
    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
    }
}
