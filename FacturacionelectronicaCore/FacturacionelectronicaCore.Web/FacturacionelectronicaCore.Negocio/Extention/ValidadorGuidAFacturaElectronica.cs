using System;
using System.Collections.Generic;
using System.Linq;

namespace EstacionesServicio.Negocio.Extention
{
    public class ValidadorGuidAFacturaElectronica : IValidadorGuidAFacturaElectronica
    {
        public readonly List<Guid> guidsSiendoProcesados;
        public readonly Dictionary<Guid,Queue<Guid>> facturasImprimirCanastilla;

        public ValidadorGuidAFacturaElectronica()
        {
            guidsSiendoProcesados = new List<Guid>();
            facturasImprimirCanastilla = new Dictionary <Guid,Queue<Guid>>();
        }

        

        public bool FacturaSiendoProceada(Guid ordenGuid)
        {
            lock (guidsSiendoProcesados)
            {
                if (guidsSiendoProcesados.Contains(ordenGuid))
                {
                    return true;
                }
                else
                {
                    guidsSiendoProcesados.Add(ordenGuid);
                    return false;
                }
            }
        }

        public bool FacturasSiendoProceada(IEnumerable<Guid> facturasGuids)
        {
            lock (guidsSiendoProcesados)
            {
                if (guidsSiendoProcesados.Any(x => facturasGuids.Contains(x)))
                {
                    return true;
                }
                else
                {
                    guidsSiendoProcesados.AddRange(facturasGuids);
                    return false;
                }
            }
        }

        public void SacarFactura(Guid ordenGuid)
        {
            lock (guidsSiendoProcesados)
            {
                guidsSiendoProcesados.Remove(ordenGuid);
            }
        }

        public void SacarFacturas(IEnumerable<Guid> facturasGuids)
        {
            lock (guidsSiendoProcesados)
            {
                guidsSiendoProcesados.RemoveAll(x => facturasGuids.Contains(x));
            }
        }


        public void AgregarAColaImpresionCanastilla(Guid guid, Guid idEstacion)
        {
            if (!facturasImprimirCanastilla.ContainsKey(idEstacion))
            {
                facturasImprimirCanastilla.Add(idEstacion, new Queue<Guid>());
            }
            facturasImprimirCanastilla[idEstacion].Enqueue(guid);
        }

        public Guid? ObtenerColaImpresionCanastilla(Guid idEstacion)
        {
            if (facturasImprimirCanastilla.ContainsKey(idEstacion) && facturasImprimirCanastilla[idEstacion].Count > 0)
            {
                return facturasImprimirCanastilla[idEstacion].Dequeue();
            }
            return null;
        }
    }
}