using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("test_case_paids")]
    public class TestCasePaid
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("test_case_id")]
        [ForeignKey("TestCase")]
        public int TestCaseID { get; set; }

        public virtual TestCase TestCase { get; set; }
    }
}
