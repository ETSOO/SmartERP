using com.etsoo.Core.Services;
using System;
using System.Collections.Generic;

namespace com.etsoo.Core.Database
{
    /// <summary>
    /// Data parameter parser interface
    /// 数据参数解析器接口
    /// </summary>
    public interface IDataParameterParser
    {
        /// <summary>
        /// Add data parameter
        /// 添加数据列表参数
        /// </summary>
        /// <typeparam name="T">Type generic</typeparam>
        /// <param name="items">Data list</param>
        /// <param name="parameters">Operation data</param>
        /// <param name="name">Parameter name</param>
        /// <param name="hasRowIndex">Has row index</param>
        /// <param name="flag">Flag value</param>
        void AddDataParameter<T>(IEnumerable<T> items, Dictionary<string, dynamic> parameters, string name, bool? hasRowIndex, bool flag = false) where T : IComparable;
    }
}