using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Unit
    {
        public int UnitId { get; set; }
        public int CourseId { get; set; }
        public string UnitName { get; set; }
        public string VideoUrl { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string Objective { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey("CourseId")] 
        public virtual Course Course { get; set; }
        public virtual Quiz Quiz { get; set; }

        public ICollection<UserProgress> UserProgresses { get; set; }
        public ICollection<UserFavorite> UserFavorites { get; set; }
    }
}
