/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { TipoIdentificacionService } from './TipoIdentificacion.service';

describe('Service: TipoIdentificacion', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TipoIdentificacionService]
    });
  });

  it('should ...', inject([TipoIdentificacionService], (service: TipoIdentificacionService) => {
    expect(service).toBeTruthy();
  }));
});
