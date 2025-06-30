// ===============================================================================================================
// Canastillas Service - JavaScript implementation for basket management
// ===============================================================================================================

import HttpService from './HttpService.js';

class CanastillasService {
  constructor() {
    this.httpService = new HttpService();
    this.url = `${window.SERVER_URL}/Canastilla`;
  }

  async getCanastillas() {
    try {
      return await this.httpService.get(this.url);
    } catch (error) {
      console.error('Error getting canastillas:', error);
      throw error;
    }
  }

  async getCanastilla(idCanastilla) {
    try {
      return await this.httpService.get(`${this.url}/${idCanastilla}`);
    } catch (error) {
      console.error(`Error getting canastilla ${idCanastilla}:`, error);
      throw error;
    }
  }

  async addOrUpdate(canastillas) {
    try {
      return await this.httpService.post(this.url, canastillas);
    } catch (error) {
      console.error('Error adding/updating canastillas:', error);
      throw error;
    }
  }

  async updateCanastilla(idCanastilla, canastilla) {
    try {
      return await this.httpService.put(`${this.url}/${idCanastilla}`, canastilla);
    } catch (error) {
      console.error(`Error updating canastilla ${idCanastilla}:`, error);
      throw error;
    }
  }

  async deleteCanastilla(idCanastilla) {
    try {
      return await this.httpService.delete(`${this.url}/${idCanastilla}`);
    } catch (error) {
      console.error(`Error deleting canastilla ${idCanastilla}:`, error);
      throw error;
    }
  }
}

export default CanastillasService;
