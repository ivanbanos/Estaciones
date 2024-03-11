import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OrdenItemComponent } from './orden-item.component';

describe('OrdenItemComponent', () => {
  let component: OrdenItemComponent;
  let fixture: ComponentFixture<OrdenItemComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OrdenItemComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OrdenItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
