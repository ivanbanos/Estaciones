import { EstacionUI } from './../../models/UIModels/EstacionUI';
import {
  Component, EventEmitter, Input, Output
} from '@angular/core';

@Component({
  selector: 'app-estacion-item',
  templateUrl: './estacion-item.component.html',
  styleUrls: ['./estacion-item.component.css']
})
export class EstacionItemComponent {

  @Input()
  estacion: EstacionUI;

  @Output()
  selectElement = new EventEmitter();

  @Output()
  editClicked = new EventEmitter();

  constructor() { }

  selectItem = () => {
    this.estacion.seleccionado = !this.estacion.seleccionado;
    this.selectElement.emit(this.estacion);
  }

  editar = () => {
    this.editClicked.emit(this.estacion);
  }

}
