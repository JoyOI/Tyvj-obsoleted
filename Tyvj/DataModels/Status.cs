using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("statuses")]
    public class Status
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("contest_id")]
        [ForeignKey("Contest")]
        public int? ContestID { get; set; }

        public virtual Contest Contest { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }
        
        [Column("code")]
        public string Code { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("language")]
        public int LanguageAsInt { get; set; }

        [NotMapped]
        public Language Language
        {
            get { return (Language)LanguageAsInt; }
            set { LanguageAsInt = (int)value; }
        }

        public virtual ICollection<JudgeTask> JudgeTasks { get; set; }

        [Column("result")]
        public int ResultAsInt { get; set; }

        [NotMapped]
        public JudgeResult Result
        {
            get { return (JudgeResult)ResultAsInt; }
            set { ResultAsInt = (int)value; }
        }
    }
}