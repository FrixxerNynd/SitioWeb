import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../components/shared/icono/icono.component';

@Component({
  selector: 'app-lista-pre-registro',
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent],
  templateUrl: './lista-pre-registro.html',
  styleUrl: './lista-pre-registro.css',
})
export class PagueListaPreRegistro implements OnInit {
  // Filtros
  filtroTexto: string = '';
  filtroEstatus: string = '';
  filtroEstatusContpaqi: string = '';
  filtroFechaInicio: string = '';
  filtroFechaFin: string = '';

  // Paginación
  filasPorPagina: number = 7;
  paginaActual: number = 1;
  opcionesFilasPorPagina: number[] = [7, 10, 25, 50];

  // Estado skeleton
  loadingData: boolean = true;

  // Datos
  preRegistrosFiltrados: any[] = [];
  preRegistrosOriginales: any[] = [];

  constructor(private router: Router) {}

  // Función para formatear fecha a "13 ene de 2026"
  formatearFecha(fechaStr: string): string {
    if (!fechaStr) return 'N/A';
    
    const fecha = new Date(fechaStr);
    const meses = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];
    
    const dia = fecha.getDate();
    const mes = meses[fecha.getMonth()];
    const anio = fecha.getFullYear();
    
    return `${dia} ${mes} de ${anio}`;
  }

  // Datos de ejemplo - Clientes con fechas formateadas
  clientes: any[] = [
      {
          id: 'CLI-001',
          nombre: 'José Manuel',
          apellidoPaterno: 'Díez',
          apellidoMaterno: 'Calderón',
          telefonoFijo: '6183316933',
          telefonoWhatsApp: '6183316933',
          rfc: 'LDMG270120DX2',
          estado: 'Pendiente',
          estadoContpaqi: 'SinEnlazar',
          fechaRegistro: '2024-01-15',
          fechaRegistroFormateada: '15 ene de 2024',
          direccion: {
              pais: 'México',
              codigoPostal: '30040',
              estado: 'Durango',
              municipio: 'Victoria Durango',
              calle: 'Calle de los Olivos #452',
              numeroExterior: '#129',
              numeroInterior: '#190',
          },
          cuenta: {
              correo: 'josemanueltadeo2003@gmail.com',
          },
          credito: null,
          constanciaFiscal: {
              disponible: false,
              url: null,
          },
      },
      {
          id: 'CLI-002',
          nombre: 'María Fernanda',
          apellidoPaterno: 'López',
          apellidoMaterno: 'Ramírez',
          telefonoFijo: '5551234567',
          telefonoWhatsApp: '5559876543',
          rfc: 'LOMR850620ABC',
          estado: 'Aprobado',
          estadoContpaqi: 'Enlazado',
          fechaRegistro: '2024-01-20',
          fechaRegistroFormateada: '20 ene de 2024',
          direccion: {
              pais: 'México',
              codigoPostal: '01020',
              estado: 'CDMX',
              municipio: 'Álvaro Obregón',
              calle: 'Av. Insurgentes Sur #123',
              numeroExterior: '#45',
              numeroInterior: '#12',
          },
          cuenta: {
              correo: 'maria.lopez@example.com',
          },
          credito: {
              limiteCredito: 500000.00,
              permisoExceder: true,
              diasCredito: 30,
              limiteDocumentosVencido: 15,
              estadoCredito: true,
          },
          constanciaFiscal: {
              disponible: true,
              url: '/assets/constancia-fiscal-ejemplo.pdf',
          },
      },
      {
          id: 'CLI-003',
          nombre: 'Carlos',
          apellidoPaterno: 'Hernández',
          apellidoMaterno: 'Gómez',
          telefonoFijo: '4421234567',
          telefonoWhatsApp: '4427654321',
          rfc: 'HEGC900101XYZ',
          estado: 'Denegado',
          estadoContpaqi: 'SinEnlazar',
          fechaRegistro: '2024-01-25',
          fechaRegistroFormateada: '25 ene de 2024',
          direccion: {
              pais: 'México',
              codigoPostal: '76000',
              estado: 'Querétaro',
              municipio: 'Querétaro',
              calle: 'Calle Corregidora #56',
              numeroExterior: '#78',
              numeroInterior: '#5',
          },
          cuenta: {
              correo: 'carlos.hg@example.com',
          },
          credito: null,
          constanciaFiscal: {
              disponible: false,
              url: null,
          },
      },
      {
          id: 'CLI-004',
          nombre: 'Ana Sofía',
          apellidoPaterno: 'Martínez',
          apellidoMaterno: 'Delgado',
          telefonoFijo: '3331239876',
          telefonoWhatsApp: '3336547890',
          rfc: 'MADA950505LMN',
          estado: 'Pendiente',
          estadoContpaqi: 'SinEnlazar',
          fechaRegistro: '2024-02-01',
          fechaRegistroFormateada: '1 feb de 2024',
          direccion: {
              pais: 'México',
              codigoPostal: '44100',
              estado: 'Jalisco',
              municipio: 'Guadalajara',
              calle: 'Av. Juárez #789',
              numeroExterior: '#20',
              numeroInterior: '#3',
          },
          cuenta: {
              correo: 'ana.sofia@example.com',
          },
          credito: null,
          constanciaFiscal: {
              disponible: true,
              url: '/assets/constancia-fiscal-ejemplo.pdf',
          },
      },
      {
          id: 'CLI-005',
          nombre: 'Luis Alberto',
          apellidoPaterno: 'Torres',
          apellidoMaterno: 'Morales',
          telefonoFijo: '8181234567',
          telefonoWhatsApp: '8187654321',
          rfc: 'TOML880808OPQ',
          estado: 'Aprobado',
          estadoContpaqi: 'Enlazado',
          fechaRegistro: '2024-02-10',
          fechaRegistroFormateada: '10 feb de 2024',
          direccion: {
              pais: 'México',
              codigoPostal: '64000',
              estado: 'Nuevo León',
              municipio: 'Monterrey',
              calle: 'Calle Hidalgo #321',
              numeroExterior: '#100',
              numeroInterior: '#15',
          },
          cuenta: {
              correo: 'luis.torres@example.com',
          },
          credito: {
              limiteCredito: 600000.00,
              permisoExceder: true,
              diasCredito: 40,
              limiteDocumentosVencido: 20,
              estadoCredito: true,
          },
          constanciaFiscal: {
              disponible: true,
              url: '/assets/constancia-fiscal-ejemplo.pdf',
          },
      },
  ];

  ngOnInit(): void {
    this.cargarDatosConSkeleton();
  }

  cargarDatosConSkeleton(): void {
    this.loadingData = true;
    
    setTimeout(() => {
      this.preRegistrosOriginales = [...this.clientes];
      this.preRegistrosFiltrados = [...this.clientes];
      this.loadingData = false;
    }, 2000);
  }

  // Navegación a página de detalle
  verDetallePreRegistro(cliente: any): void {
    this.router.navigate(['/administrador/lista-pre-registro/detalle', cliente.id]);
  }

  // Editar pre-registro
  editarPreRegistro(cliente: any): void {
    this.router.navigate(['/administrador/lista-pre-registro/editar', cliente.id]);
  }

  // Getters para paginación
  get totalPaginas(): number {
    return Math.ceil(this.preRegistrosFiltrados.length / this.filasPorPagina);
  }

  get totalItemsRegistros(): string {
    if (this.preRegistrosFiltrados.length === 0) return '0-0 de 0';
    const inicio = (this.paginaActual - 1) * this.filasPorPagina + 1;
    const fin = Math.min(this.paginaActual * this.filasPorPagina, this.preRegistrosFiltrados.length);
    return `${inicio}-${fin} de ${this.preRegistrosFiltrados.length}`;
  }

  get preRegistrosPaginados(): any[] {
    const inicio = (this.paginaActual - 1) * this.filasPorPagina;
    return this.preRegistrosFiltrados.slice(inicio, inicio + this.filasPorPagina);
  }

  // ==================== FILTROS ====================

  buscarPreRegistros() {
    this.paginaActual = 1;
    
    this.preRegistrosFiltrados = this.preRegistrosOriginales.filter((cliente) => {
      const coincideTexto = this.filtroTexto
        ? cliente.rfc.toLowerCase().includes(this.filtroTexto.toLowerCase()) ||
          cliente.nombre.toLowerCase().includes(this.filtroTexto.toLowerCase()) ||
          cliente.apellidoPaterno.toLowerCase().includes(this.filtroTexto.toLowerCase())
        : true;

      const coincideEstatus = this.filtroEstatus
        ? cliente.estado === this.filtroEstatus
        : true;

      const coincideEstatusContpaqi = this.filtroEstatusContpaqi
        ? cliente.estadoContpaqi === this.filtroEstatusContpaqi
        : true;

      const coincideFechaInicio = this.filtroFechaInicio
        ? cliente.fechaRegistro >= this.filtroFechaInicio
        : true;

      const coincideFechaFin = this.filtroFechaFin
        ? cliente.fechaRegistro <= this.filtroFechaFin
        : true;

      return coincideTexto && coincideEstatus && coincideEstatusContpaqi && coincideFechaInicio && coincideFechaFin;
    });
  }

  limpiarFiltros() {
    this.filtroTexto = '';
    this.filtroEstatus = '';
    this.filtroEstatusContpaqi = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.buscarPreRegistros();
  }

  // ==================== PAGINACIÓN ====================

  paginaAnterior() {
    if (this.paginaActual > 1) {
      this.paginaActual--;
    }
  }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) {
      this.paginaActual++;
    }
  }

  cambiarFilasPorPagina(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.filasPorPagina = Number(select.value);
    this.paginaActual = 1;
  }
}