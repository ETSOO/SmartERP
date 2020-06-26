using System;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Module service
    /// 模块服务
    /// </summary>
    /// <typeparam name="T">Id type generics</typeparam>
    public abstract class ModuleService<T> : MainService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Module id
        /// 模块编号
        /// </summary>
        public override byte ModuleId { get; }

        /// <summary>
        /// Protected constructor to prevent initialization
        /// 受保护的构造函数防止实例化
        /// </summary>
        /// <param name="moduleId">Module id</param>
        protected ModuleService(byte moduleId)
        {
            MultipleModule = true;
            ModuleId = moduleId;
        }
    }
}