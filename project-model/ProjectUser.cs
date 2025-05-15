using Microsoft.AspNetCore.Identity;

namespace project_model
{
    public class ProjectUser : IdentityUser
    {
        //Link for FK
        public UserOrigin UserOrigin { get; set; }
    }
}
