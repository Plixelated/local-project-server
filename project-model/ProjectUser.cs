using Microsoft.AspNetCore.Identity;

namespace project_model
{
    public class ProjectUser : IdentityUser
    {
        public UserOrigin UserOrigin { get; set; }
    }
}
