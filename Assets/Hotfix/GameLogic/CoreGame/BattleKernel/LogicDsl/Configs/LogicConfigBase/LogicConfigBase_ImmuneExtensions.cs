using System.Runtime.CompilerServices;

namespace LccHotfix
{
    public partial class LogicConfigBase
    {
        /// <summary>
        /// 布尔类型免疫节点，需要主动检查。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmuneBhvCfg ImmuneBool(int boolImmune, int flag = 0)
        {
            return new ImmuneBhvCfg(flag).WithBool(boolImmune);
        }

        /// <summary>
        /// 万分比类型免疫节点，需要主动检查。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmuneBhvCfg ImmunePermyriad(int permyriadImmune, int value, int flag = 0)
        {
            return new ImmuneBhvCfg(flag).WithPermyriad(permyriadImmune, value);
        }

        /// <summary>
        /// Buff ID 免疫节点，buff 系统自动处理。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmuneBhvCfg ImmuneBuffId(int buffId, int value, int flag = 0)
        {
            return new ImmuneBhvCfg(flag).WithBuffId(buffId, value);
        }

        /// <summary>
        /// Buff Tag 免疫节点，buff 系统自动处理。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ImmuneBhvCfg ImmuneBuffTag(int buffTag, int value, int flag = 0)
        {
            return new ImmuneBhvCfg(flag).WithBuffTag(buffTag, value);
        }
    }
}
