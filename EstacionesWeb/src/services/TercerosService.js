// ===============================================================================================================
// Terceros Service - JavaScript implementation for third parties management
// ===============================================================================================================

import HttpService from './HttpService.js';

class TercerosService {
  constructor() {
    this.httpService = new HttpService();
    this.url = `${window.SERVER_URL}/Terceros`;
  }

  async getTerceros() {
    try {
      return await this.httpService.get(this.url);
    } catch (error) {
      console.error('Error getting terceros:', error);
      throw error;
    }
  }

  async getTercero(idTercero) {
    try {
      return await this.httpService.get(`${this.url}/${idTercero}`);
    } catch (error) {
      console.error(`Error getting tercero ${idTercero}:`, error);
      throw error;
    }
  }

  async addOrUpdate(tercero) {
    try {
      return await this.httpService.post(this.url, tercero);
    } catch (error) {
      console.error('Error adding/updating tercero:', error);
      throw error;
    }
  }

  async updateTercero(idTercero, tercero) {
    try {
      return await this.httpService.put(`${this.url}/${idTercero}`, tercero);
    } catch (error) {
      console.error(`Error updating tercero ${idTercero}:`, error);
      throw error;
    }
  }

  async deleteTercero(idTercero) {
    try {
      return await this.httpService.delete(`${this.url}/${idTercero}`);
    } catch (error) {
      console.error(`Error deleting tercero ${idTercero}:`, error);
      throw error;
    }
  }

  async buscarTerceros(criterio) {
    try {
      return await this.httpService.get(`${this.url}/buscar`, { criterio });
    } catch (error) {
      console.error('Error searching terceros:', error);
      throw error;
    }
  }

  async getTercerosPorTipo(tipo) {
    try {
      return await this.httpService.get(`${this.url}/tipo/${tipo}`);
    } catch (error) {
      console.error(`Error getting terceros by type ${tipo}:`, error);
      throw error;
    }
  }

  async getTercerosPorDocumento(tipoDocumento, numeroDocumento) {
    try {
      const params = { tipoDocumento, numeroDocumento };
      return await this.httpService.get(`${this.url}/documento`, params);
    } catch (error) {
      console.error('Error getting terceros by document:', error);
      throw error;
    }
  }

  async validarDocumento(tipoDocumento, numeroDocumento) {
    try {
      const params = { tipoDocumento, numeroDocumento };
      return await this.httpService.get(`${this.url}/validar-documento`, params);
    } catch (error) {
      console.error('Error validating document:', error);
      throw error;
    }
  }

  async getTercerosActivos() {
    try {
      return await this.httpService.get(`${this.url}/activos`);
    } catch (error) {
      console.error('Error getting active terceros:', error);
      throw error;
    }
  }

  async cambiarEstadoTercero(idTercero, estado) {
    try {
      return await this.httpService.patch(`${this.url}/${idTercero}/estado`, { estado });
    } catch (error) {
      console.error(`Error changing state of tercero ${idTercero}:`, error);
      throw error;
    }
  }
}

export default TercerosService;
