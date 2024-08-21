namespace DScannerLibrary.Models;

public class ArticleModel
{
    public string? cod { get; set; }
    public string? denumire { get; set; }
    public decimal tva { get; set; }
    public decimal pret_vanz { get; set; }
    public decimal? cod_bare { get; set; }
    //public string? lot { get; set; }
    public string? data_intr { get; set; }
}
