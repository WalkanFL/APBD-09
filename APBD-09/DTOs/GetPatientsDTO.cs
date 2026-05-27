using APBD_09.Models;

namespace APBD_09.DTOs;

public class GetPatientsDTO
{
    public string Pesel { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int Age { get; set; }

    public bool Sex { get; set; }

    public ICollection<GetAdmissionsDTO> Admissions { get; set; } = [];

    public ICollection<GetBedAssignmentsDTO> BedAssignments { get; set; } = [];
}

public class GetAdmissionsDTO
{
    public int Id { get; set; }

    public DateTime AdmissionDate { get; set; }

    public DateTime? DischargeDate { get; set; }

    public GetWardDTO Ward { get; set; }
}

public class GetWardDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}

public class GetBedAssignmentsDTO
{
    public int Id { get; set; }

    public DateTime From { get; set; }

    public DateTime? To { get; set; }

    public GetBedDTO Bed { get; set; } = null!;
}

public class GetBedDTO
{
    public int Id { get; set; }

    public GetBedTypeDTO BedType { get; set; } = null!;

    public GetRoomDTO Room { get; set; } = null!;
}

public class GetBedTypeDTO()
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}

public class GetRoomDTO
{
    public string Id { get; set; } = null!;
    
    public bool HasTv { get; set; }

    public GetWardDTO Ward { get; set; } = null!;
}