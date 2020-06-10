using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.etsoo.SmartERP.User
{
    /// <summary>
    /// User Module
    /// 用户模块
    /// </summary>
    [Table("e_user")]
    public class UserModule
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