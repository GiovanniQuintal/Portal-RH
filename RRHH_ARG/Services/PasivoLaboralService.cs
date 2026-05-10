using Microsoft.EntityFrameworkCore;
using RRHH_ARG.Models;
using RRHH_ARG.Data;

namespace RRHH_ARG.Services
{
    public class PasivoLaboralService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly VacacionService _vacacionService;

        public PasivoLaboralService(IDbContextFactory<ApplicationDbContext> dbFactory, VacacionService vacacionService)
        {
            _dbFactory = dbFactory;
            _vacacionService = vacacionService;
        }

        public async Task<CalculoPasivoDTO> CalcularFiniquitoAsync(int empleadoId, DateTime fechaBaja, bool esLiquidacion, int diasSueldoPendienteManual = 0, bool incluir20Dias = false)
        {
            using var context = _dbFactory.CreateDbContext();

            var empleado = await context.Empleados.AsNoTracking().FirstOrDefaultAsync(e => e.EmpId == empleadoId);
            if (empleado == null) throw new Exception("Empleado no encontrado");

            DateTime fechaIngreso = empleado.EmpFechaContratacion ?? empleado.EmpFechaRegistro ?? DateTime.Now;

            // --- 2. EL NUEVO MOTOR DE SALARIOS ---
            decimal salarioDiario = empleado.SAE_SDI ?? ((empleado.SAE_MENSUAL ?? 0) / 30m);

            int aniosCumplidos = (fechaBaja.Year - fechaIngreso.Year);
            if (fechaIngreso.Date > fechaBaja.AddYears(-aniosCumplidos)) aniosCumplidos--;

            decimal factorIntegracion = CalcularFactorIntegracion(aniosCumplidos + 1);
            decimal sdiReal = Math.Round(salarioDiario * factorIntegracion, 2);

            var parametros = await context.ParametrosAnuales
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.AnioVigencia == fechaBaja.Year && p.IsActivo);

            var calculo = new CalculoPasivoDTO
            {
                EmpleadoId = empleadoId,
                FechaIngreso = fechaIngreso,
                FechaBaja = fechaBaja,
                SalarioDiario = Math.Round(salarioDiario, 2),
                SalarioDiarioIntegrado = sdiReal,
                DiasSueldoPendiente = diasSueldoPendienteManual, // <--- OBEDECE A LA PANTALLA
                MontoSueldoPendiente = Math.Round(diasSueldoPendienteManual * salarioDiario, 2)
            };

            // =======================================================
            // CÁLCULO 1: AGUINALDO PROPORCIONAL
            // =======================================================
            DateTime inicioAnio = new DateTime(fechaBaja.Year, 1, 1);
            DateTime fechaInicioAguinaldo = fechaIngreso > inicioAnio ? fechaIngreso : inicioAnio;
            int diasAguinaldo = (fechaBaja - fechaInicioAguinaldo).Days + 1;

            calculo.DiasAguinaldoProporcional = Math.Round((decimal)diasAguinaldo / 365m * 15m, 4);
            calculo.MontoAguinaldo = Math.Round(calculo.DiasAguinaldoProporcional * salarioDiario, 2);

            // =======================================================
            // CÁLCULO 2: VACACIONES PENDIENTES (CORREGIDO)
            // =======================================================
            await _vacacionService.GenerarKardexAutomaticoAsync(empleadoId);
            var kardex = await _vacacionService.GetKardexEmpleadoAsync(empleadoId);

            // 1. El saldo de las cubetas YA TIENE descontado el Ajuste Inicial gracias al motor PEPS
            decimal saldoTotalKardex = kardex.Filas.Sum(f => f.Derecho - f.Tomados);

            // 2. Sumamos la proporción del año en curso
            decimal propCurso = CalcularProporcionVacacionesEnCurso(fechaIngreso, fechaBaja, kardex.Filas.Count);

            // 3. Saldo final real sin dobles restas
            decimal saldoFinalDias = saldoTotalKardex + propCurso;

            calculo.DiasVacacionesPendientes = Math.Max(0, Math.Round(saldoFinalDias, 4));
            calculo.MontoVacaciones = Math.Round(calculo.DiasVacacionesPendientes * salarioDiario, 2);
            calculo.MontoPrimaVacacional = Math.Round(calculo.MontoVacaciones * 0.25m, 2);

