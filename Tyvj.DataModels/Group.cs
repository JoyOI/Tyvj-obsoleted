using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("groups")]
    public class Group
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("gravatar")]
        public string Gravatar { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("join_method")]
        public int JoinMethodAsInt { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [NotMapped]
        public GroupJoinMethod JoinMethod
        {
            get { return (GroupJoinMethod)JoinMethodAsInt; }
            set { JoinMethodAsInt = (int)value; }
        }

        public virtual ICollection<GroupMember> Members { get; set; }

        public virtual ICollection<GroupContest> GroupContest { get; set; }

        public virtual ICollection<GroupJoin> GroupJoins { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Group;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum GroupJoinMethod { Everyone, Ratify, Nobody };
}
