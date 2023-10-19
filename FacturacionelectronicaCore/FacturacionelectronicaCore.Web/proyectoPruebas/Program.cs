// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

EnergiaEnRuta.ENRWSREGISTERSALEPortTypeClient canal = new EnergiaEnRuta.ENRWSREGISTERSALEPortTypeClient();
canal.Endpoint.Address = new System.ServiceModel.EndpointAddress("http://3.236.155.113/enr-devs/testing-qa/energia-en-ruta/public/api/ws/SalesServices");

//< soapenv:Header >
//< enr:Credencial >
//< !--Optional: https://energiaenruta.com/index.php/api/ws/SalesServices-->
//< enr:Usuario > 85 </ enr:Usuario >
//< !--Optional:-->
//< enr:Clave > 1D6512B5AFBEE877DA78B50A406C7492 </ enr:Clave >
//< !--Optional:-->
//< enr:CodigoEDS > 85 </ enr:CodigoEDS >
//</ enr:Credencial >
//</ soapenv:Header >



EnergiaEnRuta.RegistrarVentaCombustibleRequest solicitud = new EnergiaEnRuta.RegistrarVentaCombustibleRequest();

solicitud.name = new EnergiaEnRuta.solicitud_registro_venta() { 
    Placa = "",
    CodigoEDS = "",
    TotalVenta = 0,
};
//var response = await canal.RegistrarVentaCombustibleAsync(solicitud);


EnergiaEnRuta.SolicitarAutorizacionCreditoRequest solicitud2 = new EnergiaEnRuta.SolicitarAutorizacionCreditoRequest();

solicitud2.name = new EnergiaEnRuta.solicitud_autorizacion()
{
    CodigoEDS="84",
    TipoIdentificador = "Chip",
    NumeroIdentificador = "63000001CE6B5901",
};
var response2 = await canal.SolicitarAutorizacionCreditoAsync(solicitud2);
Console.Read();