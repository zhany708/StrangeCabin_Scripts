using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



//用于处理游戏过程中的一些动态变化（比如在某个地方生成某个新东西）
public class EnvironmentManager : ManagerTemplate<EnvironmentManager>     
{
    public bool IsGameLost { get; private set; } = false;
    public bool IsFirstTimeEnterGame { get; private set; } = true;




    #region 生成物体相关
    //生成物体，同时将参数中的Transform设置为物体的父物体
    public void GenerateObjectWithParent(GameObject generatedObject, Transform parentTransform, Vector2 generatedPos)
    {
        Instantiate(generatedObject, generatedPos, Quaternion.identity, parentTransform);

        //尝试从父物体那里获取房间脚本组件
        RootRoomController parentObject = parentTransform.GetComponent<RootRoomController>();

        if (parentObject != null)
        {
            //添加新的精灵图到房间脚本的列表中，以便跟房间整体一起调整明暗
            parentObject.AddNewSpriteRenderers();
        }
    }
    #endregion


    #region 生成敌人相关
    //在房间内随机的生成单个敌人
    public void GenerateEnemy(DoorController doorController, GameObject enemyPrefab)
    {
        Vector2 spawnPos = doorController.EnemySpwanPos.GenerateSingleRandomPos();  //生成随机坐标

        EnsureNoFurnitureCollision(ref spawnPos, doorController);      //检查是否跟家具重叠


        //这里的enemy物体是敌人的跟物体（包含巡逻坐标的），在生成的同时赋予物体生成坐标
        GameObject enemyObject = EnemyPool.Instance.GetObject(enemyPrefab, spawnPos);     //从敌人对象池中生成敌人

        InitializeEnemy(enemyObject, doorController);
    }

    //根据房间提前设置的敌人数量生成敌人
    public void GenerateEnemy(DoorController doorController)
    {
        if (doorController.EnemyObjects.Length != 0)   //如果房间有怪物
        {
            List<Vector2> enemySpawnList = doorController.EnemySpwanPos.GenerateMultiRandomPos(doorController.EnemyObjects.Length);     //根据怪物数量生成随机坐标list

            //生成完坐标列表后。检查列表中是否有跟家具重合的坐标
            EnsureNoFurnitureCollision(enemySpawnList, doorController);


            for (int i = 0; i < doorController.EnemyObjects.Length; i++)
            {
                //这里的enemy物体是敌人的跟物体（包含巡逻坐标的），在生成的同时赋予物体生成坐标
                GameObject enemyObject = EnemyPool.Instance.GetObject(doorController.EnemyObjects[i], enemySpawnList[i]);     //从敌人对象池中生成敌人

                //Debug.Log("The enemy spawn position is : " + enemySpawnList[i]);

                InitializeEnemy(enemyObject, doorController);
            }
        }
    }


    //检查列表中的所有坐标处是否有家具
    private void EnsureNoFurnitureCollision(List<Vector2> enemySpawnPosList, DoorController doorController)
    {
        Vector2 checkSize = new Vector2(doorController.PhysicsCheckingXPos, doorController.PhysicsCheckingYPos);      //物理检测的大小

        float adaptiveTolerance = doorController.EnemySpwanPos.GetOverlapTolerance();        //获取检查重复的距离
        int attemptCount = 0;           //用于防止进入无限循环的变量


        while (attemptCount < 100)      //确保不超过最大尝试次数
        {
            bool isOverlap = false;

            for (int i = 0; i < enemySpawnPosList.Count; i++)
            {
                if (!IsPositionEmpty(enemySpawnPosList[i], checkSize, doorController))
                {
                    enemySpawnPosList[i] = doorController.EnemySpwanPos.GenerateNonOverlappingPosition(enemySpawnPosList);

                    doorController.EnemySpwanPos.SetOverlapTolerance(adaptiveTolerance);     //设置新的检查重复的距离

                    isOverlap = true;   //设置布尔以继续检查
                }
            }

            if (!isOverlap) break;      //当没有重复时则退出循环

            attemptCount++;
            adaptiveTolerance -= 0.1f;  //如果实在难以生成不会重复的坐标的话，减少检查重复的距离
        }
    }

