namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Organization query interface
    /// 机构查询接口
    /// </summary>
    public interface IOrgRQ
    {
        /// <summary>
        /// Organization id
        /// 机构编号
        /// </summary>
        public int? OrgId { get; set; }
    }
}
