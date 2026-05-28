import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'ui-navbar-component',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class UiNavbarComponent {
  dropOpciones: boolean = false;

  toggleDropdown() {
    this.dropOpciones = !this.dropOpciones;
  }
  
  cerrarSesion() {
    // Tu lógica de cerrar sesión
    console.log('Cerrando sesión...');
    this.dropOpciones = false; // Cierra el dropdown después de cerrar sesión
  }
}