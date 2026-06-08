import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-ui-icono',
    standalone: true,
    templateUrl: './icono.component.html',
    imports: [CommonModule],
})
export class UiIconComponent {
    @Input() name: string = 'check'; 
    @Input() size: string = 'h-6 w-6'; 
    @Input() color: string = 'text-gray-700'; 
}
