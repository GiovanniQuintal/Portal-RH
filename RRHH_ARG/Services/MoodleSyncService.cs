using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Data;
using RRHH_ARG.Models;
using RRHH_ARG.Services.MoodleResponses;

namespace RRHH_ARG.Services
{
    public class MoodleSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory; // Necesario para usar DbContext en background o servicios inyectados

        public MoodleSyncService(HttpClient httpClient, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        public async Task<string> SincronizarCursosAsync()
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];

            // Función de Moodle para obtener cursos
            var function = "core_course_get_courses";

            // Construimos la URL
            // moodlewsrestformat=json es VITAL para que no devuelva XML
            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json";

            try
            {
                // 1. LLAMADA A LA API
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                // 2. DESERIALIZAR (Convertir JSON a C#)
                // Moodle a veces devuelve una lista directa [] o un objeto si hay error. Asumimos lista para este endpoint.
                var cursosMoodle = JsonSerializer.Deserialize<List<MoodleUserCourseResponse>>(jsonString);

                if (cursosMoodle == null || !cursosMoodle.Any())
                    return "No se encontraron cursos en Moodle o hubo un error de formato.";

                // 3. GUARDAR EN SQL SERVER
                // Creamos un scope nuevo para obtener el DbContext
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    int nuevos = 0;
                    int actualizados = 0;

                    foreach (var cursoApi in cursosMoodle)
                    {
                        // Filtramos el curso ID 1 que suele ser el "Site Home" de Moodle y no un curso real
                        if (cursoApi.Id == 1) continue;

                        // Buscamos si ya existe en nuestra BD local por su ID de Moodle
                        var cursoDb = await dbContext.MoodleCursos
                            .FirstOrDefaultAsync(c => c.MoodleCursoId == cursoApi.Id);

                        if (cursoDb == null)
                        {
                            // --- CREAR NUEVO ---
                            cursoDb = new MoodleCurso
                            {
                                MoodleCursoId = cursoApi.Id,
                                McNombre = cursoApi.Fullname,
                                McCodigo = cursoApi.Shortname,
                                McFechaInicio = UnixTimeStampToDateTime(cursoApi.Startdate),
                                McFechaFin = (cursoApi.Enddate > 0) ? UnixTimeStampToDateTime(cursoApi.Enddate) : null,
                                McEstado = (byte)cursoApi.Visible, // 1 visible, 0 oculto
                                McEsPublico = true,
                                TicId = 1, // Por defecto asignamos Tipo 1 (Ajusta esto según tu lógica)
                                McFechaSync = DateTime.Now
                            };
                            dbContext.MoodleCursos.Add(cursoDb);
                            nuevos++;
                        }
                        else
                        {
                            // --- ACTUALIZAR EXISTENTE ---
                            cursoDb.McNombre = cursoApi.Fullname;
                            cursoDb.McCodigo = cursoApi.Shortname;
                            cursoDb.McEstado = (byte)cursoApi.Visible;
                            cursoDb.McFechaInicio = UnixTimeStampToDateTime(cursoApi.Startdate);
                            if (cursoApi.Enddate > 0) cursoDb.McFechaFin = UnixTimeStampToDateTime(cursoApi.Enddate);

                            cursoDb.McFechaSync = DateTime.Now;
                            actualizados++;
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    return $"Sincronización Completada: {nuevos} cursos nuevos, {actualizados} actualizados.";
                }
            }
            catch (Exception ex)
            {
                return $"Error en sincronización: {ex.Message}";
            }
        }

