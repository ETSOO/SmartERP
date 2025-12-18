using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;
using com.etsoo.WebUtils.Attributes;
using CRM.Server.Endpoints;
using CRM.Server.RQ.Person;
using CRM.Server.RQ.PersonProfile;
using NpgsqlTypes;
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

        /// <summary>
        /// Create address from request data
        /// 从请求数据创建地址对象
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="personId">Person id</param>
        /// <returns>Result</returns>
        public static PersonAddress CreateAddressFromRQ(this AddressCreateRQ rq, long personId)
        {
            return new PersonAddress
            {
                PersonId = personId,
                Kind = rq.Kind,
                Name = rq.Name,
                PlaceId = rq.PlaceId,
                Region = rq.Region,
                State = rq.State,
                City = rq.City,
                District = rq.District,
                Route = rq.Route,
                Street = rq.Street,
                PostalCode = rq.PostalCode,
                FormattedAddress = rq.FormattedAddress,
                Location = rq.Location == null ? null : new NpgsqlPoint(rq.Location.Lng, rq.Location.Lat),
                Provider = rq.Provider
            };
        }

        /// <summary>
        /// Validate person info
        /// 验证人员信息
        /// </summary>
        /// <param name="kind">Kind</param>
        /// <param name="identifier">Identifier</param>
        /// <returns>Result</returns>
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
