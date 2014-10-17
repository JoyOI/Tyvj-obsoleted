using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("contest_problems")]
    public class ContestProblem
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("contest_id")]
        [ForeignKey("Contest")]
        public int ContestID { get; set; }

        public virtual Contest Contest { get; set; }

        [Column("problem_id")]
        [ForeignKey("Problem")]
        public int ProblemID { get; set; }

        public virtual Problem Problem { get; set; }

        [Column("number")]
        public string Number { get; set; }

        [Column("point")]
        public int Point { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as ContestProblem;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}