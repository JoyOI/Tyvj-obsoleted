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
        public static JudgeResult[] FreeResults = { JudgeResult.CompileError, JudgeResult.SystemError, JudgeResult.Pending, JudgeResult.Running, JudgeResult.Accepted };
        public static JudgeResult[] _FreeResults = { JudgeResult.CompileError, JudgeResult.SystemError, JudgeResult.Pending, JudgeResult.Running };
        
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

        [Column("score")]
        public int? Score { get; set; }

        [Column("time_usage")]
        public int? TimeUsage { get; set; }

        [Column("memory_usage")]
        public int? MemoryUsage { get; set; }

        [Column("state_machine_id")]
        public string StateMachineId { get; set; }

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

        public override bool Equals(object obj)
        {
            var data = obj as User;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}