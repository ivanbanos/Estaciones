import { TestBed } from '@angular/core/testing';

import { EstacionGuardService } from './estacion-guard.service';

describe('EstacionGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: EstacionGuardService = TestBed.get(EstacionGuardService);
    expect(service).toBeTruthy();
  });
});
