using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("replies")]
    public class Reply
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("topic_id")]
        [ForeignKey("Topic")]
        public int TopicID { get; set; }

        public virtual Topic Topic { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }
    }
}