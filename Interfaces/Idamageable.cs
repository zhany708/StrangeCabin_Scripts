using UnityEngine;



public interface Idamageable        //用于所有可以被伤害的物体
{
    void Damage(float amount);      //减少生命值

    //void GetHit(Vector2 direction);     //受击瞬间执行的逻辑（比如转向，改变形态等）
}