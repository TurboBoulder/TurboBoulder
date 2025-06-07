using Microsoft.AspNetCore.Identity;

namespace TurboBoulder.Data
{
    public class User : IdentityUser
    {
        //public UserProfile Profile { get; set; }
        public string bulle { get; set; } = "Default";
    }
}
