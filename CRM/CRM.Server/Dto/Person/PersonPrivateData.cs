using com.etsoo.Database;
using PlatformShared.Database.Models;

namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Person private data
    /// 人员私有数据
    /// </summary>
    public record PersonPrivateData : IUpdateModel
    {
        /// <summary>
        /// Gender
        /// 性别
        /// </summary>
        public string? Gender { get; init; }

        /// <summary>
        /// Birthday
        /// 生日
        /// </summary>
        public DateTimeOffset? Birthday { get; init; }

        /// <summary>
        /// Ethnicity
        /// 种族
        /// </summary>
        public string? Ethnicity { get; init; }

        /// <summary>
        /// Height in cm
        /// 高度（厘米）
        /// </summary>
        public short? Height { get; init; }

        /// <summary>
        /// Weight in kg
        /// 重量（千克）
        /// </summary>
        public decimal? Weight { get; init; }

        /// <summary>
        /// Marital status
        /// 婚姻状况
        /// </summary>
        public PersonMaritalStatus? MaritalStatus { get; init; }

        /// <summary>
        /// Education
        /// 学历
        /// </summary>
        public PersonEducation? Education { get; init; }

        /// <summary>
        /// Education degree
        /// 学位
        /// </summary>
        public PersonDegree? Degree { get; init; }

        /// <summary>
        /// Political status
        /// 政治面貌
        /// </summary>
        public string? PoliticalStatus { get; init; }

        /// <summary>
        /// Changed fields
        /// 修改的字段
        /// </summary>
        public IEnumerable<string>? ChangedFields { get; set; }
    }
}