    //检查单独的坐标是否跟家具重叠
    private void EnsureNoFurnitureCollision(ref Vector2 enemySpawnPos, DoorController doorController)
    {
        Vector2 checkSize = new Vector2(doorController.PhysicsCheckingXPos, doorController.PhysicsCheckingYPos);      //物理检测的大小

        float adaptiveTolerance = doorController.EnemySpwanPos.GetOverlapTolerance();        //获取检查重复的距离
        int attemptCount = 0;       //用于防止进入无限循环的变量


        while (attemptCount < 100)      //确保不超过最大尝试次数
        {
            //检查是否跟家具重复，不重复的话就退出循环
            if (IsPositionEmpty(enemySpawnPos, checkSize, doorController)) break;    
            

            enemySpawnPos = doorController.EnemySpwanPos.GenerateSingleRandomPos();      //生成新的坐标

            doorController.EnemySpwanPos.SetOverlapTolerance(adaptiveTolerance);     //设置新的检查重复的距离                  

            attemptCount++;
            adaptiveTolerance -= 0.1f;  //如果实在难以生成不会重复的坐标的话，减少检查重复的距离
        }
    }


    //运用物理函数检查要生成的坐标是否有家具
    private bool IsPositionEmpty(Vector2 positionToCheck, Vector2 checkSize, DoorController doorController)
    {
        //第一个参数为中心点，第二个参数为长方形大小（沿着中心各延申一半），第三个参数为角度，第四个参数为检测的目标层级
        Collider2D overlapCheck = Physics2D.OverlapBox(positionToCheck, checkSize, 0f, doorController.FurnitureLayerMask);
        return overlapCheck == null;
    }

    //该函数跟上面一样，只是第三个参数不同
    public bool IsPositionEmpty(Vector2 positionToCheck, Vector2 checkSize, LayerMask checkedLayer)
    {
        Collider2D overlapCheck = Physics2D.OverlapBox(positionToCheck, checkSize, 0f, checkedLayer);
        return overlapCheck == null;
    }


    //生成完敌人后，进行初始化
    private void InitializeEnemy(GameObject enemyObject, DoorController doorController)
    {
        Enemy enemyScript = enemyObject.GetComponentInChildren<Enemy>();
        if (enemyScript == null)
        {
            Debug.LogError("Cannot get the Enemy reference in the " + enemyObject.name);
            return;
        }

        //重置敌人脚本绑定的物体的本地（相对于父物体）坐标。因为敌人从对象池重新生成后，本地坐标会继承死亡前的本地坐标
        enemyScript.ResetLocalPos();

        //设置门控制器的脚本
        enemyScript.SetDoorController(doorController);

        //生成敌人后重置生命，否则重新激活的敌人生命依然为0
        Stats enemyStats = enemyObject.GetComponentInChildren<Stats>();
        if (enemyStats == null)
        {
            Debug.LogError("Cannot get the Stats reference in the " + enemyObject.name);
            return;
        }

        enemyStats.SetCurrentHealth(enemyStats.MaxHealth);
    }
    #endregion


    #region 其余函数
    //每当加载新场景时调用的函数
    public void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        //每当进入一楼场景时都调用以下逻辑
        if (scene.name == "FirstFloor")
        {
            //将布尔设置为false，防止玩家重新游戏后重复打开游戏背景介绍
            IsFirstTimeEnterGame = false;
        }

        //进入其余场景时（目前只有主菜单）
        else
        {
            //重置布尔
            IsGameLost = false;
        }
    }

    //重置游戏
    public void ResetGame()
    {
        //重置布尔
        IsGameLost = false;
    }
    #endregion


    #region Setters
    public void SetIsGameLost(bool isTrue)
    {
        IsGameLost = isTrue;
    }
    #endregion
}