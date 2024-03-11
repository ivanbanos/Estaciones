import { FacturaUI } from 'src/app/models/UIModels/FacturaUI';
import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FacturaService } from 'src/app/providers/factura/factura.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-factura-item',
  templateUrl: './factura-item.component.html',
  styleUrls: ['./factura-item.component.css']
})
export class FacturaItemComponent {

  @Input()
  factura: FacturaUI;

  @Output()
  selectElement = new EventEmitter();

  constructor(
    public readonly facturaService: FacturaService,
    public readonly toast: ToastrService,) { }

  selectItem = () => {
    this.factura.seleccionado = !this.factura.seleccionado;
    this.selectElement.emit(this.factura);
  }
  async EnviarFacturacion() {
    const response = await this.facturaService.EnviarFacturacion(this.factura.guid)
      .toPromise()
      .catch(error => {
        this.toast.error(`Error grave al generar factura ${error}`);
      });
      if(response){
        if (response == "Ok") {
          this.factura.idFacturaElectronica = "NoNulo";
          this.toast.success(`Factura electrónica generada`);
        } else{
          this.toast.warning(`Factura electrónica no generada.  Razón ${response}`);
        }
        }
  }
}
