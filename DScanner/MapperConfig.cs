using Mapster;
using DScannerLibrary.Models;
using DScanner.Models;

namespace DScanner;

public class MapperConfig
{
    public static TypeAdapterConfig GetAdapterConfig()
    {
        var adapterConfig = new TypeAdapterConfig();
        adapterConfig.NewConfig<InventoryExitModel, InventoryExitDisplayModel>()
            .Map(dest => dest.Gestiune, src => src.den_gest)
            .Map(dest => dest.Denumire, src => src.denumire)
            .Map(dest => dest.CodProdus, src => src.cod)
            //.Map(dest => dest.TipArticol, src => src.den_tip)
            //.Map(dest => dest.UM, src => src.um)
            .Map(dest => dest.Cantitate, src => src.cantitate)
            .Map(dest => dest.PretUnitar, src => src.pret_unitar)
            .Map(dest => dest.Total, src => src.total)
            .Map(dest => dest.Adaos, src => src.adaos)
            .Map(dest => dest.Cont, src => src.cont)
            .Map(dest => dest.TextSuplimentar, src => src.text_supl);

        return adapterConfig;
    }
}
