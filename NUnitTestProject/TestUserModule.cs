using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace com.etsoo.Core.UnitTests
{
    /// <summary>
    /// User module for test
    /// </summary>
    [Table("e_user")]
    public class TestUserModule
    {
        /// <summary>
        /// Auto generated id
        /// 自动编号
        /// </summary>
        [Key]
        [Column("id")]
        public int? Id { get; set; }

        /// <summary>
        /// Name
        /// 姓名
        /// </summary>
        [Required]
        [StringLength(128)]
        [Column("name")]
        public string Name { get; set; }
    }
}