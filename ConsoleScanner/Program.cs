using DScannerLibrary.Extensions;
using DScannerLibrary.Models;
using DScannerLibrary.DataAccess;

if (IntPtr.Size == 8)
{
    Console.WriteLine("Sorry this is not going to work in 64 bits");
    return;
}
var da = new DbfDataAccess();

// Caut documentul de iesire din aceasta zi si daca nu exista dau mesaj ca nu exista
// Works fine
var exitDocumentId = da.ReadDbf("Select top 1 id_iesire from iesiri where data = DATE() order by data desc");
if (exitDocumentId.Rows.Count == 0)
{
    Console.WriteLine("Adauga o iesire cu data de azi mai intai!");
    return;
}

Console.WriteLine(exitDocumentId.Rows[0][0]);

while (true)
{
    Console.WriteLine("Enter the barcode:");
    var articleBarCode = Console.ReadLine();

    // articole pe gestiuni, article list va avea atatea randuri cate gestiuni sunt
    var articleAsDataTable = da.ReadDbf($"Select * from articole where cod_bare={articleBarCode}");
    var articleAsList = articleAsDataTable.ConvertDataTable<ArticleModel>();

    var articleMovementsDataTable = da.ReadDbf($"Select * from miscari where cod_art='{articleAsList.FirstOrDefault().cod}'");
    var inventoryMovements = articleMovementsDataTable.ConvertDataTable<InventoryMovementModel>();

    if (inventoryMovements.Count == 1)
    {
        var inventoryMovement = inventoryMovements.FirstOrDefault();

        Console.WriteLine("Type quantity");
        var quantity = Console.ReadLine();
        if (Convert.ToDecimal(quantity) > 100000)
        {
            quantity = "1";
        }

        //ID_U,N,10,0	ID,N,10,0	DATA,D	COD_ART,C,16	
        //GESTIUNE,C,4	CANTITATE,N,14,3	CANT_NESTI,N,14,3	PRET,N,15,4	TIP_DOC,C,10	NR_DOC,C,16	SUMA_DESC,N,15,4
        var lastExitId = da.ReadDbf("Select top 1 id_u from ies_det order by id_u desc").Rows[0][0];
        var article = articleAsList.SingleOrDefault();

        // Iesire simpla pe aceeasi gestiune
        var inventoryExit = new InventoryExitModel
        {
            id_u = (decimal)lastExitId + 1,
            id_iesire = (decimal)exitDocumentId.Rows[0][0],
            gestiune = inventoryMovement?.gestiune,
            den_gest = article?.den_gest,
            cod = inventoryMovement?.cod_art,
            denumire = article?.denumire,
            cantitate = Convert.ToDecimal(quantity),
            den_tip = "Marfuri",
            um = "BUC",
        };

        Console.WriteLine("Rows affected: " + da.InsertIntoIesiriDbf(inventoryExit));
    }

    if (inventoryMovements.Count > 1)
    {
        // Iesire alternativa, cate o bucata din fiecare gestiune pe rand!

        // Daca exista, caut daca mai exista o pozitie din acelasi produs
        // daca nu, adaug o pozitie cu cantitatea 1 sau adaug mai multe pozitii (maximum cate gestiuni sunt) in distribui alternativ cantitatea
        // daca mai exista pozitii din nou continui cu urmatorea gestiune 
        // ATENTIE! conteaza cantitatea pe gestiune deci o gestiune poate avea mai putine bucati => cand nu mai are nu mai descarc de acolo
    }

    if (inventoryMovements.Count == 0)
    {
        Console.WriteLine("Nu au fost gasite intrari pentru acest produs");
    }
}

//var foxProDbfDateTable = da.ReadDbf("Select top 1 * from ies_det order by id_u desc");
//
//var foxProDbfAsList = foxProDbfDateTable.ConvertDataTable<InventoryExitModel>();
//
//foreach (var item in foxProDbfAsList)
//{
//    Console.WriteLine(item.id_u);
//}

//da.InsertIntoDbf<InventoryExitModel>();
