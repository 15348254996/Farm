using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Farm.Crops
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails cropDetails;

        private Transform playerTransform => FindObjectOfType<Player>().transform;
        

        public void InitCropData(int ID)
        {
            cropDetails = CropManager.Instance.GetCropDetails(ID);
        }
        public void SpawnHarvestItems()
        {
            Debug.Log(cropDetails);
            for (int i = 0; i < cropDetails.producedItemID.Length; i++)
            {
                int amountToProduce;
                if (cropDetails.producedMinAmount[i]==cropDetails.producedMaxAmount[i])
                {
                    amountToProduce = cropDetails.producedMinAmount[i];
                }
                else
                {
                    amountToProduce = Random.Range(cropDetails.producedMinAmount[i], cropDetails.producedMaxAmount[i] + 1);
                }

                for (int j = 0; j < amountToProduce; j++)
                {
                    if(cropDetails.generateAtPlayerPosition)
                        EventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i]);
                    else
                    {
                        var dirX = transform.position.x > playerTransform.position.x ? 1 : -1;
                        var spawnPos =
                            new Vector3(transform.position.x + Random.Range(dirX, cropDetails.spawnRadius.x * dirX),
                                transform.position.y +
                                Random.Range(-cropDetails.spawnRadius.y, cropDetails.spawnRadius.y), 0);
                        EventHandler.CallInstantiateItemInScene(cropDetails.producedItemID[i],spawnPos);
                    }
                }
            }
        }
    }
}

