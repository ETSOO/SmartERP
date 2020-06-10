using com.etsoo.Core.Services;
using System;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Service interface
    /// 服务接口
    /// </summary>
    /// <typeparam name="T">Generic for id type</typeparam>
    public partial interface IService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Data model interface
        /// 数据模型接口
        /// </summary>
        public interface IDataModel
        {
            /// <summary>
            /// Parameterize data to database operation
            /// 参数化数据到数据库操作
            /// </summary>
            /// <param name="data">Operation data</param>
            /// <param name="service">Current service</param>
            void Parameterize(OperationData data, IService<T> service);

            /// <summary>
            /// Model validation
            /// 模块自定义验证
            /// </summary>
            /// <param name="result">OperationResult to hold the validation results</param>
            void Validate(OperationResult result);
        }

        /// <summary>
        /// Id support data model interface
        /// 编号数据模块接口
        /// </summary>
        public interface IIdDataModel : IDataModel
        {
            /// <summary>
            /// Id
            /// 编号
            /// </summary>
            T? Id { get; set; }
        }

        /// <summary>
        /// Tiplist data model
        /// 列表数据模块
        /// </summary>
        public interface ITiplistDataModel : IIdDataModel
        {
            /// <summary>
            /// Ids
            /// 编号数组
            /// </summary>
            T[] Ids { get; set; }

            /// <summary>
            /// Hide id
            /// 隐藏的编号
            /// </summary>
            T? HideId { get; set; }

            /// <summary>
            /// Hide ids
            /// 隐藏的编号数组
            /// </summary>
            T[] HideIds { get; set; }

            /// <summary>
            /// Records to read
            /// 读取的记录数
            /// </summary>
            int? Records { get; set; }

            /// <summary>
            /// Search keyword
            /// 查询关键词
            /// </summary>
            string Sc { get; set; }
        }

        /// <summary>
        /// Search condition data model
        /// 查询条件数据模块
        /// </summary>
        public interface ISearchDataModel : ITiplistDataModel
        {
            /// <summary>
            /// Count total records
            /// 统计总记录数
            /// </summary>
            bool? CountTotal { get; set; }

            /// <summary>
            /// View domain, not passed as parameter
            /// 浏览域，不作为参数传递
            /// </summary>
            string Domain { get; set; }

            /// <summary>
            /// View model
            /// 当前浏览模式
            /// </summary>
            string Field { get; set; }

            /// <summary>
            /// Is return layout
            /// 是否返回布局信息
            /// </summary>
            bool? HasLayout { get; set; }

            /// <summary>
            /// Order index, positive numbers indicate ascending order, negative numbers indicate descending order
            /// 排序索引，正数表示升序，负数表示降序
            /// </summary>
            short? OrderIndex { get; set; }

            /// <summary>
            /// Current page
            /// 当前页码
            /// </summary>
            int Page { get; set; }
        }

        /// <summary>
        /// Org search condition data model
        /// 机构查询条件数据模块
        /// </summary>
        public interface ISearchOrgDataModel : ISearchDataModel
        {
            /// <summary>
            /// Full text search keywoards
            /// 全文搜索关键词
            /// </summary>
            string Fulltext { get; set; }

            /// <summary>
            /// Limit to the current organization
            /// 限制到当前机构
            /// </summary>
            bool? LimitToSelf { get; set; }

            /// <summary>
            /// Organization id
            /// 机构编号
            /// </summary>
            int? OrganizationId { get; set; }
        }

        /// <summary>
        /// Address search condition data model
        /// 地址查询条件数据模块
        /// </summary>
        public interface ISearchAddressDataModel : ISearchOrgDataModel
        {
            /// <summary>
            /// Country
            /// 国家
            /// </summary>
            string CountryId { get; set; }

            /// <summary>
            /// Region id
            /// 地区编号
            /// </summary>
            int? RegionId { get; set; }

            /// <summary>
            /// City id
            /// 城市编号
            /// </summary>
            int? CityId { get; set; }

            /// <summary>
            /// District id
            /// 区县编号
            /// </summary>
            int? DistrictId { get; set; }

            /// <summary>
            /// Address
            /// 地址
            /// </summary>
            string Address { get; set; }

            /// <summary>
            /// Postcode
            /// 邮编
            /// </summary>
            string Postcode { get; set; }

            /// <summary>
            /// Postcode part
            /// 邮编部分
            /// </summary>
            string PostcodePart { get; set; }
        }
    }
}