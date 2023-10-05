namespace DScannerLibrary.Models;

public class InventoryMovementModel
{
    public decimal id_u { get; set; }
    public decimal id_iesire { get; set; }
    public DateTime data { get; set; }
    public string? cod_art { get; set; }
    public string? denumire { get; set; }
    public string? gestiune { get; set; }
    public string? den_gest { get; set; }
    public decimal cantitate { get; set; }
}
