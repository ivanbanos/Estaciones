/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { ResolucionService } from './resolucion.service';

describe('Service: Resolucion', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ResolucionService]
    });
  });

  it('should ...', inject([ResolucionService], (service: ResolucionService) => {
    expect(service).toBeTruthy();
  }));
});
