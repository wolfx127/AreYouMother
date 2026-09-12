using Taffy.Play.Enemy;

namespace Taffy.Play.Algorithm
{
    /// <summary>
    /// 敌人“攻击一次”的桥。
    ///
    /// TODO 空壳：项目里还没有敌人攻击系统——
    ///   EnemyData 的 ECS 攻击组件（CloseAttackComp/RemoteAttackComp/PoisonComp）都还没人消费，
    ///   EnemySystem 里的攻击 System 也还没写，BulletPool 目前只有玩家在取。
    /// 等攻击系统落地后，在这里调用真实 API，例如：
    ///   - 近战：对攻击半径内玩家结算伤害（敌人 entity 上的 CloseAttackComp.ATK）；
    ///   - 远程：BulletPool.Instance.GetBullet() 朝玩家发射（isEnemyLaunched = true）；
    ///   - 附带触发攻击动画/音效。
    /// </summary>
    public static class EnemyAttackBridge
    {
        /// <summary>攻击一下（空壳，等攻击系统落地后实现）。</summary>
        public static void AttackOnce(EnemyData enemy)
        {
            // TODO: 接真实攻击 API
        }
    }
}
