namespace APBD_09.DTOs;

public class PostBedAssignmentDTO
{
    public DateTime From { get; set; }

    public DateTime? To { get; set; }

    public string bedTypeName { get; set; }
    public string wardName { get; set; }
}