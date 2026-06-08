import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent } from '../tipografia/tipografia.component';  
import { UiIconComponent } from '../icono/icono.component'; 
import { UiEtiquetaComponent } from '../etiqueta/etiqueta.component'; 

type VarianteEtiqueta = 'positivo' | 'negativo' | 'neutro';

// Interfaz para tarjetas
export interface TarjetaConfig {
    id: number;
    nameIcono: string;
    titulo: string;
    viewSimbolo: boolean;
    porcentaje: string;
    estadoEtiqueta: VarianteEtiqueta;
    valor: number | number[];
    datos: [];
    viewDescripcion: boolean;
    descripcion?: string; 
    viewLabel: boolean;
}

@Component({
    selector: 'app-ui-card',
    standalone: true,
    imports: [CommonModule, UitipografiaComponent, UiIconComponent, UiEtiquetaComponent],
    templateUrl: './card.component.html'
})
export class UiCardComponent {
    @Input() nameIcono?: string = '';
    @Input() title?: string = '';
    @Input() valor?: number | number[] = 0;
    @Input() descripcion?: string ='';
    @Input() viewLabel: boolean = false;
    @Input() viewSimbolo: boolean = false;
    @Input() viewDescripcion: boolean = true;
    @Input() porcentaje: string = '';
    @Input() estadoEtiqueta: VarianteEtiqueta = "neutro";
    @Output() seleccionar = new EventEmitter<void>();
    @Input() selected: boolean = false;

    // Método para formatear el valor
    formatearValor(): string {
        if (!this.valor && this.valor !== 0) return '0';
        
        // Si es array, mostrar el primer valor
        if (Array.isArray(this.valor)) {
            const numValor = this.valor[0] || 0;
            return this.formatearNumero(numValor);
        }
        
        // Si es número
        return this.formatearNumero(this.valor);
    }

    // Método auxiliar para formatear números
    private formatearNumero(valor: number): string {
        // Si viewSimbolo es true, formatear como moneda
        if (this.viewSimbolo) {
            return new Intl.NumberFormat('en-US', {
                style: 'currency',
                currency: 'USD',
                minimumFractionDigits: 0,
                maximumFractionDigits: 0
            }).format(valor);
        }
        
        // Si el título contiene "tasa" o "éxito", formatear como porcentaje
        if (this.title?.toLowerCase().includes('tasa') || this.title?.toLowerCase().includes('éxito')) {
            return valor.toFixed(1) + '%';
        }
        
        // Por defecto, formatear como número con separadores de miles
        return valor.toLocaleString('en-US', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        });
    }
}