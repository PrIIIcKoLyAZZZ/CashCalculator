using CashCalculator.Infrastructure;
using CashCalculator.Infrastructure.Data;
using CashCalculator.Services.Interfaces;

namespace CashCalculator.Services.Data;

/// <summary>
/// JSON-backed implementation of <see cref="ICalculationDataService"/>.
/// Persists to "calculationdata.json" under %APPDATA%\KremenchugskayaTeam\CashCalculator\.
/// </summary>
public class JsonCalculationDataService : ICalculationDataService
{
    private readonly JsonRepository<CalculationData> _repo
        = new JsonRepository<CalculationData>("calculationdata.json");

    public CalculationData Load() => _repo.Load();
    public void Save(CalculationData data) => _repo.Save(data);
}