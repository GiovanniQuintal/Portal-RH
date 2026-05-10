using Microsoft.AspNetCore.Mvc;
using RRHH_ARG.Data;
using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Models;

namespace RRHH_ARG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArchivoController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArchivo(int id)//MODIFICADO ENE 9
        {
            var expediente = await _context.Expediente.FindAsync(id);

            if (expediente == null || expediente.ExpArchivo == null)
            {
                return NotFound();
            }

            // Usamos el nombre guardado, o un genérico si es nulo
            string nombreArchivo = expediente.ExpRuta ?? "documento.dat";
            string ext = Path.GetExtension(nombreArchivo).ToLower();

            // 1. Determinar MIME Type
            string mimeType = GetMimeType(ext);

            // 2. CONFIGURACIÓN PARA VISUALIZAR (Solución aplicada aquí)
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = nombreArchivo, // Pasas el nombre tal cual, con acentos o ñ
                Inline = true             // Intenta mostrarlo en el navegador
            };
            Response.Headers.Append("Content-Disposition", cd.ToString()); // .ToString() hace la magia

            // 3. Retornar archivo
            return File(expediente.ExpArchivo, mimeType);
        }
        private string GetMimeType(string extension)
        {
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".avif" => "image/avif",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".tif" => "image/tiff",
                ".heic" => "image/heic", // Apple High Efficiency
                ".heif" => "image/heif",
                ".raw" => "image/x-dcraw", // Genérico para RAW, varía por cámara
                _ => "application/octet-stream" // Default para desconocidos
            };
        }

        // MODIFICADO ENE 9

        [HttpGet("Aplicante/{id}")]
        public async Task<IActionResult> GetCvAplicante(int id)
        {
            var aplicante = await _context.Aplicante.FindAsync(id);

            if (aplicante == null || aplicante.AplCvArchivo == null)
            {
                return NotFound();
            }

            string mimeType = "application/octet-stream";
            string nombreArchivo = aplicante.AplCv ?? "cv_aplicante.pdf";
            string ext = Path.GetExtension(nombreArchivo).ToLower();

            // Mapeo de tipos...
            if (ext == ".pdf") mimeType = "application/pdf";
            else if (ext == ".jpg" || ext == ".jpeg") mimeType = "image/jpeg";
            else if (ext == ".png") mimeType = "image/png";
            // Word (.doc/.docx) NO se puede visualizar en el navegador nativamente,
            // así que esos seguirán descargándose aunque pongas "inline".
            else if (ext == ".doc" || ext == ".docx")
            {
                mimeType = "application/msword"; // O el de docx
                                                 // Para Word forzamos descarga mejor
                return File(aplicante.AplCvArchivo, mimeType, nombreArchivo);
            }

            // Para PDF e Imágenes: Visualización Inline 
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = nombreArchivo,
                Inline = true
            };
            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(aplicante.AplCvArchivo, mimeType);
        }


        [HttpGet("Incidencia/{id}")]
        public async Task<IActionResult> GetIncidenciaArchivo(int id) //MODIFICADO DIC 18
        {
            var incidencia = await _context.Incidencium
                .Include(i => i.Din)
                .FirstOrDefaultAsync(i => i.IncId == id);

            if (incidencia == null || incidencia.Din == null || incidencia.Din.DinContenido == null)
            {
                return NotFound("El archivo no existe.");
            }

            string nombreOriginal = incidencia.Din.DinArchivo ?? $"incidencia_{id}.pdf";
            string mimeType = "application/pdf";

            // 1. Usamos la clase nativa con el nombre ORIGINAL, sin limpiarlo
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = nombreOriginal,
                Inline = true // Mantiene tu visualización en el navegador
            };

            // 2. Lo agregamos al header (el .ToString() hace la magia de codificarlo seguro)
            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(incidencia.Din.DinContenido, mimeType);
        }


        [HttpGet("EntregaEpp/{id}")]
        public async Task<IActionResult> GetEvidenciaEntrega(int id)//MODIFICADO DIC 18
        {
            // 1. Buscamos la entrega
            var entrega = await _context.EntregaEpp.FindAsync(id);

            if (entrega == null || entrega.EntEvidencia == null)
            {
                return NotFound("No se encontró evidencia para esta entrega.");
            }

            // 2. Variables para construir el nombre
            string nombrePersona = "Empleado_Desconocido";

            // 3. Buscamos al Empleado usando EmpId (que está en EntregaEpp)
            // NOTA: Tu DbSet se llama "Empleados" (plural)
            var empleado = await _context.Empleados.FindAsync(entrega.EmpId);

            if (empleado != null)
            {
                // 4. Buscamos a la Persona usando PerId (que está en Empleado)
                // NOTA: El nombre real está en la tabla "Personas"
                var persona = await _context.Personas.FindAsync(empleado.PerId);

                if (persona != null)
                {
                    // Concatenamos Nombre y Apellido (ej: Juan_Perez)
                    nombrePersona = $"{persona.PerNombre}_{persona.PerApellidoPaterno}";
                }
            }

            // 5. Formatear la fecha
            string fechaStr = entrega.EntFecha.HasValue
                              ? entrega.EntFecha.Value.ToString("dd-MM-yyyy")
                              : "SinFecha";

            // 6. Limpieza de caracteres inválidos en el nombre
            // Primero quitamos caracteres inválidos de ruta
            string nombreBase = string.Join("_", nombrePersona.Split(Path.GetInvalidFileNameChars()));
            nombreBase = nombreBase.Replace(" ", "_"); // Opcional: espacios por guiones

            // 6. Generar el nombre final (Ej: 12-12-2025_José_Peña_EPP.pdf)
            string nombreArchivo = $"{fechaStr}_{nombreBase}_EPP.pdf";

            // --- LA SOLUCIÓN NATIVA ---
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = nombreArchivo,
                Inline = true
            };
            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(entrega.EntEvidencia, "application/pdf");
        }

        [HttpGet("Puesto/{id}")]
        public async Task<IActionResult> GetPuestoArchivo(int id)
        {
            // Asumo que tu DbSet se llama PerfilPuestos o similar
            // Ajusta _context.PerfilPuestos si el nombre es diferente en tu DbContext
            var puesto = await _context.PerfilPuesto.FindAsync(id);

            if (puesto == null || puesto.PepArchivo == null)
            {
                return NotFound();
            }

            // Determinar tipo MIME simple (puedes mejorarlo si guardas la extensión)
            // Si la mayoría son PDF, usa application/pdf. 
            // Si guardas la extensión en otro campo, úsalo aquí.
            string mimeType = "application/pdf";
            string nombreArchivo = $"Perfil_Puesto_{id}.pdf";

            // --- LA SOLUCIÓN NATIVA ---
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = nombreArchivo,
                Inline = true
            };
            Response.Headers.Append("Content-Disposition", cd.ToString());

            return File(puesto.PepArchivo, mimeType);
        }

        [HttpGet("{empId}/Foto")]
        public async Task<IActionResult> GetFotoPerfil(int empId)
        {
            // 1. Buscamos el Expediente que sea FOTO (DexId 13) y de ese Empleado
            var fotoExpediente = await _context.Expediente
                .Where(x => x.EmpId == empId && x.DexId == 13 && x.ExpArchivo != null)
                .OrderByDescending(x => x.ExpFechasubido) // Por si acaso hay duplicados, traer el último
                .FirstOrDefaultAsync();

            if (fotoExpediente == null)
            {
                // Retorna una imagen vacía o 404
                return NotFound();
            }

            // 2. Determinar tipo MIME (Asumiendo jpg/png)
            string nombreOriginal = fotoExpediente.ExpRuta ?? $"foto_perfil_{empId}.jpg";
            string ext = Path.GetExtension(fotoExpediente.ExpRuta ?? ".jpg").ToLower();
            string mimeType = ext == ".png" ? "image/png" : "image/jpeg";

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = nombreOriginal, // Respetamos si se llama "fotografía_diseño.jpg"
                Inline = true
            };
            Response.Headers.Append("Content-Disposition", cd.ToString());

            // 3. Retornar la imagen
            return File(fotoExpediente.ExpArchivo!, mimeType);
        }
    }


}

