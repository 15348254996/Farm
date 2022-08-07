using System.Collections;
using UnityEngine;

namespace Farm.Crops
{
    public class Crop : MonoBehaviour
    {
        public CropDetails cropDetails;

        public TileDetails tileDetails;
        
        private int harvestActionCount;

        public bool CanHarvest => tileDetails.growthDays >= cropDetails.TotalGrowthDays;

        private Animator anim;

        private Transform playerTransform => FindObjectOfType<Player>().transform;
        
        public void ProcessToolAction(ItemDetails tool,TileDetails tile)
        {
            tileDetails = tile;
            int requireActionCount = cropDetails.GetTotalRequireCount(tool.itemID);
            if (requireActionCount == -1) return;

            anim = GetComponentInChildren<Animator>();
            if (harvestActionCount<requireActionCount)
            {
                harvestActionCount++;
                
                if (anim!=null)
                {
                    if (playerTransform.position.x<transform.position.x)
                    {
                        anim.SetTrigger("RotateRight");
                    }
                    else
                    {
                        anim.SetTrigger("RotateLeft");
                    }
                }

                if (cropDetails.hasParticalEffect)
                {
                    EventHandler.CallParticleEffectEvent(cropDetails.particaleEffectType,transform.position+cropDetails.effectPos);
                }

                //TODO 播放音乐
            }
 
            if (harvestActionCount>=requireActionCount)
            {
                if (cropDetails.generateAtPlayerPosition||!cropDetails.hasAnimation)
                {
                    //生成农作物
                    SpawnHarvestItems();
                }
                else if (cropDetails.hasAnimation)
                {
                    GetComponent<Collider2D>().enabled = false;
                    if (playerTransform.position.x<transform.position.x)
                        anim.SetTrigger("FallRight");
                    else
                        anim.SetTrigger("FallLeft");
                    StartCoroutine(HarvestAfterAnimation());
                }
            }
        }

        private IEnumerator HarvestAfterAnimation()
        {
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("END"))
            {
                yield return null;
            }
            SpawnHarvestItems();

            if (cropDetails.transferItemID>0)
            {
                CreateTransferCrop();
            }
        }


        private void CreateTransferCrop()
        {
            tileDetails.seedItemId = cropDetails.transferItemID;
            tileDetails.daysSinceLastHarvest = -1;
            tileDetails.growthDays = 0;
            EventHandler.CallRefreshCurrentMap();
        }
        public void SpawnHarvestItems()
        {
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

            if (tileDetails!=null)
            {
                tileDetails.daysSinceLastHarvest++;
                if (cropDetails.daysToRegrow>0&&tileDetails.daysSinceLastHarvest<cropDetails.regrowTimes)
                {
                    tileDetails.growthDays = cropDetails.TotalGrowthDays - cropDetails.daysToRegrow;
                    //刷新地图
                    EventHandler.CallRefreshCurrentMap();
                }
                else//不可重复生长，还原地面
                {
                    tileDetails.daysSinceLastHarvest = -1;
                    tileDetails.seedItemId = -1;

                    tileDetails.daySinceDug = -1;
                    tileDetails.canDig = true;
                    EventHandler.CallRefreshCurrentMap();
                }
                
                Destroy(gameObject);
            }
        }
    }
}

