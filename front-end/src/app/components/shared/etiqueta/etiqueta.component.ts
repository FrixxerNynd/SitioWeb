import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UitipografiaComponent } from '../tipografia/tipografia.component'; 
import { UiIconComponent } from '../icono/icono.component'; 

export type VarianteEtiqueta = 'positivo' | 'negativo' | 'neutro';

@Component({
    selector: 'app-ui-etiqueta',
    standalone: true,
    imports: [UitipografiaComponent, UiIconComponent, CommonModule],
    template: `
        <div
        class="flex gap-1 items-center rounded-sm p-1"
        [ngClass]="mapaVariante[variante].bg"
        (click)="onSeleccionar.emit(texto)"
        >
        <app-ui-icono
            [name]="mapaVariante[variante].icono"
            size="size-4"
            [color]="mapaVariante[variante].color"
        />
        <app-ui-tipografia
            variante="caption-md"
            [texto]="texto"
            [color]="mapaVariante[variante].color"
        />
        </div>
    `,
    })
    export class UiEtiquetaComponent {
    @Input() texto: string = '';
    @Input() variante: VarianteEtiqueta = 'neutro';
    @Output() onSeleccionar = new EventEmitter<string>();

    mapaVariante: Record<VarianteEtiqueta, { bg: string; color: string; icono: string }> = {
        positivo: {
        bg: 'bg-green-100',
        color: 'text-green-800',
        icono: 'arrow-trending-up',
        },
        negativo: {
        bg: 'bg-red-100',
        color: 'text-red-800',
        icono: 'arrow-trending-down',
        },
        neutro: {
        bg: 'bg-zinc-100',
        color: 'text-zinc-800',
        icono: 'information-circle',
        },
    };
}