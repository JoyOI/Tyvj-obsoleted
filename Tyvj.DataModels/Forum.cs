using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("forums")]
    public class Forum
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("sort")]
        public int Sort { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("father_id")]
        [ForeignKey("Father")]
        public int FatherID { get; set; }

        public virtual Forum Father { get; set; }

        public virtual ICollection<Forum> Children { get; set; }
    }
}