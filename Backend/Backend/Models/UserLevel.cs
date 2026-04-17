using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class UserLevel
    {
        public int UserLevelId { get; set; }
        public int UserId { get; set; }
        public int LevelId { get; set; }
        public bool Status { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("LevelId")]
        public virtual Level Level { get; set; }
       
    }
}
