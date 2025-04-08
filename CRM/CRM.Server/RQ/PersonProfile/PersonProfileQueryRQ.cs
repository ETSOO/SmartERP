namespace CRM.Server.RQ.PersonProfile
{
    /// <summary>
    /// Person profile query request data
    /// 人员档案查询请求数据
    /// </summary>
    public record PersonProfileQueryRQ : PersonProfileListRQ
    {
        /// <summary>
        /// Location
        /// 位置
        /// </summary>
        public string? Location { get; init; }
    }
}
