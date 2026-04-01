using Mapster;
using DScannerLibrary.Models;
using DScannerTool.Models;

namespace DScannerTool;

public static class MapperConfig
{
    public static TypeAdapterConfig GetAdapterConfig()
    {
        var config = new TypeAdapterConfig();

        config.NewConfig<InventoryExitModel, InventoryExitDisplayModel>()
            .Map(dest => dest.Nr, src => 0)
            .Map(dest => dest.Gestiune, src => src.gestiune ?? string.Empty)
            .Map(dest => dest.Denumire, src => src.denumire ?? string.Empty)
            .Map(dest => dest.CodProdus, src => ParseCod(src.cod))
            .Map(dest => dest.Cantitate, src => src.cantitate)
            .Map(dest => dest.PretUnitar, src => src.pret_unitar)
            .Map(dest => dest.Total, src => src.total)
            .Map(dest => dest.TextSuplimentar, src => src.text_supl ?? string.Empty)
            .Map(dest => dest.Adaos, src => src.adaos)
            .Map(dest => dest.Cont, src => ParseCont(src.cont));

        return config;
    }

    private static int ParseCod(string? cod)
    {
        return int.TryParse(cod, out int result) ? result : 0;
    }

    private static decimal ParseCont(string? cont)
    {
        return decimal.TryParse(cont, out decimal result) ? result : 0;
    }
}