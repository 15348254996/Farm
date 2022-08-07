using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Crops;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Farm.Map
{
    public class GridMapManager : Singleton<GridMapManager>
    {
        [Header("种地瓦片切换信息")] 
        public RuleTile digTile;
        public RuleTile waterTile;
        private Tilemap waterTilemap;
        private Tilemap digTilemap;
        [Header("地图信息")] 
        public List<MapData_SO> mapDataList;
        
        private Season currentSeason;

        

        private Dictionary<string, TileDetails> tileDetailsDict = new Dictionary<string, TileDetails>();

        private Dictionary<string, bool> firstLoadDict = new Dictionary<string, bool>();



        private List<ReapItem> itemsInRaduis;
        private Grid currentGrid;
        private void OnEnable()
        {
            EventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent+=OnGameDayEvent;
            EventHandler.RefreshCurrentMap+=OnRefreshCurrentMap;
        }

        

        private void OnDisable()
        {
            EventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimation;
            EventHandler.AfterSceneLoadedEvent-=OnAfterSceneLoadedEvent;
            EventHandler.GameDayEvent-=OnGameDayEvent;
            EventHandler.RefreshCurrentMap-=OnRefreshCurrentMap;
        }
        

        private void Start()
        {
            foreach (var mapData in mapDataList)
            {
                firstLoadDict.Add(mapData.sceneName,true);
                InitTileDetailsDict(mapData);
            }
        }

        private void InitTileDetailsDict(MapData_SO mapData)
        {
            foreach (var tileProperty in mapData.TileProperties)
            {
                TileDetails tileDetails = new TileDetails
                {
                    gridpos = new Vector2Int(tileProperty.tileCoordinate.x,tileProperty.tileCoordinate.y)
                    
                };
                string key = tileDetails.gridpos.x + "x" + tileDetails.gridpos.y + "y" + mapData.sceneName;

                if (GetTileDetails(key)!=null)
                {
                    tileDetails = GetTileDetails(key);
                }

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        tileDetails.canDig = tileProperty.boolTypeValue;
                        break;
                    case GridType.DragItem:
                        tileDetails.canDropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        tileDetails.canPlaceFurniture = tileProperty.boolTypeValue;
                        break;
                    case GridType.NPCObstacle:
                        tileDetails.isNPCObstacle = tileProperty.boolTypeValue;
                        break;
                }

                if (GetTileDetails(key)!=null)
                {
                    tileDetailsDict[key] = tileDetails;
                }
                else
                {
                    tileDetailsDict.Add(key,tileDetails);
                }
            }
        }

        private TileDetails GetTileDetails(string key)
        {
            if (tileDetailsDict.ContainsKey(key))
            {
                return tileDetailsDict[key];
            }

            return null;
        }

        public TileDetails GetTileDetailsOnMousePosition(Vector3Int mouseGridPos)
        {
            string key = mouseGridPos.x + "x" + mouseGridPos.y + "y"+SceneManager.GetActiveScene().name;
            return GetTileDetails(key);
        }
        
        
        private void OnExecuteActionAfterAnimation(Vector3 mouseWorldPos, ItemDetails itemDetails)
        {
            var currentGridPos = currentGrid.WorldToCell(mouseWorldPos);
            var currentTile = GetTileDetailsOnMousePosition(currentGridPos);

            if (currentTile!=null)
            {
                Crop currentCrop = GetCropObject(mouseWorldPos);
                switch (itemDetails.itemType)
                {
                    //WORKFLOW:物品信息实际使用功能
                    case ItemType.Seed:
                        EventHandler.CallPlantSeedEvent(itemDetails.itemID,currentTile);
                        EventHandler.CallDropItemEvent(itemDetails.itemID,new Vector3(mouseWorldPos.x,mouseWorldPos.y,0),itemDetails.itemType);
                        break;
                    case ItemType.Commodity:
                        EventHandler.CallDropItemEvent(itemDetails.itemID,new Vector3(mouseWorldPos.x,mouseWorldPos.y,0),itemDetails.itemType);
                        break;
                    case ItemType.HoeTool:
                        SetDigGround(currentTile);
                        currentTile.daySinceDug = 0;
                        currentTile.canDig = false;
                        currentTile.canDropItem = false;
                        //TODO 音效
                        break;
                    case ItemType.Water:
                        SetWaterGround(currentTile);
                        currentTile.daysSinceWatered = 0;
                        //TODO 音效
                        break;
                    case ItemType.CollectTool:
                        currentCrop.ProcessToolAction(itemDetails,currentTile);
                        break;
                    case ItemType.BreakTool:
                    case ItemType.ChopTool:
                        if (currentCrop != null)
                        {
                            currentCrop.ProcessToolAction(itemDetails, currentCrop.tileDetails);
                        }
                        break;
                    case ItemType.ReapTool:
                        for (int i = 0; i < itemsInRaduis.Count; i++)
                        {
                            EventHandler.CallParticleEffectEvent(ParticaleEffectType.Reap,itemsInRaduis[i].transform.position+Vector3.up);
                            itemsInRaduis[i].SpawnHarvestItems();
                            Destroy(itemsInRaduis[i].gameObject);
                        }
                        break;

                }
            }
            UpdateTileDetails(currentTile);
        }

        public Crop GetCropObject(Vector3 pos)
        {
            Collider2D[] colliders = Physics2D.OverlapPointAll(pos);
            Crop currentCrop = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponent<Crop>())
                {
                    currentCrop = colliders[i].GetComponent<Crop>();
                }
            }
            return currentCrop;
        }

        public bool HaveReapableItemsInRadius(Vector3 pos, ItemDetails tool)
        {
            itemsInRaduis = new List<ReapItem>();
            Collider2D[] collider2Ds = new Collider2D[20];
            Physics2D.OverlapCircleNonAlloc(pos, tool.itemUseRadius, collider2Ds);
            if (collider2Ds.Length>0)
            {
                for (int i = 0; i < collider2Ds.Length; i++)
                {
                    if (collider2Ds[i]!=null)
                    {
                        if (collider2Ds[i].GetComponent<ReapItem>())
                        {
                            var item = collider2Ds[i].GetComponent<ReapItem>();
                            itemsInRaduis.Add(item);
                        }
                    }
                }
            }

            return itemsInRaduis.Count > 0;
        }
        
        private void OnAfterSceneLoadedEvent()
        {
            currentGrid= FindObjectOfType<Grid>();
            digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            waterTilemap = GameObject.FindWithTag("Water").GetComponent<Tilemap>();
            
            //预先生成农作物
            if (firstLoadDict[SceneManager.GetActiveScene().name])
            {
                EventHandler.CallGenerateCropEvent();
                firstLoadDict[SceneManager.GetActiveScene().name] = false;
            }
            
            RefreshMap();
        }
        
        private void OnGameDayEvent(int gameDay, Season gameSeason)
        {
            currentSeason = gameSeason;

            foreach (var tile in tileDetailsDict)
            {
                if (tile.Value.daysSinceWatered>-1)
                {
                    tile.Value.daysSinceWatered = -1;
                }
                if (tile.Value.daySinceDug>-1)
                {
                    tile.Value.daySinceDug++;
                }

                if (tile.Value.daySinceDug>=5&&tile.Value.seedItemId==-1)
                {
                    tile.Value.daySinceDug = -1;
                    tile.Value.canDig = true;
                    tile.Value.growthDays = -1;
                }

                if (tile.Value.seedItemId!=-1)
                {
                    tile.Value.growthDays++;
                }
            }
            RefreshMap();
        }
        
        private void OnRefreshCurrentMap()
        {
            RefreshMap();
        }
        
        private void SetDigGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridpos.x, tile.gridpos.y, 0);
            if (digTilemap != null)
                digTilemap.SetTile(pos, digTile);
        }
        private void SetWaterGround(TileDetails tile)
        {
            Vector3Int pos = new Vector3Int(tile.gridpos.x, tile.gridpos.y, 0);
            if (waterTilemap != null)
                waterTilemap.SetTile(pos, waterTile);
        }

        public void UpdateTileDetails(TileDetails tileDetails)
        {
            string key = tileDetails.gridpos.x + "x" + tileDetails.gridpos.y + "y" + SceneManager.GetActiveScene().name;
            if (tileDetailsDict.ContainsKey(key))
            {
                tileDetailsDict[key] = tileDetails;
            }
            else
            {
                tileDetailsDict.Add(key,tileDetails);
            }
        }

        private void RefreshMap()
        {
            if (digTilemap != null)
                digTilemap.ClearAllTiles();
            if (waterTilemap != null)
                waterTilemap.ClearAllTiles();
            foreach (var crop in FindObjectsOfType<Crop>())
            {
                Destroy(crop.gameObject);
            }
            DisplayMap(SceneManager.GetActiveScene().name);
        }
        private void DisplayMap(string sceneName)
        {
            foreach (var tile in tileDetailsDict)
            {
                var key = tile.Key;
                var tileDetails = tile.Value;

                if (key.Contains(sceneName))
                {
                    if (tileDetails.daySinceDug>-1)
                        SetDigGround(tileDetails);
                    if (tileDetails.daysSinceWatered>-1)
                        SetWaterGround(tileDetails);
                    if (tileDetails.seedItemId>-1)
                        EventHandler.CallPlantSeedEvent(tileDetails.seedItemId,tileDetails);
                }
            }
        }
    }
}

