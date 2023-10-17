namespace DScannerLibrary.Models;

public class InventoryMovementModel
{
    public string? cod_art { get; set; }
    public string gestiune { get; set; } = null!;
    public decimal cantitate { get; set; }
}
