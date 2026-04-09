using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class AuditLog : BaseEntity // ← مهم
    {
        // لو الـ BaseEntity فيه Id, ما محتاجش Id هنا
        // public long Id { get; set; }  ← احذفها لو موجودة في BaseEntity

        public int UserId { get; set; }
        public string Action { get; set; }
        public string IPAddress { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }

        public User User { get; set; }
    }
}
