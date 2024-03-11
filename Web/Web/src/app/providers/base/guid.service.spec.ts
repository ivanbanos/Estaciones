import { TestBed } from '@angular/core/testing';

import { GuidService } from './guid.service';

describe('GuidService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: GuidService = TestBed.get(GuidService);
    expect(service).toBeTruthy();
  });
});