            // =======================================================
            // CÁLCULO 3 Y 4: INDEMNIZACIONES 
            // =======================================================
            decimal antiguedadTotalDias = (fechaBaja - fechaIngreso).Days + 1;
            decimal antiguedadAniosDecimal = antiguedadTotalDias / 365.25m;
            calculo.AniosCompletosTrabajados = Math.Truncate(antiguedadAniosDecimal);

            if (esLiquidacion)
            {
                calculo.MontoIndemnizacion90Dias = Math.Round(90m * sdiReal, 2);

                if (incluir20Dias)
                {
                    decimal dias20PorAnio = Math.Round(antiguedadAniosDecimal * 20m, 4);
                    calculo.MontoIndemnizacion20Dias = Math.Round(dias20PorAnio * sdiReal, 2);
                }
            }

            if (esLiquidacion || antiguedadAniosDecimal >= 15)
            {
                decimal smg = parametros?.SalarioMinimo ?? 248.93m;
                decimal topePrima = smg * 2m;

                decimal basePrima = salarioDiario > topePrima ? topePrima : salarioDiario;
                decimal diasAntiguedad = Math.Round(antiguedadAniosDecimal * 12m, 4);

                calculo.MontoPrimaAntiguedad = Math.Round(diasAntiguedad * basePrima, 2);
            }

            return calculo;
        }

        // ==========================================================
        // HELPERS MATEMÁTICOS PRIVADOS
        // ==========================================================

        private decimal CalcularFactorIntegracion(int anioEnCurso)
        {
            decimal diasAguinaldo = 15m;
            decimal diasVacaciones = CalcularDiasPorLey(anioEnCurso);
            decimal primaVacacionalDias = diasVacaciones * 0.25m;

            // Fórmula LFT: (365 + Aguinaldo + Prima) / 365
            return Math.Round((365m + diasAguinaldo + primaVacacionalDias) / 365m, 4);
        }

        private decimal CalcularProporcionVacacionesEnCurso(DateTime fechaIngreso, DateTime fechaBaja, int aniosCumplidos)
        {
            DateTime ultimoAniversario = fechaIngreso.AddYears(aniosCumplidos);
            int diasTranscurridos = (fechaBaja - ultimoAniversario).Days;

            if (diasTranscurridos <= 0) return 0;

            int anioEvaluado = aniosCumplidos + 1;
            decimal derechoProximoAno = CalcularDiasPorLey(anioEvaluado);

            return Math.Round((derechoProximoAno / 365m) * diasTranscurridos, 4);
        }

        private decimal CalcularDiasPorLey(int anioCumplido)
        {
            if (anioCumplido <= 1) return 12;
            if (anioCumplido == 2) return 14;
            if (anioCumplido == 3) return 16;
            if (anioCumplido == 4) return 18;
            if (anioCumplido == 5) return 20;
            if (anioCumplido >= 6 && anioCumplido <= 10) return 22;
            if (anioCumplido >= 11 && anioCumplido <= 15) return 24;
            if (anioCumplido >= 16 && anioCumplido <= 20) return 26;
            if (anioCumplido >= 21 && anioCumplido <= 25) return 28;
            return 30;
        }
        // ==========================================================
        // PROYECCIÓN MASIVA (SÁBANA DE PASIVOS)
        // ==========================================================
        // Añadimos 'int diasSueldoPendiente' a los parámetros
        public async Task<List<CalculoPasivoDTO>> GenerarProyeccionMasivaAsync(DateTime fechaCorte, bool esLiquidacion, bool incluir20Dias, int diasSueldoPendiente)
        {
            using var context = _dbFactory.CreateDbContext();

            var empleadosActivos = await context.Empleados
                .AsNoTracking()
                .Where(e => e.StaId == 1)
                .Select(e => e.EmpId)
                .ToListAsync();

            var proyecciones = new List<CalculoPasivoDTO>();

            foreach (var empId in empleadosActivos)
            {
                try
                {
                    // Pasamos el valor 'diasSueldoPendiente' en lugar del 0 fijo que teníamos
                    var calculo = await CalcularFiniquitoAsync(empId, fechaCorte, esLiquidacion, diasSueldoPendiente, incluir20Dias);
                    proyecciones.Add(calculo);
                }
                catch
                {
                    continue;
                }
            }

            return proyecciones;
        }
    }
}