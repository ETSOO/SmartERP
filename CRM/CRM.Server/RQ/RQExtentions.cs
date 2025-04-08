using CRM.Server.RQ.PersonProfile;
using PlatformShared.Database.Models;

namespace CRM.Server.RQ
{
    /// <summary>
    /// Request data extentions
    /// 请求数据扩展
    /// </summary>
    public static class RQExtentions
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
    }
}
