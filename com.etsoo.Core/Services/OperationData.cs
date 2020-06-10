using System;
using System.Collections.Generic;

namespace com.etsoo.Core.Services
{
    /// <summary>
    /// Database operation data
    /// 数据库操作数
    /// </summary>
    public class OperationData
    {
        /// <summary>
        /// Data format
        /// 数据格式
        /// </summary>
        public DataFormat Format { get; set; }

        /// <summary>
        /// Procedure name
        /// 调用的存储过程名称
        /// </summary>
        /// <value>读写</value>
        public string Procedure { get; set; }

        /// <summary>
        /// Parameters, carefully to set directly
        /// 参数，直接替换时需慎重
        /// </summary>
        public Dictionary<string, dynamic> Parameters { get; set; }

        /// <summary>
        /// Test result
        /// 测试结果
        /// </summary>
        public OperationResult TestResult { get; private set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        public OperationData()
        {
            Parameters = new Dictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
            TestResult = new OperationResult();
        }
    }
}