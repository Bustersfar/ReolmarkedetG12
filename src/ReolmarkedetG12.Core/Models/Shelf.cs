namespace ReolmarkedetG12.Core.Models;

public class Shelf
{
    public int ShelfId { get; set; }
    public int Number { get; set; }
    public ShelfType Type { get; set; }
    public ShelfStatus Status { get; set; }
}
