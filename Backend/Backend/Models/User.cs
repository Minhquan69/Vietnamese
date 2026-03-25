using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Backend.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int RoleId { get; set; }
        public int Status { get; set; }
        public ICollection<Video> Videos { get; set; }
    }
}
