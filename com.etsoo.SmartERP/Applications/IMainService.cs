using com.etsoo.Core.Application;
using com.etsoo.Core.Services;
using System;

namespace com.etsoo.SmartERP.Applications
{
    /// <summary>
    /// Main service interface
    /// 主服务接口
    /// </summary>
    /// <typeparam name="T">Id type generic</typeparam>
    public interface IMainService<T> : IService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Appliction
        /// 程序对象
        /// </summary>
        new IMainApp Application { get; }
    }
}