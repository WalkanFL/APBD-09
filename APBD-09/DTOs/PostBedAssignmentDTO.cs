namespace APBD_09.DTOs;

public class PostBedAssignmentDTO
{
    public DateTime From { get; set; }

    public DateTime? To { get; set; }

    public string BedTypeName { get; set; }
    public string WardName { get; set; }
}