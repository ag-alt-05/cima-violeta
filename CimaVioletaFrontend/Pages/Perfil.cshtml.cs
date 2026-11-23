using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CimaVioletaFrontend.Pages
{
    public class PerfilModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string NombreUsuarioUrl { get; set; }

        public void OnGet(string nombreUsuario)
        {
            NombreUsuarioUrl = nombreUsuario;
        }
    }
}