using CashCalculator.Infrastructure.Data;

namespace CashCalculator.Services.Interfaces;

/// <summary>
/// Handles loading and saving of calculation session data.
/// </summary>
public interface ICalculationDataService
{
    CalculationData Load();
    void Save(CalculationData data);
}