using System.Security.AccessControl;

namespace ReolmarkedetG12.Core.Models;

public class Rack
{
    public int RackId { get; set; }
    public int Number { get; set; }
    public RackStatus Status { get; set; }
}