export class FacturaCanastillaUI {
    constructor( 
        public consecutivo : number,
        public fecha: string,
        public nombre: string,
        public subtotal : number,
        public descuento : number,
        public iva : number,
        public total : number,
        public guid: string,
        public facturasCanastillaId : number,
        public segundo: string,
        public apellidos: string,
        public identificacion: string,
        public tipoIdentificacion: string,
        public formaDePago: string){}
}