import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { OrdenDeDespachoUI } from 'src/app/models/UIModels/OrdenDeDespachoUI';
import { OrdenDeDespachoService } from 'src/app/providers/ordenDeDespacho/orden-de-despacho.service';

@Component({
  selector: 'app-orden-item',
  templateUrl: './orden-item.component.html',
  styleUrls: ['./orden-item.component.css']
})
export class OrdenItemComponent {

  @Input()
  ordenDeDespacho: OrdenDeDespachoUI;

  @Output()
  selectElement = new EventEmitter();

  constructor(
    public readonly ordenDeDespachoService: OrdenDeDespachoService,
    public readonly toast: ToastrService,) { }

  selectItem = () => {
    this.ordenDeDespacho.seleccionado = !this.ordenDeDespacho.seleccionado;
    this.selectElement.emit(this.ordenDeDespacho);
  }
  async EnviarFacturacion() {
    
    const response = await this.ordenDeDespachoService.EnviarFacturacion(this.ordenDeDespacho.guid)
      .toPromise()
      .catch(error => {
        console.log(error);
        this.toast.error(`Error grave al generar factura ${error}`);
      });
      console.log(response);
      if(response){
        if (response == "Ok") {
          this.ordenDeDespacho.idFacturaElectronica = "NoNulo";
          this.toast.success(`Factura electrónica generada`);
        } else{
          this.toast.warning(`Factura electrónica no generada.  Razón ${response}`);
        }
        }
  }
}
