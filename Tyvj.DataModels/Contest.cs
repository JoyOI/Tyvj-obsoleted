using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("contests")]
    public class Contest
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("begin")]
        public DateTime Begin { get; set; }

        [Column("end")]
        public DateTime End { get; set; }

        [Column("format")]
        public int FormatAsInt { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("password")]
        public string Password { get; set; }
        
        [Column("official")]
        public bool Official { get; set; }

        [NotMapped]
        public ContestFormat Format
        {
            get { return (ContestFormat)FormatAsInt; }
            set { FormatAsInt = (int)value; }
        }

        public virtual ICollection<ContestProblem> ContestProblems { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Contest;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
    public enum ContestFormat { OI, ACM, Codeforces };
}