        // Helpers
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Moodle timestamps are seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", String.Empty);
        }

        // Agrega esto dentro de la clase MoodleSyncService

        public async Task SincronizarCalificacionesMasivasAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Obtenemos todos los empleados que tienen email (usuario de sistema)
                // Necesitamos el email para buscar quién es quién en Moodle
                var empleados = await dbContext.Empleados
                    .Include(e => e.Per)
                    .Include(e => e.Usuario) // Para obtener el email real
                    .Where(e => e.AspNetUserId != null)
                    .ToListAsync();

                foreach (var emp in empleados)
                {
                    try
                    {
                        var email = emp.Usuario.Email; // O emp.Per.PerEmail dependiendo cual uses en Moodle
                        if (string.IsNullOrEmpty(email)) continue;

                        // 2. Buscar ID de Moodle usando el Email
                        int moodleUserId = await GetMoodleUserIdByEmail(email);

                        if (moodleUserId > 0)
                        {
                            // 3. Obtener cursos y calificaciones de este usuario
                            await SincronizarNotasPorUsuario(dbContext, emp.EmpId, moodleUserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sync empleado {emp.EmpId}: {ex.Message}");
                        // Continuamos con el siguiente
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task<int> GetMoodleUserIdByEmail(string email)
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];
            var function = "core_user_get_users_by_field";

            // Moodle pide field=email y values[0]=correo
            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json&field=email&values[0]={email}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            // Usamos JsonDocument para no crear una clase solo para el ID
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    return root[0].GetProperty("id").GetInt32();
                }
            }
            return 0; // No encontrado
        }

        private async Task SincronizarNotasPorUsuario(ApplicationDbContext db, int empId, int moodleUserId)
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];
            var function = "gradereport_user_get_grade_items";

            // courseid=0 trae todos los cursos
            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json&userid={moodleUserId}&courseid=0";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            // Deserialización manual rápida o usa clases
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                if (root.TryGetProperty("usergrades", out var userGrades))
                {
                    foreach (var gradeInfo in userGrades.EnumerateArray())
                    {
                        int courseId = gradeInfo.GetProperty("courseid").GetInt32();

                        // Buscamos el curso en nuestra BD local para saber su ID interno (MC_ID)
                        var cursoLocal = await db.MoodleCursos.FirstOrDefaultAsync(c => c.MoodleCursoId == courseId);

                        if (cursoLocal != null)
                        {
                            // Buscamos la nota final del curso (itemtype = course)
                            var gradeItems = gradeInfo.GetProperty("gradeitems");
                            double porcentaje = 0;

                            foreach (var item in gradeItems.EnumerateArray())
                            {
                                if (item.GetProperty("itemtype").GetString() == "course")
                                {
                                    // A veces viene null
                                    if (item.TryGetProperty("graderaw", out var raw) && raw.ValueKind != JsonValueKind.Null)
                                    {
                                        // Asumiendo que la nota es sobre 100, si es sobre 10 ajusta aquí
                                        porcentaje = raw.GetDouble();
                                    }
                                    break;
                                }
                            }

                            // --- UPSERT (Insertar o Actualizar) en MOODLE_CURSO_EMPLEADO ---
                            var inscripcion = await db.MoodleCursoEmpleados
                                .FirstOrDefaultAsync(x => x.EmpId == empId && x.McId == cursoLocal.McId);

                            if (inscripcion == null)
                            {
                                inscripcion = new MoodleCursoEmpleado
                                {
                                    EmpId = empId,
                                    McId = cursoLocal.McId,
                                    MceFechaAsignacion = DateTime.Now, // Aproximado
                                    MceFechaInicio = DateTime.Now,
                                    MceEstado = 1, // Activo
                                    McePorcentajeAvance = (decimal)porcentaje,
                                    MceFechaSync = DateTime.Now
                                };
                                db.MoodleCursoEmpleados.Add(inscripcion);
                            }
                            else
                            {
                                inscripcion.McePorcentajeAvance = (decimal)porcentaje;
                                inscripcion.MceFechaSync = DateTime.Now;
                                if (porcentaje >= 100) inscripcion.MceEstado = 2; // Completado
                            }
                        }
                    }
                }
            }
        }

        // En MoodleSyncService.cs
        public async Task<bool> CrearUsuarioMoodleAsync(string username, string password, string nombre, string apellido, string email)
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];
            var function = "core_user_create_users";

            // NOTA: Es mejor usar POST para crear usuarios, pero sigamos con GET para no romper tu lógica actual.
            // Aseguramos que los espacios se codifiquen bien (Uri.EscapeDataString)
            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json" +
                      $"&users[0][username]={Uri.EscapeDataString(username)}" +
                      $"&users[0][password]={Uri.EscapeDataString(password)}" +
                      $"&users[0][firstname]={Uri.EscapeDataString(nombre)}" +
                      $"&users[0][lastname]={Uri.EscapeDataString(apellido)}" +
                      $"&users[0][email]={Uri.EscapeDataString(email)}" +
                      $"&users[0][auth]=manual";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                // --- ESTO ES LO QUE NECESITAS VER EN LA CONSOLA ---
                Console.WriteLine("========================================");
                Console.WriteLine($"MOODLE RESPONSE ({username}): {json}");
                Console.WriteLine("========================================");

                // Moodle devuelve una excepción o un debuginfo si falla
                if (json.Contains("\"exception\"") || json.Contains("\"debuginfo\""))
                {
                    throw new Exception($"Moodle rechazó la creación: {json}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR HTTP MOODLE: {ex.Message}");
                throw; // Re-lanzamos para que EmpleadoService se entere
            }
        }



        public async Task<int> CrearCohorteMoodleAsync(string nombreArea, string idNumberArea)
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];
            var function = "core_cohort_create_cohorts";

            // ESTRUCTURA CORREGIDA:
            // 1. Usamos categorytype[type]=system
            // 2. IMPORTANTE: Enviamos categorytype[value]= (vacío), pero debe estar presente.

            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json" +
                      $"&cohorts[0][name]={Uri.EscapeDataString(nombreArea)}" +
                      $"&cohorts[0][idnumber]={Uri.EscapeDataString(idNumberArea)}" +
                      $"&cohorts[0][categorytype][type]=system" +
                      $"&cohorts[0][categorytype][value]="; // <--- ESTO FALTABA

            try
            {
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[MOODLE API] Crear Cohorte Respuesta: {json}");

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;

                    // Caso de Éxito
                    if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                    {
                        // Devolvemos el ID de la nueva cohorte
                        return root[0].GetProperty("id").GetInt32();
                    }

                    // Caso de Error (ej: ID duplicado)
                    if (root.ValueKind == JsonValueKind.Object && json.Contains("exception"))
                    {
                        Console.WriteLine($"[MOODLE ERROR] {json}");
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR C#] {ex.Message}");
                return 0;
            }
        }

        // En MoodleSyncService.cs

        public async Task UpdatePasswordMoodleAsync(string email, string newPassword)
        {
            // 1. Primero necesitamos saber el ID de Moodle de ese usuario
            int moodleUserId = await GetMoodleUserIdByEmail(email); // (Ya tenías este método)

            if (moodleUserId == 0) return;

            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];
            var function = "core_user_update_users";

            // users[0][id] = ID del usuario a modificar
            // users[0][password] = La nueva contraseña
            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json" +
                      $"&users[0][id]={moodleUserId}" +
                      $"&users[0][password]={Uri.EscapeDataString(newPassword)}";

            await _httpClient.GetAsync(url);
            // No necesitamos leer la respuesta, si falla, falla silenciosamente o logueas.
        }

        // ---------------------------------------------------------
        // SINCRONIZAR VINCULACIÓN CURSO <-> ÁREA (COHORTE)
        // ---------------------------------------------------------
        public async Task SincronizarRelacionesCursoAreaAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cursosLocales = await dbContext.MoodleCursos.ToListAsync();
                var areasConCohorte = await dbContext.Area
                    .Where(a => a.MoodleCohortId != null)
                    .ToListAsync();

                // --- DEBUG LOGS ---
                Console.WriteLine($"[DEBUG] Cursos locales encontrados: {cursosLocales.Count}");
                Console.WriteLine($"[DEBUG] Áreas con Cohorte vinculado (NO NULL): {areasConCohorte.Count}");
                // ------------------

                if (!cursosLocales.Any() || !areasConCohorte.Any())
                {
                    Console.WriteLine("[DEBUG] Abortando sincronización de relaciones: Faltan cursos o áreas vinculadas.");
                    return;
                }

                var baseUrl = _configuration["MoodleConfig:BaseUrl"];
                var token = _configuration["MoodleConfig:Token"];
                var function = "core_enrol_get_course_enrolment_methods";

                foreach (var curso in cursosLocales)
                {
                    try
                    {
                        // Preguntamos a Moodle: "¿Qué métodos de matriculación tiene este curso?"
                        var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json&courseid={curso.MoodleCursoId}";

                        var response = await _httpClient.GetAsync(url);
                        var json = await response.Content.ReadAsStringAsync();

                        Console.WriteLine($"[DEBUG ENROL] Curso {curso.McId} JSON: {json}");

                        using (JsonDocument doc = JsonDocument.Parse(json))
                        {
                            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var method in doc.RootElement.EnumerateArray())
                                {
                                    // 2. LOGUEA CADA MÉTODO ENCONTRADO
                                    if (method.TryGetProperty("type", out var typeProp))
                                    {
                                        string tipo = typeProp.GetString();
                                        Console.WriteLine($"   -> Método encontrado: {tipo}");

                                        if (tipo == "cohort")
                                        {
                                            if (method.TryGetProperty("customint1", out var cohortIdElem))
                                            {
                                                int moodleCohortId = 0;
                                                if (cohortIdElem.ValueKind == JsonValueKind.Number)
                                                    moodleCohortId = cohortIdElem.GetInt32();
                                                else if (cohortIdElem.ValueKind == JsonValueKind.String)
                                                    int.TryParse(cohortIdElem.GetString(), out moodleCohortId);

                                                Console.WriteLine($"      -> ID Cohorte detectado: {moodleCohortId}");

                                                // 2. BUSCAMOS EL MATCH EN NUESTRA BD
                                                // ¿Qué área tiene este ID de cohorte?
                                                var areaMatch = areasConCohorte.FirstOrDefault(a => a.MoodleCohortId == moodleCohortId);

                                                if (areaMatch != null)
                                                {
                                                    Console.WriteLine($"      -> ¡MATCH! Corresponde al área: {areaMatch.AreNombre}");
                                                    // 3. VERIFICAMOS SI YA EXISTE LA RELACIÓN PARA NO DUPLICAR
                                                    var existeRelacion = await dbContext.MoodleCursoAreas
                                                    .AnyAsync(r => r.McId == curso.McId && r.AreId == areaMatch.AreId);

                                                    if (!existeRelacion)
                                                    {
                                                        // 4. CREAMOS LA RELACIÓN
                                                        dbContext.MoodleCursoAreas.Add(new MoodleCursoArea
                                                        {
                                                            McId = curso.McId,
                                                            AreId = areaMatch.AreId
                                                        });
                                                        Console.WriteLine($"[SYNC] Vinculado: {curso.McNombre} <-> {areaMatch.AreNombre}");
                                                    }
                                                }

                                                Console.WriteLine($"      -> No se encontró área local con MOODLE_COHORT_ID = {moodleCohortId}");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sync relaciones curso {curso.MoodleCursoId}: {ex.Message}");
                    }
                }

                // Guardamos todos los vínculos nuevos de una sola vez
                await dbContext.SaveChangesAsync();
            }
        }

        // Agrega esto dentro de la clase MoodleSyncService

        public async Task<bool> AsignarCohorteACursoMoodleAsync(int moodleCohortId, int moodleCourseId)
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];

            // IMPORTANTE: Asegúrate de haber agregado esta función en tu Token de Moodle
            var function = "enrol_cohort_enrol_cohort";

            // members[0][cohorttype][type] = id
            // members[0][cohorttype][value] = ID del Cohorte
            // members[0][coursetype][type] = id
            // members[0][coursetype][value] = ID del Curso

            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json" +
                      $"&members[0][cohorttype][type]=id" +
                      $"&members[0][cohorttype][value]={moodleCohortId}" +
                      $"&members[0][coursetype][type]=id" +
                      $"&members[0][coursetype][value]={moodleCourseId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[MOODLE API] Asignar Cohorte ({moodleCohortId}) a Curso ({moodleCourseId}): {json}");

                // Moodle devuelve null o [] si es exitoso. Si falla devuelve un objeto con "exception".
                if (json.Contains("\"exception\""))
                {
                    Console.WriteLine($"[MOODLE ERROR] {json}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR C#] Falló asignación de cohorte: {ex.Message}");
                return false;
            }
        }

        public async Task SincronizarInscripcionesMasivasAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // 1. Obtenemos empleados con email
                var empleados = await dbContext.Empleados
                    .Include(e => e.Usuario)
                    .Where(e => e.AspNetUserId != null && e.Usuario.Email != null)
                    .ToListAsync();

                foreach (var emp in empleados)
                {
                    try
                    {
                        // 2. Buscar ID de Moodle por Email
                        int moodleUserId = await GetMoodleUserIdByEmail(emp.Usuario.Email);

                        if (moodleUserId > 0)
                        {
                            // 3. Traer cursos inscritos de este usuario
                            await SincronizarCursosDelUsuario(dbContext, emp.EmpId, moodleUserId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sync empleado {emp.EmpId}: {ex.Message}");
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task SincronizarCursosDelUsuario(ApplicationDbContext db, int empId, int moodleUserId)
        {
            var baseUrl = _configuration["MoodleConfig:BaseUrl"];
            var token = _configuration["MoodleConfig:Token"];

            // FUNCIÓN CLAVE: Trae los cursos donde el usuario está inscrito (manual o autom)
            var function = "core_enrol_get_users_courses";

            var url = $"{baseUrl}/webservice/rest/server.php?wstoken={token}&wsfunction={function}&moodlewsrestformat=json&userid={moodleUserId}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            // Si el usuario no tiene cursos, Moodle devuelve array vacío [].
            // Si hay error, devuelve objeto con "exception".
            if (json.Contains("\"exception\"")) return;

            var cursosInscritos = JsonSerializer.Deserialize<List<MoodleUserCourseResponse>>(json);
            if (cursosInscritos == null) return;

            // IDs de cursos Moodle que tiene el usuario ACTUALMENTE
            var moodleCourseIds = cursosInscritos.Select(c => c.Id).ToList();

            // A. DETECTAR DES-INSCRIPCIONES (Estaba en BD local, pero ya no viene de Moodle)
            var inscripcionesObsoletas = await db.MoodleCursoEmpleados
                .Include(mce => mce.Curso)
                .Where(mce => mce.EmpId == empId && !moodleCourseIds.Contains(mce.Curso.MoodleCursoId))
                .ToListAsync();

            if (inscripcionesObsoletas.Any())
            {
                // Opción 1: Eliminarlos físicamente
                // db.MoodleCursoEmpleados.RemoveRange(inscripcionesObsoletas);

                // Opción 2: Cambiar estado a "Inactivo" / "Expirado" (Recomendado)
                foreach (var obs in inscripcionesObsoletas)
                {
                    obs.MceEstado = 0; // 0 = Desvinculado/Inactivo
                }
            }

            // B. ACTUALIZAR O INSERTAR (UPSERT)
            foreach (var cursoApi in cursosInscritos)
            {
                // 1. Verificar que tenemos el curso en el catálogo local MOODLE_CURSO
                var cursoLocal = await db.MoodleCursos.FirstOrDefaultAsync(c => c.MoodleCursoId == cursoApi.Id);

                // Si el curso existe en Moodle pero no lo hemos sincronizado en el catálogo local,
                // lo saltamos (o podrías llamar a crear curso aquí, pero mejor mantener orden).
                if (cursoLocal == null) continue;

                // 2. Buscar la inscripción local
                var inscripcion = await db.MoodleCursoEmpleados
                    .FirstOrDefaultAsync(x => x.EmpId == empId && x.McId == cursoLocal.McId);

                if (inscripcion == null)
                {
                    
                    inscripcion = new MoodleCursoEmpleado
                    {
                        EmpId = empId,
                        McId = cursoLocal.McId,
                        MceFechaAsignacion = DateTime.Now,
                        MceFechaInicio = UnixTimeStampToDateTime(cursoApi.Startdate),
                        MceEstado = 1, // Activo
                        McePorcentajeAvance = (decimal)(cursoApi.Progress ?? 0),
                        MceFechaSync = DateTime.Now
                    };

                    if (cursoApi.Completed == true) inscripcion.MceEstado = 2; // Completado

                    db.MoodleCursoEmpleados.Add(inscripcion);
                }
                else
                {
                    
                    inscripcion.McePorcentajeAvance = (decimal)(cursoApi.Progress ?? 0);
                    inscripcion.MceFechaSync = DateTime.Now;

                    // Lógica simple de estado
                    if (cursoApi.Completed == true || (cursoApi.Progress.HasValue && cursoApi.Progress >= 100))
                    {
                        inscripcion.MceEstado = 2; 
                    }
                    else
                    {
                        inscripcion.MceEstado = 1;
                    }
                }
            }

            // =====================================================================
            // RECALCULAR KPI DE CURSOS AL SINCRONIZAR CON MOODLE
            // =====================================================================
            var kpisCursos = await db.PdPriorities
                .Where(p => p.EmpId == empId && p.UnitOfMeasure == "Cursos" && !p.IsArchived)
                .ToListAsync();

            foreach (var kpi in kpisCursos)
            {
                var cursosDelAnio = await db.MoodleCursoEmpleados
                    .Where(c => c.EmpId == empId && c.MceFechaAsignacion.Year == kpi.Year)
                    .ToListAsync();

                kpi.TargetValue = cursosDelAnio.Count;
                kpi.ActualValue = cursosDelAnio.Count(c => c.MceEstado == 2 || c.McePorcentajeAvance >= 100);

                // Si ya completó todos los cursos asignados, lo marcamos como "Cumplió"
                if (kpi.TargetValue > 0 && kpi.ActualValue >= kpi.TargetValue && kpi.TrackingType == "MHB")
                {
                    kpi.FinalStatus = "Cumplió";
                }
                else
                {
                    // Por si se le agregan nuevos cursos y se le "quita" el estatus de completado
                    kpi.FinalStatus = null;
                }
            }

    }
    }
}