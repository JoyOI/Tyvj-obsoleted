using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("topics")]
    public class Topic
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("top")]
        public bool Top { get; set; }

        [Column("last_reply")]
        public DateTime LastReply { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("forum_id")]
        [ForeignKey("Forum")]
        public int ForumID { get; set; }

        public virtual Forum Forum { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("reward")]
        public int Reward { get; set; }

        public virtual ICollection<Reply> Replies { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Topic;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}