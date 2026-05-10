using RRHH_ARG.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RRHH_ARG.Components.Pages.Account;

public class RolesModel : PageModel
{
    // Solo necesitamos el gestor de Roles
    private readonly RoleManager<IdentityRole> _roleManager;

    [BindProperty]
    public RolesDTO Rol { get; set; }

    // Constructor: Eliminamos UserManager, ya no lo necesitamos aquí
    public RolesModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<ActionResult> OnGet()
    {
        // Obtenemos la lista solo para visualizar
        var MyRoles = await _roleManager.Roles.ToListAsync();
        ViewData["roles"] = MyRoles;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validación simple para evitar roles vacíos
        if (string.IsNullOrEmpty(Rol.Name))
        {
            return Page();
        }

        var newRol = new IdentityRole();
        newRol.Name = Rol.Name;

        // Acción única: Crear el rol en la base de datos
        await _roleManager.CreateAsync(newRol);

        // Recargamos la página para ver el nuevo rol en la tabla
        return RedirectPermanent("/account/roles");
    }
}