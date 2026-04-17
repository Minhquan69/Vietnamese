using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class UserProgress
    {
        [Key]
        public int UserProgressId { get; set; }
        public int UserId { get; set; }
        public int UnitId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool Status { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }
        
    }
    
}
