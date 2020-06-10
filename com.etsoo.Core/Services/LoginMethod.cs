using System;
using System.Collections.Generic;
using System.Text;

namespace com.etsoo.Core.Services
{
    /// <summary>
    /// Login method
    /// 登录方式
    /// </summary>
    public enum LoginMethod
    {
        /// <summary>
        /// Web
        /// 网页
        /// </summary>
        Web = 1,

        /// <summary>
        /// Desktop software
        /// 桌面软件
        /// </summary>
        Desktop = 2,

        /// <summary>
        /// Mobile app
        /// 手机程序
        /// </summary>
        App = 4
    }
}