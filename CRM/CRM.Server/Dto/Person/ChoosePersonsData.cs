namespace CRM.Server.Dto.Person
{
    /// <summary>
    /// Choose persons data
    /// 选择人员数据
    /// </summary>
    public record ChoosePersonsData
    {
        /// <summary>
        /// Users
        /// 用户
        /// </summary>
        public required IEnumerable<PersonListItem> Users { get; init; }

        /// <summary>
        /// Contacts
        /// 联系人
        /// </summary>
        public required IEnumerable<PersonListItem> Contacts { get; init; }
    }
}
