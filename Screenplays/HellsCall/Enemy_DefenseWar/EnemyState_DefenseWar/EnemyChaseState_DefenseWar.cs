using UnityEngine;



public class EnemyChaseState_DefenseWar : EnemyState_DefenseWar
{
    float m_DistanceToAltar;
    float m_DistanceToPlayer;

    public EnemyChaseState_DefenseWar(Enemy_DefenseWar enemy, EnemyStateMachine stateMachine, SO_EnemyData enemyData, string animBoolName) : base(enemy, stateMachine, enemyData, animBoolName)
    {
    }



    public override void LogicUpdate()
    {
        //检查是否有玩家的坐标
        if (enemy.Parameter_DefenseWar.PlayerTarget != null)
        {
            //使敌人朝向玩家
            enemy.EnemyFlip.FlipX( enemy.Movement.GetFlipNum(enemy.Parameter_DefenseWar.PlayerTarget.position, enemy.transform.position) );

            //计算敌人与玩家的距离
            m_DistanceToPlayer = Vector2.Distance(enemy.transform.position, enemy.Parameter_DefenseWar.PlayerTarget.position);       
        }
        
        //检查是否有祷告石的坐标
        else if (enemy.Parameter_DefenseWar.AltarTarget != null)
        {
            //使敌人朝向祷告石
            enemy.EnemyFlip.FlipX( enemy.Movement.GetFlipNum(enemy.Parameter_DefenseWar.AltarTarget.position, enemy.transform.position) );

            //计算敌人与祷告石的距离
            m_DistanceToAltar = Vector2.Distance(enemy.transform.position, enemy.Parameter_DefenseWar.AltarTarget.position);       
        }

        
        base.LogicUpdate();


        //检查玩家是否进入攻击范围
        if (Physics2D.OverlapCircle(enemy.Parameter_DefenseWar.AttackPoint.position, enemyData.AttackArea, enemyData.PlayerLayer) && enemy.CanAttack)
        {
            stateMachine.ChangeState(enemy.AttackState);
        }

        
        else if (enemy.CanAttack)
        {
            //检查祷告石是否进入攻击范围：第一个参数为圆心位置，第二个为半径，第三个为目标图层.玩家处于攻击范围且攻击间隔结束则进入攻击状态
            Collider2D[] detectedColliders = Physics2D.OverlapCircleAll(enemy.Parameter_DefenseWar.AttackPoint.position, enemyData.AttackArea, enemyData.AltarLayer);
            
            foreach (Collider2D collider in  detectedColliders)
            {
                if (collider.CompareTag("Altar") )      //检查碰撞器是否有祷告石标签
                {
                    stateMachine.ChangeState(enemy.AttackState);
                    break;
                }
            }           
        }       
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        //有玩家坐标且与玩家距离大于最小距离时持续追击玩家
        if (enemy.Parameter_DefenseWar.PlayerTarget && m_DistanceToPlayer > enemyData.StoppingDistance)     
        {
            enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, enemy.Parameter_DefenseWar.PlayerTarget.position, enemyData.ChaseSpeed * Time.deltaTime);
        }

        //有祷告石坐标且与祷告石距离大于最小距离时持续追击祷告石
        else if (enemy.Parameter_DefenseWar.AltarTarget && m_DistanceToAltar > enemyData.StoppingDistance)     
        {
            enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, enemy.Parameter_DefenseWar.AltarTarget.position, enemyData.ChaseSpeed * Time.deltaTime);
        }        
    }
}