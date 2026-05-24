using APBD_09.Data;
using APBD_09.DTOs;
using APBD_09.Exceptions;
using APBD_09.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace APBD_09.Services;

public class DBService : IDBService
{
    private readonly DbfirstContext _context;
    public DBService(DbfirstContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GetPatientsDTO>> GetPatients(string? search)
    {
        var res = await _context.Patients
            .Where(p => search.IsNullOrEmpty() ? true : p.FirstName.Contains(search) || p.LastName.Contains(search))
            .Select(p => new GetPatientsDTO()
            {
                pesel = p.Pesel,
                firstName = p.FirstName,
                lastName = p.LastName,
                age = p.Age,
                sex = p.Sex,
                admissions = p.Admissions.Select(a => new GetAdmissionsDTO()
                    {
                        id = a.Id,
                        admissionDate = a.AdmissionDate,
                        dischargeDate = a.DischargeDate,
                        ward = new GetWardDTO()
                        {
                            id = a.WardId,
                            name = a.Ward.Name,
                            description = a.Ward.Description
                        }
                    }).ToList(),
                bedAssignments = p.BedAssignments.Select(ba => new GetBedAssignmentsDTO()
                {
                    id = ba.Id,
                    from = ba.From,
                    to = ba.To,
                    bed = new GetBedDTO()
                    {
                        id = ba.BedId,
                        bedType = new GetBedTypeDTO()
                        {
                            id = ba.Bed.BedTypeId,
                            name = ba.Bed.BedType.Name,
                            description = ba.Bed.BedType.Description
                        },
                        room = new GetRoomDTO()
                        {
                            id = ba.Bed.RoomId,
                            hasTv = ba.Bed.Room.HasTv,
                            ward = new GetWardDTO()
                            {
                                id = ba.Bed.Room.WardId,
                                name = ba.Bed.Room.Ward.Name,
                                description = ba.Bed.Room.Ward.Description
                            }
                        }
                    }
                }).ToList()
            })
            .ToListAsync();
        
        if (res == null)
        {
            throw new NotFoundException();
        }
        
        return res;
    }
    
}