using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    [Table("XpLedger")]
    public class XpLedgerEntry
    {
        [Key]
        public int XpLedgerId { get; set; }

        public int UserId { get; set; }

        public int Amount { get; set; }

        public string Source { get; set; } = string.Empty;

        public string? RefKey { get; set; }

        public DateTime CreatedUtc { get; set; }

        public User? User { get; set; }
    }
}
