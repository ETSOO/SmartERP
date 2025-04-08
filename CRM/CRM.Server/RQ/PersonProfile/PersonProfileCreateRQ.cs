using com.etsoo.CoreFramework.Application;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.CoreFramework.Business;
using com.etsoo.Utils.Actions;
using com.etsoo.Utils.Models;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile create request data
    /// 人员档案创建请求数据
    /// </summary>
    public record PersonProfileCreateRQ : IModelValidator
    {
        /// <summary>
        /// Person id
        /// 人员编号
        /// </summary>
        public long PersonId { get; init; }

        /// <summary>
        /// Other participants
        /// 其他参与者
        /// </summary>
        public IEnumerable<long>? Persons { get; init; }

        /// <summary>
        /// Order / purchase id
        /// 订单 / 采购编号
        /// </summary>
        public long? OrderId { get; init; }

        /// <summary>
        /// Kind
        /// 类型
        /// </summary>
        public PersonProfileKind Kind { get; init; }

        /// <summary>
        /// Title
        /// 标题
        /// </summary>
        public required string Title { get; init; }

        /// <summary>
        /// Comment
        /// 评论
        /// </summary>
        public required string Comment { get; init; }

        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public string? Location { get; init; }

        /// <summary>
        /// Location id
        /// 位置编号
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// Happen date
        /// 发生日期
        /// </summary>
        public DateTimeOffset? HappenDate { get; init; }

        /// <summary>
        /// Happen date end
        /// 发生日期结束
        /// </summary>
        public DateTimeOffset? HappenDateEnd { get; init; }

        /// <summary>
        /// User role for privacy
        /// 控制隐私的用户角色
        /// </summary>
        public UserRole? UserRole { get; init; }

        /// <summary>
        /// JSON data
        /// JSON 数据
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Status
        /// 状况
        /// </summary>
        public EntityStatus Status { get; init; }

        /// <summary>
        /// Importance
        /// 重要性
        /// </summary>
        public PersonProfileImportance? Importance { get; init; }

        /// <summary>
        /// Assignee id
        /// 经办人
        /// </summary>
        public long? AssigneeId { get; init; }

        /// <summary>
        /// Validate the model
        /// 验证模块
        /// </summary>
        /// <returns>Result</returns>
        public IActionResult? Validate()
        {
            if (Title.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Title));
            }

            if (Kind >= PersonProfileKind.Schedule)
            {
                // Larger kinds are used for system internal use
                return ApplicationErrors.NoValidData.AsResult(nameof(Kind));
            }

            if (Location != null && Location.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Location));
            }

            return null;
        }
    }
}
