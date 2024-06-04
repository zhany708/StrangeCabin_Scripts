using System.Collections.Generic;
using UnityEngine;



public class HellsCall : BaseScreenplay<HellsCall>
{
    public GameObject RitualStone;      //祷告石物体


    public PlayerStats PlayerStats      //Lazy load
    {
        get
        {
            if (m_PlayerStats == null)
            {
                m_PlayerStats = FindObjectOfType<PlayerStats>();
            }
            return m_PlayerStats;
        }
    }
    private PlayerStats m_PlayerStats;



    Coroutine m_HealthDrainCoroutine;       //玩家持续掉血的协程

    List<Vector2> m_TempRoomPos = new List<Vector2>();    //用于储存所有房间字典里的坐标

    string m_RitualRoomName = "RitualRoom";

    bool m_NeedGenerateStone = false;   //判断是否需要生成祷告石
    bool m_CanStartRitual = false;      //判断是否可以开始仪式

    public int m_NeededStoneNum = 2;    //需要生成的祷告石的数量
    int m_GeneratedStoneNum = 0;        //表示当前生成了多少祷告石







    private void OnEnable()
    {
        PlayerStats.OnHealthZero += DestroyCoroutine;       //玩家死亡时停止协程
        RoomManager.Instance.OnRoomGenerated += GenerateStoneAtSingleRoom;      //新生成房间时，调用此函数
    }

    private void OnDisable()
    {
        if (PlayerStats != null)
        {
            PlayerStats.OnHealthZero -= DestroyCoroutine;
        }

        RoomManager.Instance.OnRoomGenerated += GenerateStoneAtSingleRoom;
    }

    private void OnDestroy()
    {
        //将仪式房的名字从列表中移除，因为ScriptObject的记录不会随着游戏结束而消失
        RoomManager.Instance.RoomKeys.FirstFloorRoomKeys.Remove(m_RitualRoomName);
    }



    public async override void StartScreenplay()
    {
        await UIManager.Instance.OpenPanel(UIManager.Instance.UIKeys.HellsCallPanel);   //打开剧本背景界面

        GenerateRitualStones();     //生成祷告石                                                                                                                                                                                                                                                                                                                                                                                                 666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666666
        GenerateRitualRoom();       //生成仪式房
    }





    #region 玩家持续掉血相关
    public void StartHealthDrain()      //开始持续掉血
    {
        if (PlayerStats != null)
        {
            //持续10秒，每次掉5点血，每5秒掉一次
            m_HealthDrainCoroutine = StartCoroutine(PlayerStats.HealthDrain(10f, 5f, 12f));
        }
    }

    private void DestroyCoroutine()     //停止协程
    {
        if (m_HealthDrainCoroutine != null)
        {
            UnityEngine.Debug.Log("Coroutine stopped!!");
            StopCoroutine(m_HealthDrainCoroutine);
        }
    }
    #endregion


    private void AddAllRoomPosIntoList()
    {
        //将字典里的所有坐标储存在列表中
        foreach (var room in RoomManager.Instance.GeneratedRoomDict.Keys)
        {
            m_TempRoomPos.Add(room);
        }

        m_TempRoomPos.Remove(EventManager.Instance.GetRoomPosWhereEnterSecondStage());   //移除触发进入二阶段的房间的坐标，防止玩家立刻获得祷告石
    }



    #region 仪式房相关
    private void GenerateRitualRoom()       //生成仪式房（整个地图只有一个）
    {
        //一行可以生成的房间数量。FloorToInt函数用于将结果向下取整（无论小数部分有多大）
        int allowedRoomNumOnRow = Mathf.FloorToInt(RoomManager.Instance.MaximumXPos * 2 / RoomManager.RoomLength ) + 1;
        //一列可以生成的房间数量
        int allowedRoomNumOnColumn = Mathf.FloorToInt(RoomManager.Instance.MaximumYPos * 2 / RoomManager.RoomWidth) + 1;

        int maxAllowedRoomNum = allowedRoomNumOnRow * allowedRoomNumOnColumn;       //一楼可以生成的最大房间数（当前为35）


        //当没有新的房间可以生成时
        if (RoomManager.Instance.GeneratedRoomDict.Count >= maxAllowedRoomNum)
        {
            AddAllRoomPosIntoList();

            Vector2 selectedRoomPos = GenerateSuitableRandomRoomPos();     //随机选择的房间的坐标
            GameObject deletedRoom = null;                                 //创建坐标对应的房间

            if (RoomManager.Instance.GeneratedRoomDict.TryGetValue(selectedRoomPos, out deletedRoom))   //尝试从字典中获取对应的房间
            {
                Destroy(deletedRoom);       //删除随机坐标对应的房间，随后将仪式房生成在这里
            }

            else
            {
                Debug.LogError("A room has generated here, but cannot get the corresponding gameobject: " + selectedRoomPos);
            }
        }

        else
        {
            RoomManager.Instance.RoomKeys.FirstFloorRoomKeys.Add(m_RitualRoomName);       //将仪式房的名字加进列表，以便后续可以生成
        }
    }

