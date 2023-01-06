package ApiConnectionClasses;

import java.util.List;

import Modelo.Canastilla;
import Modelo.Cara;
import Modelo.Factura;
import Modelo.FacturaCanastilla;
import Modelo.FormasDePagos;
import Modelo.Isla;
import Modelo.Tercero;
import Modelo.TipoIdentificacion;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;

public interface IVentas {
    @GET("GetCaras")
    Call<List<Cara>> getCaras();
    @GET("GetIslas")
    Call<List<Isla>> getIslas();
    @GET("GetFormasPago")
    Call<List<FormasDePagos>> GetFormasPago();
    @GET("GetTipoIdentificaciones")
    Call<List<TipoIdentificacion>> GetTipoIdentificaciones();
    @GET("GetFactura")
    Call<Factura> getFactura(@Query("COD_CAR")int cod_car);
    @POST("ImprimirFactura/{imprimir}")
    Call<String> ImprimirFactura(@Path("imprimir") boolean imprimir, @Body Factura factura);
    @POST("EnviarFacturaElectronica")
    Call<String> EnviarFacturaElectronica(@Body Factura factura);

    @POST("Trama")
    Call<String> Trama(@Query("trama") String trama);
    @GET("GetTerceros")
    Call<Tercero> GetTerceros(@Query("identificacion") String identificacion);
    @POST("ConvertirAFactura")
    Call<Boolean> ConvertirAFactura(@Query("ventaId") int ventaId);
    @POST("ConvertirAOrden")
    Call<Boolean> ConvertirAOrden(@Query("ventaId") int ventaId);


    @GET("GetCanastilla")
    Call<List<Canastilla>> GetCanastilla();


    @POST("GenerarFacturaCanastilla/{imprimir}")
    Call<String> GenerarFacturaCanastilla(@Path("imprimir") boolean imprimir, @Body FacturaCanastilla factura);


    @GET("getFacturaCanastilla/{consecutivo}")
    Call<String> GetFacturaCanastilla(@Path("consecutivo") String consecutivo);
}
