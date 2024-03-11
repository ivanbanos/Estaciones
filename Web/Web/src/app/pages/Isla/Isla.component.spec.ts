/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { IslaComponent } from './Isla.component';

describe('IslaComponent', () => {
  let component: IslaComponent;
  let fixture: ComponentFixture<IslaComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ IslaComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IslaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
