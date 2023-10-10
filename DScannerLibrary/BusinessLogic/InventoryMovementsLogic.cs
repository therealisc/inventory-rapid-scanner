using DScannerLibrary.DataAccess;
using DScannerLibrary.Extensions;
using DScannerLibrary.Models;

namespace DScannerLibrary.BusinessLogic;

public class InventoryMovementsLogic
{
    private readonly DbfDataAccess _dataAccess;

    public InventoryMovementsLogic()
    {
        _dataAccess = new DbfDataAccess();
    }

    public List<InventoryMovementModel> GetInventoryMovements(string? articleCode)
    {
        var articleMovementsDataTable = _dataAccess.ReadDbf($"Select cod_art, gestiune, sum(cantitate) from miscari " +
            $"where cod_art='{articleCode}' group by cod_art, gestiune");
        var inventoryMovements = articleMovementsDataTable.ConvertDataTable<InventoryMovementModel>();

        return inventoryMovements;
    }

    /// <summary>
    /// Creates an exit record for the only available inventory, without checking the available quantity.
    /// </summary>
    public int ProcessSingleInventoryExit(decimal exitDocumentId,
        List<InventoryMovementModel> inventoryMovements,
        ArticleModel article,
        decimal quantity)
    {
        var inventoryMovement = inventoryMovements.Single();

        var numberOfExistsOnCurrentDocument = _dataAccess.ReadDbf($"Select count(*) id_u from ies_det where id_iesire={exitDocumentId}").Rows[0][0];
        var id = DateTime.Now.ToString("yyMMdd");
        id = id + numberOfExistsOnCurrentDocument;
        var idAsDecimal = Convert.ToDecimal(id);

        var inventoryName = _dataAccess.ReadDbf($"Select denumire from gestiuni where cod='{inventoryMovement?.gestiune}'").Rows[0][0];

        var inventoryExit = new InventoryExitModel
        {
            id_u = idAsDecimal,
            id_iesire = exitDocumentId,
            gestiune = inventoryMovement?.gestiune,
            den_gest = (string)inventoryName,
            cod = inventoryMovement?.cod_art,
            denumire = article?.denumire,
            cantitate = quantity,
            pret_unitar = article.pret_vanz,
            valoare = quantity * article.pret_vanz,
            total = (quantity * article.pret_vanz) + ((article.tva / 100) * article.pret_vanz),
            tva_art = article.tva,
            tva_ded = ((article.tva / 100) * article.pret_vanz),
            cont = "707",
            den_tip = "Marfuri",
            um = "BUC",
            text_supl = $"articol scanat la {DateTime.Now}"
        };

        return _dataAccess.InsertIntoIesiriDbf(inventoryExit);
    }
}
