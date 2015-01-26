using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    public enum ReviewLevel
    {
        Good,
        Medium,
        Bad
    }

    [Table("reviews")]
    public class Review
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("level")]
        public int LevelAsInt { get; set; }

        [NotMapped]
        public ReviewLevel Level
        {
            get { return (ReviewLevel)LevelAsInt; }
            set { LevelAsInt = (int)value; }
        }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("comment")]
        public string Comment { get; set; }
    }
}
