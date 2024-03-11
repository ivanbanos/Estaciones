import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { InfoBusquedaComponent } from './info-busqueda.component';

describe('InfoBusquedaComponent', () => {
  let component: InfoBusquedaComponent;
  let fixture: ComponentFixture<InfoBusquedaComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ InfoBusquedaComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(InfoBusquedaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
