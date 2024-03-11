import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EstacionItemComponent } from './estacion-item.component';

describe('EstacionItemComponent', () => {
  let component: EstacionItemComponent;
  let fixture: ComponentFixture<EstacionItemComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EstacionItemComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EstacionItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
