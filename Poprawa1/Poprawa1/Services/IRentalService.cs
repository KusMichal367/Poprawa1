using Poprawa1.Models;
namespace Poprawa1.Services;


public interface IRentalService
{
    Task<ClientDTO> GetClientAsync(int id);
    Task AddClientAndRentalAsync(InputDTO inputDto);
}