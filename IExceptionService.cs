using RetailConsole.Models;

namespace RetailConsole.Services;

public interface IExceptionService
{
    Task<List<RetailException>> GetExceptionsAsync();
    Task<RetailException?> GetExceptionByIdAsync(string id);
    Task<bool> ResolveExceptionAsync(string id, ResolutionRequest request);
    Task<List<RetailException>> SearchAsync(string query, string? statusFilter, string? priorityFilter,
        string? typeFilter, string? sortField, bool sortDescending);
}
