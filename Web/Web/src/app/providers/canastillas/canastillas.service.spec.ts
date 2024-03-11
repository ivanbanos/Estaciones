/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { CanastillasService } from './canastillas.service';

describe('Service: Canastillas', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CanastillasService]
    });
  });

  it('should ...', inject([CanastillasService], (service: CanastillasService) => {
    expect(service).toBeTruthy();
  }));
});