    private Vector2 GenerateSuitableRandomRoomPos()    //生成合适的随机房间坐标（因为某些房间不可更改）
    {
        List<Vector2> importantRoomPos = new List<Vector2>();    //用于储存所有不可更改的房间坐标（比如初始房间等）

        importantRoomPos.Add(Vector2.zero);     //将入口大堂添加进列表    后面要做的：添加其余的一楼初始板块


        Vector2 selectedRoomPos = Vector2.zero;     //用于储存随机选择到的房间的坐标
        int attemptCount = 0;                       //表示尝试了多少次
        const int maxAttemptCount = 50;             //最大尝试次数

        //只要随机到不可更改的房间坐标，就重新获取随机索引
        while (importantRoomPos.Contains(selectedRoomPos) && attemptCount <= maxAttemptCount)
        {
            int randomNum = Random.Range(0, m_TempRoomPos.Count);   //随机房间索引
            selectedRoomPos = m_TempRoomPos[randomNum];     //获取随机选择的房间的坐标

            attemptCount++;     //增加尝试计数
        }

        if (attemptCount > maxAttemptCount)        //Report error if attempCount exceed the maximun allowed counts
        {
            Debug.LogError("Failed to generate a suitable random room position after " + maxAttemptCount + " attempts!");
        }


        return selectedRoomPos;
    }
    #endregion


    #region 祷告石相关
    //尝试生成所有的祷告石（整个剧本只调用一次）
    public void GenerateRitualStones()       //在随机房间生成祷告石
    {
        AddAllRoomPosIntoList();    //将字典里的所有坐标储存在列表中


        //判断房间数量是否足够生成所有祷告石
        if (m_TempRoomPos.Count <= m_NeededStoneNum)      //房间数量不足以生成所有祷告石时
        {
            //需要做的：在后续房间生成后强行生成祷告石
            GenerateSeveralStones(m_TempRoomPos.Count, m_TempRoomPos);      //能生成多少祷告石，就生成多少

            m_NeedGenerateStone = true;
        }

        else
        {
            GenerateSeveralStones(m_NeededStoneNum, m_TempRoomPos);       //生成所有祷告石
        }
    }

    private void GenerateSeveralStones(int generatedNum, List<Vector2> roomPosList)     //生成参数中的数量的祷告石
    {
        //将所需的祷告石全部生成出来
        for (int i = 0; i < generatedNum; i++)
        {
            int randomNum = Random.Range(0, roomPosList.Count);     //随机房间索引
            Vector2 selectedRoomPos = roomPosList[randomNum];               //获取随机选择的房间的坐标
            roomPosList.RemoveAt(randomNum);                                //移除已选择的房间以防止重复

            //在选中的房间生成祷告石
            EnvironmentManager.Instance.GenerateObjectWithParent(RitualStone, RoomManager.Instance.GeneratedRoomDict[selectedRoomPos].transform, selectedRoomPos);

            m_GeneratedStoneNum++;      //增加祷告石计数
        }
    }

    //在单独的房间生成祷告石
    private void GenerateStoneAtSingleRoom(Vector2 roomPos)
    {
        if (m_NeedGenerateStone)
        {
            //在参数中的房间生成祷告石
            EnvironmentManager.Instance.GenerateObjectWithParent(RitualStone, RoomManager.Instance.GeneratedRoomDict[roomPos].transform, roomPos);

            m_GeneratedStoneNum++;      //增加祷告石计数
        }        

        if (m_GeneratedStoneNum >= m_NeededStoneNum)        //判断是否已经生成了足够的祷告石
        {
            m_NeedGenerateStone = false;
        }
    }
    #endregion


    #region Setters
    public void SetCanStartRitual(bool isTrue)
    {
        Debug.Log("Can player start the ritual?" + isTrue);

        m_CanStartRitual = isTrue;
    }
    #endregion


    #region Getters
    public bool GetCanStartRitual()
    {
        return m_CanStartRitual;
    }
    #endregion
}