import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { FacturaItemComponent } from './factura-item.component';

describe('FacturaItemComponent', () => {
  let component: FacturaItemComponent;
  let fixture: ComponentFixture<FacturaItemComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FacturaItemComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FacturaItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
