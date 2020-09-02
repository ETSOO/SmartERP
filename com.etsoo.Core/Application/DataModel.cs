using com.etsoo.Core.Services;
using System;

namespace com.etsoo.Core.Application
{
    /// <summary>
    /// Common service
    /// 通用服务
    /// </summary>
    /// <typeparam name="T">Id type generic</typeparam>
    public abstract partial class Service<T> : IService<T> where T : struct, IComparable
    {
        /// <summary>
        /// Abstract data model
        /// 抽象数据模块
        /// </summary>
        public abstract class DataModel : IService<T>.IDataModel
        {
            /// <summary>
            /// Parameterize data to database operation
            /// 参数化数据到数据库操作
            /// </summary>
            /// <param name="data">Operation data</param>
            /// <param name="service">Current service</param>
            public virtual void Parameterize(OperationData data, IService<T> service)
            {
                Parameterize(data);
            }

            /// <summary>
            /// Simply parameterize data to database operation
            /// 简化的参数化数据到数据库操作
            /// </summary>
            /// <param name="data">Operation data</param>
            protected virtual void Parameterize(OperationData data)
            {

            }

            /// <summary>
            /// Model validation
            /// 模块自定义验证
            /// </summary>
            /// <param name="result">OperationResult to hold the validation results</param>
            public virtual void Validate(OperationResult result)
            {

            }
        }

        /// <summary>
        /// Id support data model
        /// 编号数据模块
        /// </summary>
        public abstract class IdDataModel : DataModel, IService<T>.IIdDataModel
        {
            /// <summary>
            /// Id
            /// 编号
            /// </summary>
            public T? Id { get; set; }

            /// <summary>
            /// Override to collect parameters
            /// 重写收集参数
            /// </summary>
            /// <typeparam name="T">Id type generic</typeparam>
            /// <param name="data">Data</param>
            /// <param name="service">Service</param>
            public override void Parameterize(OperationData data, IService<T> service)
            {
                base.Parameterize(data, service);

                data.Parameters.Add("id", Id);
            }
        }

        /// <summary>
        /// Tiplist data model
        /// 列表数据模块
        /// </summary>
        public abstract class TiplistDataModel : IdDataModel, IService<T>.ITiplistDataModel
        {
            /// <summary>
            /// Ids
            /// 编号数组
            /// </summary>
            public T[] Ids { get; set; }

            /// <summary>
            /// Hide id
            /// 隐藏的编号
            /// </summary>
            public T? HideId { get; set; }

            /// <summary>
            /// Hide ids
            /// 隐藏的编号数组
            /// </summary>
            public T[] HideIds { get; set; }

            /// <summary>
            /// Records to read
            /// 读取的记录数
            /// </summary>
            public int? Records { get; set; }

            /// <summary>
            /// Search keyword
            /// 查询关键词
            /// </summary>
            public string Sc { get; set; }

            /// <summary>
            /// Override to collect parameters
            /// 重写收集参数
            /// </summary>
            /// <typeparam name="T">Id type generic</typeparam>
            /// <param name="data">Data</param>
            /// <param name="service">Service</param>
            public override void Parameterize(OperationData data, IService<T> service)
            {
                base.Parameterize(data, service);

                // Add parameters
                var paras = data.Parameters;

                service.Application.Database.AddDataParameter(Ids, paras, "ids", false);
                paras.Add("hide_id", HideId);
                service.Application.Database.AddDataParameter(HideIds, paras, "hide_ids", false);
                paras.Add("e_max_per_page", Records);
                if (!string.IsNullOrEmpty(Sc))
                    paras.Add("e_sc", Sc);
            }
        }

        /// <summary>
        /// Search condition data model
        /// 查询条件数据模块
        /// </summary>
        public abstract class SearchDataModel : TiplistDataModel, IService<T>.ISearchDataModel
        {
            /// <summary>
            /// Count total records
            /// 统计总记录数
            /// </summary>
            public bool? CountTotal { get; set; }

            /// <summary>
            /// View domain, not passed as parameter
            /// 浏览域，不作为参数传递
            /// </summary>
            public string Domain { get; set; }

            /// <summary>
            /// View model
            /// 当前浏览模式
            /// </summary>
            public string Field { get; set; }

            /// <summary>
            /// Is return layout
            /// 是否返回布局信息
            /// </summary>
            public bool? HasLayout { get; set; }

            /// <summary>
            /// Order index, positive numbers indicate ascending order, negative numbers indicate descending order
            /// 排序索引，正数表示升序，负数表示降序
            /// </summary>
            public short? OrderIndex { get; set; }

