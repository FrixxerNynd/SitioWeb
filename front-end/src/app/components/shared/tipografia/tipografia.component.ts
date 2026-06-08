import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

type tipografiaVariantes =
    | 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'h6'
    | 'p'  | 'p-lg' | 'p-md' | 'p-sm'
    | 'caption' | 'caption-lg' | 'caption-md' | 'caption-sm';

@Component({
    selector: 'app-ui-tipografia',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './tipografia.component.html'
})
export class UitipografiaComponent {
    @Input() variante: tipografiaVariantes = 'p';
    @Input() texto?: string | number;
    @Input() color?: string;

    get classes(): string {
        const baseClasses = {
            // h
            h1: 'text-4xl font-bold',
            h2: 'text-3xl font-semibold',
            h3: 'text-2xl font-semibold',
            h4: 'text-xl font-medium',
            h5: 'text-lg font-medium',
            h6: 'text-base font-medium',

            // p
            p: 'text-base',
            'p-lg': 'text-lg',
            'p-md': 'text-base',
            'p-sm': 'text-sm font-medium',

            // caption 
            caption: 'text-sm font-medium',
            'caption-lg': 'text-sm font-medium',
            'caption-md': 'text-xs font-medium',
            'caption-sm': 'text-[11px] font-medium',
        }[this.variante];

        // Colores por defecto (sin color personalizado)
        const defaultColors = {
            // h
            h1: 'text-gray-800',
            h2: 'text-gray-700',
            h3: 'text-gray-700',
            h4: 'text-gray-600',
            h5: 'text-gray-600',
            h6: 'text-gray-500',

            // p
            p: 'text-gray-700',
            'p-lg': 'text-gray-800',
            'p-md': 'text-gray-700',
            'p-sm': 'text-zinc-700',

            // caption 
            caption: 'text-gray-500',
            'caption-lg': 'text-gray-500',
            'caption-md': 'text-gray-500',
            'caption-sm': 'text-gray-400',
        }[this.variante];

        // Si el usuario pasa un color, usarlo, sino usar el default
        const colorClass = this.color ? this.color : defaultColors;

        return `${baseClasses} ${colorClass}`.trim();
    }
}