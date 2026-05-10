using Microsoft.AspNetCore.Identity;
using RRHH_ARG.Models;

namespace RRHH_ARG.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? UsuCodigo { get; set; }

        public bool EsPrimerIngreso { get; set; } = true;

        
    }

}

