package Modelo;

import java.util.Date;
import java.util.List;

public class FacturaCanastilla {
    public int FacturasCanastillaId;
    public Date fecha;
    public Resolucion resolucion ;
    public int consecutivo;
    public String estado;
    public Tercero terceroId ;
    public int impresa ;
    public int enviada ;
    public FormasPagos codigoFormaPago ;
    public List<CanastillaFactura> canastillas ;
    public float subtotal ;
    public float descuento ;
    public float iva;
    public float total;
}
