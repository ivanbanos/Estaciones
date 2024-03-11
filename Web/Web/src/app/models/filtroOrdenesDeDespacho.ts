export class FiltroBusqueda {
    public fechaInicial: Date;
    public fechaFinal: Date;
    public identificacion: string;
    public nombreTercero: string;
    public estacion: string;

    constructor(fechaInicial: Date, fechaFinal: Date, identificacion: string, nombreTercero: string, estacion: string) {
        this.fechaInicial = fechaInicial;
        this.fechaFinal = fechaFinal;
        this.identificacion = identificacion;
        this.nombreTercero = nombreTercero;
        this.estacion = estacion;
    }
}
