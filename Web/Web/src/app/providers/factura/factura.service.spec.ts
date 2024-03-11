/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { FacturaService } from './factura.service';

describe('Service: Factura', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FacturaService]
    });
  });

  it('should ...', inject([FacturaService], (service: FacturaService) => {
    expect(service).toBeTruthy();
  }));
});
