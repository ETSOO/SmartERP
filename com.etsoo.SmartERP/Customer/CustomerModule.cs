using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace com.etsoo.SmartERP.Customer
{
    /// <summary>
    /// Customer Module
    /// 客户模型
    /// </summary>
    [Table("e_customer")]
    public class CustomerModule
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
        /// 名称
        /// </summary>
        [Required]
        [StringLength(128)]
        [Column("name")]
        public string Name { get; set; }
    }
}