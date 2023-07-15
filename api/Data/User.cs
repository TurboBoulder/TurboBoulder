using Microsoft.AspNetCore.Identity;

namespace IdaWebApplicationTemplate.Data
{
    public class User : IdentityUser
    {
        //public UserProfile Profile { get; set; }
        public string bulle { get; set; } = "Default";
    }
}
