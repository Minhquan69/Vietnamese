namespace Backend.dto
{
    public class UnitDTO
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
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? Status { get; set; }
        public bool IsDelete { get; set; }
    }
}
