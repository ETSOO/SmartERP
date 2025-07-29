using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.RQ.PersonProfile;
using PlatformShared.Database.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CRM.Server.RQ
{
    /// <summary>
    /// Request data extentions
    /// 请求数据扩展
    /// </summary>
    public static partial class RQExtentions
    {
        /// <summary>
        /// Create person profile request from task
        /// 从任务创建人员档案请求
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="personId">Person id</param>
        /// <returns>Person profile create request</returns>
        public static PersonProfileCreateRQ ProfileFromTask(this PersonTaskCreateRQ task, long personId)
        {
            return new PersonProfileCreateRQ
            {
                Auth = task.Auth,
                PersonId = personId,
                Persons = task.Persons,
                OrderId = task.OrderId,
                Kind = PersonProfileKind.Schedule,
                Title = task.Title,
                Comment = task.Comment,
                Location = task.Location,
                LocationId = task.LocationId,
                HappenDate = task.HappenDate,
                HappenDateEnd = task.HappenDateEnd,
                UserRole = task.UserRole,
                Data = task.Data,
                Status = task.Status,
                Importance = task.Importance,
                AssigneeId = task.AssigneeId
            };
        }

        public static IActionResult? ValidatePersonInfo(PersonInfoKind kind, string identifier)
        {
            if (identifier.Length is not (>= 1 and <= 256))
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(identifier));
            }

            switch (kind)
            {
                case PersonInfoKind.Email:
                    var emailValidator = new EmailAddressAttribute();
                    if (!emailValidator.IsValid(identifier))
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(PersonInfoKind.Email));
                    }
                    break;
                case PersonInfoKind.Mobile:
                case PersonInfoKind.Phone:
                    var phoneValidator = new PhoneAttribute();
                    if (!phoneValidator.IsValid(identifier))
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(PersonInfoKind.Phone));
                    }
                    break;
                case PersonInfoKind.QQ:
                    if (!QQRegex().IsMatch(identifier))
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(PersonInfoKind.QQ));
                    }
                    break;
                case PersonInfoKind.WeChat:
                    var wechatValidator = new WechatIdAttribute();
                    if (!wechatValidator.IsValid(identifier))
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(PersonInfoKind.WeChat));
                    }
                    break;
                default:
                    if (!Uri.TryCreate(identifier, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                    {
                        return ApplicationErrors.NoValidData.AsResult(nameof(PersonInfoKind.Website));
                    }
                    break;
            }

            return null;
        }

        [GeneratedRegex(@"^[1-9][0-9]{4,11}$")]
        private static partial Regex QQRegex();
    }
}
