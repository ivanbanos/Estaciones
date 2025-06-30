// ===============================================================================================================
// OrdenDeDespacho Service - JavaScript implementation for dispatch order management
// ===============================================================================================================

import HttpService from './HttpService.js';

class OrdenDeDespachoService {
  constructor() {
    this.httpService = new HttpService();
    this.url = `${window.SERVER_URL}/OrdenDeDespacho`;
  }

  async getOrdenesDeDespacho() {
    try {
      return await this.httpService.get(this.url);
    } catch (error) {
      console.error('Error getting ordenes de despacho:', error);
      throw error;
    }
  }

  async getOrdenDeDespacho(idOrden) {
    try {
      return await this.httpService.get(`${this.url}/${idOrden}`);
    } catch (error) {
      console.error(`Error getting orden de despacho ${idOrden}:`, error);
      throw error;
    }
  }

  async addOrUpdate(ordenDeDespacho) {
    try {
      return await this.httpService.post(this.url, ordenDeDespacho);
    } catch (error) {
      console.error('Error adding/updating orden de despacho:', error);
      throw error;
    }
  }

  async updateOrdenDeDespacho(idOrden, ordenDeDespacho) {
    try {
      return await this.httpService.put(`${this.url}/${idOrden}`, ordenDeDespacho);
    } catch (error) {
      console.error(`Error updating orden de despacho ${idOrden}:`, error);
      throw error;
    }
  }

  async deleteOrdenDeDespacho(idOrden) {
    try {
      return await this.httpService.delete(`${this.url}/${idOrden}`);
    } catch (error) {
      console.error(`Error deleting orden de despacho ${idOrden}:`, error);
      throw error;
    }
  }

  async getOrdenesPorFecha(fechaInicio, fechaFin) {
    try {
      const params = { fechaInicio, fechaFin };
      return await this.httpService.get(`${this.url}/fecha`, params);
    } catch (error) {
      console.error('Error getting ordenes by date:', error);
      throw error;
    }
  }

  async getOrdenesPorEstacion(idEstacion) {
    try {
      return await this.httpService.get(`${this.url}/estacion/${idEstacion}`);
    } catch (error) {
      console.error(`Error getting ordenes for station ${idEstacion}:`, error);
      throw error;
    }
  }

  async getOrdenesPorEstado(estado) {
    try {
      return await this.httpService.get(`${this.url}/estado/${estado}`);
    } catch (error) {
      console.error(`Error getting ordenes by state ${estado}:`, error);
      throw error;
    }
  }

  async cambiarEstadoOrden(idOrden, estado) {
    try {
      return await this.httpService.patch(`${this.url}/${idOrden}/estado`, { estado });
    } catch (error) {
      console.error(`Error changing state of orden ${idOrden}:`, error);
      throw error;
    }
  }

  async procesarOrden(idOrden) {
    try {
      return await this.httpService.post(`${this.url}/${idOrden}/procesar`);
    } catch (error) {
      console.error(`Error processing orden ${idOrden}:`, error);
      throw error;
    }
  }

  async cancelarOrden(idOrden, motivo) {
    try {
      return await this.httpService.post(`${this.url}/${idOrden}/cancelar`, { motivo });
    } catch (error) {
      console.error(`Error cancelling orden ${idOrden}:`, error);
      throw error;
    }
  }
}

export default OrdenDeDespachoService;
