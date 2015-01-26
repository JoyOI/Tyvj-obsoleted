using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tyvj.DataModels
{
    public enum VIPRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    [Table("vip_requests")]
    public class VIPRequest
    {
        [Column("id")]
        public int ID { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("user_id")]
        public int UserID { get; set; }

        public virtual User User { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }

        [Column("status")]
        public int StatusAsInt { get; set; }

        [Column("reason")]
        public string Reason { get; set; }

        [NotMapped]
        public VIPRequestStatus Status
        {
            get { return (VIPRequestStatus)StatusAsInt; }
            set { StatusAsInt = (int)value; }
        }

        public override bool Equals(object obj)
        {
            var data = obj as VIPRequest;
            if (data.ID == this.ID) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return this.ID;
        }
    }
}
