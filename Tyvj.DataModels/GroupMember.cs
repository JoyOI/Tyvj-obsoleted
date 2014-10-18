using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("group_members")]
    public class GroupMember
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("group_id")]
        [ForeignKey("Group")]
        public int GroupID { get; set; }

        public virtual Group Group { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as GroupMember;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
