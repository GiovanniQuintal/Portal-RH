using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace RRHH_ARG.Services
{
    public class MoodleBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        // Ejecutar cada 1 hora
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public MoodleBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Esperar un poco al iniciar la app para no bloquear el arranque
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            using PeriodicTimer timer = new PeriodicTimer(_period);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var moodleService = scope.ServiceProvider.GetRequiredService<MoodleSyncService>();

                        // 1. Sincronizar Catálogo
                        await moodleService.SincronizarCursosAsync();

                        // 2. Sincronizar Notas/Inscripciones (El método nuevo que te di arriba)
                        await moodleService.SincronizarCalificacionesMasivasAsync();

                        await moodleService.SincronizarInscripcionesMasivasAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Log error
                    Console.WriteLine($"Error en Background Job Moodle: {ex.Message}");
                }
            }
        }
    }
}