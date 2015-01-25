using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    public enum QuestStatus
    {
        Pending,
        Finished
    }

    [Table("quests")]
    public class Quest
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("status")]
        public int StatusAsInt { get; set; }

        [NotMapped]
        public QuestStatus Status
        {
            get { return (QuestStatus)StatusAsInt; }
            set { StatusAsInt = (int)value; }
        }

        [Column("time")]
        public DateTime Time { get; set; }
    }
}
