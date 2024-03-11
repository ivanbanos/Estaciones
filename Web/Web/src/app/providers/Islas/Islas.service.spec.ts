/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { IslasService } from './Islas.service';

describe('Service: Islas', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [IslasService]
    });
  });

  it('should ...', inject([IslasService], (service: IslasService) => {
    expect(service).toBeTruthy();
  }));
});
