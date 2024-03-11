/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { FacturasCanastillasService } from './FacturasCanastillas.service';

describe('Service: Canastillas', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FacturasCanastillasService]
    });
  });

  it('should ...', inject([FacturasCanastillasService], (service: FacturasCanastillasService) => {
    expect(service).toBeTruthy();
  }));
});
