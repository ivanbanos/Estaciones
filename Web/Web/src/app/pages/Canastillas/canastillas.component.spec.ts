/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { CanastillasComponent } from './canastillas.component';

describe('CanastillasComponent', () => {
  let component: CanastillasComponent;
  let fixture: ComponentFixture<CanastillasComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ CanastillasComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CanastillasComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
