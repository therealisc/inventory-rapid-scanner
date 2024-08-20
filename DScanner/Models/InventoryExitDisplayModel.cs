namespace DScanner.Models;

public class InventoryExitDisplayModel
{
    public string Gestiune { get; set; } = null!;
    public string Denumire { get; set; } = null!;
    public int CodProdus { get; set; }
    //public string TipArticol { get; set; } = null!;
    //public string UM { get; set; } = null!;
    public decimal Cantitate { get; set; }
    public decimal PretUnitar { get; set; }
    public string Total { get; set; }
    public string TextSuplimentar { get; set; } = null!;
    public decimal Adaos { get; set; }
    public decimal Cont { get; set; }
}
