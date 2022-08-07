public enum ItemType
{
    //种子，商品,家具
    Seed,Commodity,Furniture,
    //锄地工具，砍伐工具，砸石头工具，割草工具，浇水工具，收集工具
    HoeTool,ChopTool,BreakTool,ReapTool,Water,CollectTool,
    //杂草
    ReapableScenery
}

public enum SlotType
{
    Bag,Box,Shop
}

public enum InventoryLocation
{
    Player,Box
}

public enum PartType
{
    None,Carry,Hoe,Water,Collect,Chop,Break,Reap
}

public enum PartName
{
    Body,Hair,Arm,Tool
}

public enum Season
{
    春天,夏天,秋天,冬天
}

public enum GridType
{
    Diggable,DragItem,PlaceFurniture,NPCObstacle
}

public enum ParticaleEffectType
{
    None,LeavesFalling01,LeavesFalling02,Rock,Reap
}