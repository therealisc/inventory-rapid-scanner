using System.Text.Json;
using DScannerLibrary.DataAccess;
using DScannerLibrary.Models;

public class Program
{
    public static int Main(string[] args)
    {
        var _dataAccess = new DbfDataAccess();
        var inventoryMovements = _dataAccess.ReadDbf<InventoryMovementModel>($"Select cod_art, gestiune, SUM(cantitate) as cantitate from miscari " +
            $"where cod_art='{args[0]}' group by cod_art, gestiune order by gestiune");

        var json = JsonSerializer.Serialize(inventoryMovements);
        Console.WriteLine(json);
        return 0;
    }
}
