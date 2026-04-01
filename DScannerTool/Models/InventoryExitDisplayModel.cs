namespace DScannerTool.Models;

public class InventoryExitDisplayModel
{
    public int Nr { get; set; }
    public string Gestiune { get; set; } = string.Empty;
    public string Denumire { get; set; } = string.Empty;
    public int CodProdus { get; set; }
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    public decimal Total { get; set; }
    public string TextSuplimentar { get; set; } = string.Empty;
    public decimal Adaos { get; set; }
    public decimal Cont { get; set; }
}