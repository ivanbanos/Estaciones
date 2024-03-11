/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { OrdenDeDespachoService } from './orden-de-despacho.service';

describe('Service: OrdenDeDespacho', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [OrdenDeDespachoService]
    });
  });

  it('should ...', inject([OrdenDeDespachoService], (service: OrdenDeDespachoService) => {
    expect(service).toBeTruthy();
  }));
});
