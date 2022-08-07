using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Farm.Crops
{
    public class CropGenerator : MonoBehaviour
    {
        private Grid currentGrid;

        public int seedItemID;

        public int growthDays;


        private void OnEnable()
        {
            EventHandler.GenerateCropEvent += GenerateCrop;
        }

        private void OnDisable()
        {
            EventHandler.GenerateCropEvent -= GenerateCrop;
        }

        private void Awake()
        {
            currentGrid = FindObjectOfType<Grid>();
        }

        private void GenerateCrop()
        {
            Vector3Int cropGridPos = currentGrid.WorldToCell(transform.position);
            if (seedItemID!=0)
            {
                var tile = GridMapManager.Instance.GetTileDetailsOnMousePosition(cropGridPos);

                if (tile==null)
                {
                    tile = new TileDetails();
                }

                tile.gridpos.x = cropGridPos.x;
                tile.gridpos.y = cropGridPos.y;
                tile.daysSinceWatered = -1;
                tile.seedItemId = seedItemID;
                tile.growthDays = growthDays;
                
                GridMapManager.Instance.UpdateTileDetails(tile);
                EventHandler.CallRefreshCurrentMap();
            }
        }
    }
}

