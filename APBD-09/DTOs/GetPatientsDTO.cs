using APBD_09.Models;

namespace APBD_09.DTOs;

public class GetPatientsDTO
{
    public string pesel { get; set; } = null!;

    public string firstName { get; set; } = null!;

    public string lastName { get; set; } = null!;

    public int age { get; set; }

    public bool sex { get; set; }

    public ICollection<GetAdmissionsDTO> admissions { get; set; } = [];

    public ICollection<GetBedAssignmentsDTO> bedAssignments { get; set; } = [];
}

public class GetAdmissionsDTO
{
    public int id { get; set; }

    public DateTime admissionDate { get; set; }

    public DateTime? dischargeDate { get; set; }

    public GetWardDTO ward { get; set; }
}

public class GetWardDTO
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public string description { get; set; } = null!;
}

public class GetBedAssignmentsDTO
{
    public int id { get; set; }

    public DateTime from { get; set; }

    public DateTime? to { get; set; }

    public GetBedDTO bed { get; set; } = null!;
}

public class GetBedDTO
{
    public int id { get; set; }

    public GetBedTypeDTO bedType { get; set; } = null!;

    public GetRoomDTO room { get; set; } = null!;
}

public class GetBedTypeDTO()
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public string description { get; set; } = null!;
}

public class GetRoomDTO
{
    public string id { get; set; } = null!;
    
    public bool hasTv { get; set; }

    public GetWardDTO ward { get; set; } = null!;
}