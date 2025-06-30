// ===============================================================================================================
// FacturasCanastillas Service - JavaScript implementation for basket invoices management
// ===============================================================================================================

import HttpService from './HttpService.js';

class FacturasCanastillasService {
  constructor() {
    this.httpService = new HttpService();
    this.url = `${window.SERVER_URL}/FacturasCanastillas`;
  }

  async getFacturasCanastillas() {
    try {
      return await this.httpService.get(this.url);
    } catch (error) {
      console.error('Error getting facturas canastillas:', error);
      throw error;
    }
  }

  async getFacturaCanastilla(id) {
    try {
      return await this.httpService.get(`${this.url}/${id}`);
    } catch (error) {
      console.error(`Error getting factura canastilla ${id}:`, error);
      throw error;
    }
  }

  async addOrUpdate(facturaCanastilla) {
    try {
      return await this.httpService.post(this.url, facturaCanastilla);
    } catch (error) {
      console.error('Error adding/updating factura canastilla:', error);
      throw error;
    }
  }

  async updateFacturaCanastilla(id, facturaCanastilla) {
    try {
      return await this.httpService.put(`${this.url}/${id}`, facturaCanastilla);
    } catch (error) {
      console.error(`Error updating factura canastilla ${id}:`, error);
      throw error;
    }
  }

  async deleteFacturaCanastilla(id) {
    try {
      return await this.httpService.delete(`${this.url}/${id}`);
    } catch (error) {
      console.error(`Error deleting factura canastilla ${id}:`, error);
      throw error;
    }
  }

  async getFacturasCanastillasPorCanastilla(idCanastilla) {
    try {
      return await this.httpService.get(`${this.url}/canastilla/${idCanastilla}`);
    } catch (error) {
      console.error(`Error getting facturas for canastilla ${idCanastilla}:`, error);
      throw error;
    }
  }

  async getFacturasCanastillasPorFecha(fechaInicio, fechaFin) {
    try {
      const params = { fechaInicio, fechaFin };
      return await this.httpService.get(`${this.url}/fecha`, params);
    } catch (error) {
      console.error('Error getting facturas canastillas by date:', error);
      throw error;
    }
  }

  async getFacturasCanastillasPorEstacion(idEstacion) {
    try {
      return await this.httpService.get(`${this.url}/estacion/${idEstacion}`);
    } catch (error) {
      console.error(`Error getting facturas canastillas for station ${idEstacion}:`, error);
      throw error;
    }
  }

  async generarReporteCanastillas(parametros) {
    try {
      return await this.httpService.post(`${this.url}/reporte`, parametros);
    } catch (error) {
      console.error('Error generating facturas canastillas report:', error);
      throw error;
    }
  }
}

export default FacturasCanastillasService;
