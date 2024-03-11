export class FacturaUI {
    constructor(
        public consecutivo: number,
        public combustible: string,
        public cantidad: number,
        public precio: number,
        public total: number,
        public fecha: string,
        public guid: string,
        public descuento: number,
        public subTotal: number,
        public descripcionResolucion: string,
        public placa: string,
        public identificacion: string,
        public nombre: string,
        public estado: string,
        public formaDePago: string,
        public seleccionado: boolean,
        public idFacturaElectronica :string) { }
}
