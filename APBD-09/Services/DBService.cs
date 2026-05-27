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
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex,
                Admissions = p.Admissions.Select(a => new GetAdmissionsDTO()
                    {
                        Id = a.Id,
                        AdmissionDate = a.AdmissionDate,
                        DischargeDate = a.DischargeDate,
                        Ward = new GetWardDTO()
                        {
                            Id = a.WardId,
                            Name = a.Ward.Name,
                            Description = a.Ward.Description
                        }
                    }).ToList(),
                BedAssignments = p.BedAssignments.Select(ba => new GetBedAssignmentsDTO()
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new GetBedDTO()
                    {
                        Id = ba.BedId,
                        BedType = new GetBedTypeDTO()
                        {
                            Id = ba.Bed.BedTypeId,
                            Name = ba.Bed.BedType.Name,
                            Description = ba.Bed.BedType.Description
                        },
                        Room = new GetRoomDTO()
                        {
                            Id = ba.Bed.RoomId,
                            HasTv = ba.Bed.Room.HasTv,
                            Ward = new GetWardDTO()
                            {
                                Id = ba.Bed.Room.WardId,
                                Name = ba.Bed.Room.Ward.Name,
                                Description = ba.Bed.Room.Ward.Description
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

        var doesBedTypeExist = await _context.BedTypes.Where(bt => bt.Name.Equals(createBedAssignment.BedTypeName)).AnyAsync();
        if (!doesBedTypeExist)
        {
            throw new NotFoundException("Bed Type of that name doesnt exist");
        }
        var doesWardExist = await _context.Wards.Where(w => w.Name.Equals(createBedAssignment.WardName)).AnyAsync();
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
            .Where(b => b.Room.Ward.Name.Equals(createBedAssignment.WardName))
            .ToList();
        if (beds.IsNullOrEmpty())
        {
            throw new NotFoundException("No Bed in that Ward in that timeframe found");
        }
        
        var chosenBed = beds
            .Where(b => b.BedType.Name.Equals(createBedAssignment.BedTypeName))//bed Type
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