            /// <summary>
            /// Current page
            /// 当前页码
            /// </summary>
            public int Page { get; set; } = 1;

            /// <summary>
            /// Override to collect parameters
            /// 重写收集参数
            /// </summary>
            /// <typeparam name="T">Id type generic</typeparam>
            /// <param name="data">Data</param>
            /// <param name="service">Service</param>
            public override void Parameterize(OperationData data, IService<T> service)
            {
                base.Parameterize(data, service);

                // Add parameters
                var paras = data.Parameters;

                if (!string.IsNullOrEmpty(Field) && Field.Length <= 30)
                    paras.Add("e_field", Field);
                paras.Add("e_order_index", OrderIndex);

                // Under tiplist domain, prevent unnecessary parameters to pass
                if(!data.Procedure.Contains("_search_tiplist_"))
                {
                    // Contains layout for the first page as default
                    if (HasLayout == null && Page == 1)
                        HasLayout = true;

                    // Validate page
                    if (Page < 1)
                        Page = 1;

                    paras.Add("e_current_page", Page);
                    paras.Add("e_has_layout", HasLayout);
                    paras.Add("e_count_total", CountTotal);
                }
            }
        }

        /// <summary>
        /// Org search condition data model
        /// 机构查询条件数据模块
        /// </summary>
        public abstract class SearchOrgDataModel : SearchDataModel, IService<T>.ISearchOrgDataModel
        {
            /// <summary>
            /// Full text search keywoards
            /// 全文搜索关键词
            /// </summary>
            public string Fulltext { get; set; }

            /// <summary>
            /// Limit to the current organization
            /// 限制到当前机构
            /// </summary>
            public bool? LimitToSelf { get; set; }

            /// <summary>
            /// Organization id
            /// 机构编号
            /// </summary>
            public int? OrganizationId { get; set; }

            /// <summary>
            /// Override to collect parameters
            /// 重写收集参数
            /// </summary>
            /// <typeparam name="T">Id type generic</typeparam>
            /// <param name="data">Data</param>
            /// <param name="service">Service</param>
            public override void Parameterize(OperationData data, IService<T> service)
            {
                base.Parameterize(data, service);

                // When organization id is null and limit to self is null then set true for it
                if (OrganizationId == null && LimitToSelf == null)
                    LimitToSelf = true;

                // Add parameters
                var paras = data.Parameters;

                if (!string.IsNullOrEmpty(Fulltext))
                    paras.Add("e_fulltext", Fulltext);

                paras.Add("limit_to_self", LimitToSelf);
                paras.Add("organization_id", OrganizationId);
            }
        }

        /// <summary>
        /// Address search condition data model
        /// 地址查询条件数据模块
        /// </summary>
        public abstract class SearchAddressDataModel : SearchOrgDataModel, IService<T>.ISearchAddressDataModel
        {
            /// <summary>
            /// Country
            /// 国家
            /// </summary>
            public string CountryId { get; set; }

            /// <summary>
            /// Region id
            /// 地区编号
            /// </summary>
            public int? RegionId { get; set; }

            /// <summary>
            /// City id
            /// 城市编号
            /// </summary>
            public int? CityId { get; set; }

            /// <summary>
            /// District id
            /// 区县编号
            /// </summary>
            public int? DistrictId { get; set; }

            /// <summary>
            /// Address part
            /// 地址部分
            /// </summary>
            public string Address { get; set; }

            /// <summary>
            /// Postcode
            /// 邮编
            /// </summary>
            public string Postcode { get; set; }

            /// <summary>
            /// Postcode part
            /// 邮编部分
            /// </summary>
            public string PostcodePart { get; set; }

            /// <summary>
            /// Override to collect parameters
            /// 重写收集参数
            /// </summary>
            /// <typeparam name="T">Id type generic</typeparam>
            /// <param name="data">Data</param>
            /// <param name="service">Service</param>
            public override void Parameterize(OperationData data, IService<T> service)
            {
                base.Parameterize(data, service);

                // Add parameters
                var paras = data.Parameters;

                if (!string.IsNullOrEmpty(CountryId))
                    paras.Add("country_id", CountryId);
                if (!string.IsNullOrEmpty(Address))
                    paras.Add("address", Address);
                if (!string.IsNullOrEmpty(Postcode))
                    paras.Add("postcode", Postcode);
                if (!string.IsNullOrEmpty(PostcodePart))
                    paras.Add("postcode_part", PostcodePart);

                paras.Add("region_id", RegionId);
                paras.Add("city_id", CityId);
                paras.Add("district_id", DistrictId);
            }
        }
    }
}