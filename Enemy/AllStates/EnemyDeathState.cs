using UnityEngine;




public class EnemyDeathState : EnemyState
{
    AnimatorStateInfo m_AnimatorStateInfo;



    public EnemyDeathState(Enemy enemy, EnemyStateMachine stateMachine, SO_EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName)
    {
    }



    public override void Enter()
    {
        enemy.Parameter.PlayerTarget = null;      //敌人进入死亡状态后将Target坐标清零，防止出现bug

        m_AnimatorStateInfo = core.Animator.GetCurrentAnimatorStateInfo(0);       //获取当前动画

        if (!m_AnimatorStateInfo.IsName("Death"))       //检查当前是否在播放死亡动画，如果没有则播放
        {          
            core.Animator.SetBool("Death", true);
        }
    }

    public override void LogicUpdate() { }    //不需要执行此函数在父类中的逻辑

    //不需要执行此函数在父类中的逻辑，防止出现敌人在某些情况下同时进入包含死亡的两个状态从而卡死在原地（在动画帧事件中会将Death布尔设置为false）
    public override void Exit() { }           
}