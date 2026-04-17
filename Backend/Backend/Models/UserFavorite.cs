using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class UserFavorite
    {
        public int UserFavoriteId { get; set; }
        public int UserId { get; set; }
        public int UnitId { get; set; }
        public DateTime SavedDate { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }
        
    }
}
