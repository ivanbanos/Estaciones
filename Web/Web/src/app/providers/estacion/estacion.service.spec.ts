/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { EstacionService } from './estacion.service';

describe('Service: Estacion', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [EstacionService]
    });
  });

  it('should ...', inject([EstacionService], (service: EstacionService) => {
    expect(service).toBeTruthy();
  }));
});
