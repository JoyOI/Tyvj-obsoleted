using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("problems")]
    public class Problem
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("background")]
        public string Background { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("input")]
        public string Input { get; set; }

        [Column("output")]
        public string Output { get; set; }

        [Column("hint")]
        public string Hint { get; set; }

        [Column("standard_program")]
        public string StandardProgram { get; set; }

        [Column("time_limit")]
        public int TimeLimit { get; set; }

        [Column("memory_limit")]
        public int MemoryLimit { get; set; }

        [Column("standard_problem_language")]
        public int StandardProgramLanguageAsInt { get; set; }

        [NotMapped]
        public Language StandardProgramLanguage
        {
            get { return (Language)StandardProgramLanguageAsInt; }
            set { StandardProgramLanguageAsInt = (int)value; }
        }

        [Column("special_judge")]
        public string SpecialJudge { get; set; }

        [Column("special_judge_language")]
        public int SpecialJudgeLanguageAsInt { get; set; }

        [NotMapped]
        public Language SpecialJudgeLanguage
        {
            get { return (Language)SpecialJudgeLanguageAsInt; }
            set { SpecialJudgeLanguageAsInt = (int)value; }
        }

        [Column("range_validator")]
        public string RangeValidator { get; set; }

        [Column("range_validator_language")]
        public int RangeValidatorLanguageAsInt { get; set; }

        [NotMapped]
        public Language RangeValidatorLanguage
        {
            get { return (Language)RangeValidatorLanguageAsInt; }
            set { RangeValidatorLanguageAsInt = (int)value; }
        }

        [Column("official")]
        public bool Official { get; set; }

        public virtual ICollection<TestCase> TestCases { get; set; }

        public virtual ICollection<Status> Statuses { get; set; }

        public override bool Equals(object obj)
        {
            var data = obj as Problem;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}