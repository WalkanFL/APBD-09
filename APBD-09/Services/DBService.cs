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

    public async Task<PostBedAssignmentDTO> PostBed(PostBedAssignmentDTO createBedAssignment, string pesel)
    {
        var anyPatientOfPesel = await _context.Patients.Where(p => p.Pesel.Equals(pesel)).AnyAsync();
        if (!anyPatientOfPesel)
        {
            throw new NotFoundException("No Patient of given Pesel");
        }

        var doesBedTypeExist = await _context.BedTypes.Where(bt => bt.Name.Equals(createBedAssignment.bedTypeName)).AnyAsync();
        if (!doesBedTypeExist)
        {
            throw new NotFoundException("Bed Type of that name doesnt exist");
        }
        var doesWardExist = await _context.Wards.Where(w => w.Name.Equals(createBedAssignment.wardName)).AnyAsync();
        if (!doesWardExist)
        {
            throw new NotFoundException("Ward of that name doesnt exist");
        }

        var beds = await _context.Beds
            .Where(b => !b.BedAssignments
                .Any(ba => createBedAssignment.From < ba.To && createBedAssignment.To > ba.From) //prawo De'Morgana zaprzeczenie warunku na brak konfliktu
            ) //brak konfliktów w czasie //brak konfliktów w czasie
            .ToListAsync();
        if (beds.IsNullOrEmpty())
        {
            throw new NotFoundException("No Bed in that timeframe found");
        }
        
        beds = beds//wybór oddziału pewnie jest ważniejszy więc powinien być sprawdzany wcześniej
            .Where(b => b.Room.Ward.Name.Equals(createBedAssignment.wardName))
            .ToList();
        if (beds.IsNullOrEmpty())
        {
            throw new NotFoundException("No Bed in that Ward in that timeframe found");
        }
        
        var chosenBed = beds
            .Where(b => b.BedType.Name.Equals(createBedAssignment.bedTypeName))//bed Type
            .FirstOrDefault();
        
        if (chosenBed == null)
        {
            throw new NotFoundException("No Bed of that type in that ward in that timeframe found");
        }
        
        var transaction =  await _context.Database.BeginTransactionAsync();
        try
        {
            var newBedAssignment = new BedAssignment()
            {
                PatientPesel = pesel,
                BedId = chosenBed.Id,
                From = createBedAssignment.From,
                To = createBedAssignment.To,
            };
            
             await _context.BedAssignments.AddAsync(newBedAssignment);
             await _context.SaveChangesAsync();
             
            await transaction.CommitAsync();
            return createBedAssignment;
        }catch(Exception e){
            await transaction.RollbackAsync();
            throw;
        }
    }
}