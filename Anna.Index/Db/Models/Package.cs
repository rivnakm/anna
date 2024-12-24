namespace Anna.Index.Db.Models;

public class Package
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string LowerName { get; set; } = null!;
    public ICollection<Version> Versions { get; set; } = null!;
}
