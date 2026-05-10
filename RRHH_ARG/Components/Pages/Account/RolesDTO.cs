namespace RRHH_ARG.Components.Pages.Account;

public class RolesDTO
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El nombre es obligatorio")]
    public string Name { get; set; }

}
