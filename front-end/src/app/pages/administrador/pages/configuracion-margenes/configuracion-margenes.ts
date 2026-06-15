import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../components/shared/icono/icono.component';

@Component({
  selector: 'app-configuracion-margenes',
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent],
  templateUrl: './configuracion-margenes.html',
  styleUrl: './configuracion-margenes.css',
})
export class PagueConfiguracionMargenes implements OnInit {

  loadingData: boolean = true;

  // Arreglo original de categorías (valores por defecto)
  categoriasOriginales = [
    {
      id: 1,
      nombre: 'Oficina y Escolar',
      descripcion: 'Suministros básicos, papelería, archivos y consumibles de uso.',
      margen: 15,
      icono: 'building-office-2',
    },
    {
      id: 2,
      nombre: 'Cómputo',
      descripcion: 'Laptops, pantallas grandes, internos y periféricos.',
      margen: 12,
      icono: 'computer-desktop',
    },
    {
      id: 3,
      nombre: 'Impresión y Multifuncionales',
      descripcion: 'Equipos láser, inyección de tinta y suministros de tóner originales.',
      margen: 15,
      icono: 'printer',
    },
    {
      id: 4,
      nombre: 'Electrónica',
      descripcion: 'Gadgets, accesorios, inteligentes y dispositivos electrónicos.',
      margen: 12,
      icono: 'device-phone-mobile',
    },
    {
      id: 5,
      nombre: 'Redes y Comunicaciónes',
      descripcion: 'Routers, switches, cableado estructurado y soluciones.',
      margen: 10,
      icono: 'server-stack',
    },
    {
      id: 6,
      nombre: 'Audio y Video',
      descripcion: 'Sistemas de sonidos, pantallas profesionales y accesorios.',
      margen: 14,
      icono: 'musical-note',
    },
    {
      id: 7,
      nombre: 'Seguridad',
      descripcion: 'Cámaras CCTV, sistemas de alarma y control de acceso.',
      margen: 12,
      icono: 'shield-check',
    },    
  ];

  // Arreglo de categorías que se mostrarán (copia editable)
  categorias: any[] = [];

  ngOnInit(): void {
    this.cargarDatos();
  }

  cargarDatos() {
    this.loadingData = true;
    
    // Simular carga de datos (como una petición HTTP)
    setTimeout(() => {
      // Copiar los datos originales
      this.categorias = JSON.parse(JSON.stringify(this.categoriasOriginales));
      this.loadingData = false;
    }, 1500);
  }

  // Método para guardar los cambios
  guardarCambios() {
    console.log('Márgenes guardados:', this.categorias);
    alert('Configuración guardada exitosamente');
  }

  // Método para restablecer los valores originales
  restablecerValores() {
    this.categorias = JSON.parse(JSON.stringify(this.categoriasOriginales));
    alert('Valores restablecidos a los originales');
  }
}