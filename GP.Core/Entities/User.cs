using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP.Core.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        //public string FirstName { get; set; }
        //public string LastName { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // "Student" or "Instructor"

        // Navigation
        public Student Student { get; set; }
        public Instructor Instructor { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
    }
}