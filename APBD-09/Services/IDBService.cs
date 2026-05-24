using APBD_09.DTOs;

namespace APBD_09.Services;

public interface IDBService
{
    public async Task<IEnumerable<GetPatientsDTO>> GetPatients(string? search)
    {
        return null;
    }
}