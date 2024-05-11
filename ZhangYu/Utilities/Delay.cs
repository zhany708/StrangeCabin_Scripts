using System;
using System.Collections;
using UnityEngine;




public class Delay : MonoBehaviour      //用于处理延迟相关的脚本
{
    public static Delay Instance { get; private set; }





    private void Awake()
    {
        //单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        else
        {
            Instance = this;
        }
    }



    public IEnumerator DelaySomeTime(float delay, Action onTimerDone = null)      //用于延迟一段时间后执行一些逻辑
    {
        yield return new WaitForSeconds(delay);

        onTimerDone?.Invoke();
    }



    //等待玩家按空格或鼠标
    public IEnumerator WaitForPlayerInput(Action onInputReceived = null)      
    {
        bool inputReceived = false;     //表示是否接受到玩家的信号，用于决定是否结束循环

        while (!inputReceived)
        {
            //检查玩家是否按下空格或点击鼠标左键
            if (PlayerInputHandler.Instance.IsSpacePressed || PlayerInputHandler.Instance.AttackInputs[(int)CombatInputs.primary])
            {
                inputReceived = true;
                onInputReceived?.Invoke();

                yield break;
            }

            yield return null;  //等待到下一帧为止，从而再次检查
        }
    }
}