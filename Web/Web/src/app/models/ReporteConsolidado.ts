import { Consolidado } from './Consolidado';
import { ConsolidadoCliente } from './ConsolidadoCliente';
import { Factura } from './Factura';
import { OrdenDeDespacho } from './OrdenDeDespacho';

export interface ReporteConsolidado {
    consecutivoFacturaInicial: number;
    consecutivoDeFacturaFinal: number;
    totalDeVentas: number;
    totalDeOrdenes: number;
    consolidados: Array<Consolidado>;
    consolidadosOrdenes: Array<Consolidado>;
    consolidadoClienteFacturas: Array<ConsolidadoCliente>;
    consolidadoClienteOrdenes: Array<ConsolidadoCliente>;
    cantidadDeFacturasAnuladas: number;
    consolidadoFacturasAnuladas: Array<Consolidado>;
    consolidadoOrdenesAnuladas: Array<Consolidado>;
    totalOrdenesAnuladas: number;
    totalFacturasAnuladas: number;
}
