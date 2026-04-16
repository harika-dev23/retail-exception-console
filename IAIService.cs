using RetailConsole.Models;
namespace RetailConsole.Services;
public interface IAIService
{
    Task<string> GetRecommendationAsync(RetailException exception);
    Task<string> GenerateDraftNoteAsync(RetailException exception, ResolutionAction action);
}
