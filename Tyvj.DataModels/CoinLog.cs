using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    [Table("CoinLogs")]
    public class CoinLog
    {
        public int ID { get; set; }

        [ForeignKey("Giver")]
        public int GiverUserID { get; set; }

        public virtual User Giver { get; set; }

        [ForeignKey("Receiver")]
        public int ReceiverUserID { get; set; }

        public virtual User Receiver { get; set; }

        public int Count { get; set; }

        public DateTime Time { get; set; }
    }
}
