import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../components/shared/icono/icono.component';
import { ModalDetallesUsuarioComponent } from './detalles-usuario/detalles-usuario'; 

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent, ModalDetallesUsuarioComponent],
  styleUrl: './lista-usuario.css',
  templateUrl: './lista-usuario.html',
})
export class PagueListaUsuario implements OnInit {
  // Filtros
  filtroTexto: string = '';
  filtroEstatus: string = '';
  filtroFechaInicio: string = '';
  filtroFechaFin: string = '';

  // Paginación
  filasPorPagina: number = 7;
  paginaActual: number = 1;
  opcionesFilasPorPagina: number[] = [7, 10, 25, 50];

  // Estado skeleton
  loadingData: boolean = true;

  // Datos
  usuariosFiltrados: any[] = [];
  
  // Modal
  usuarioSeleccionado: any = null;
  modalVisible: boolean = false;

  usuariosOriginales: any[] = [];

  usuarios: any[] = [
    {
        id: 'USR-001',
        RFC: 'LDMG270120DX2',
        EstadoUsuario: true,
        nombre: 'José Manuel Díez Calderón',
        correoElectronico: 'josemanueltadeo2003@gmail.com',
        limiteCredito: 500000,
        diasCredito: 15,
        limiteDocumentosVencido: 14,
        permisoExcederLimiteCredito: false,
        telefonoFijo: '6183316933',
        telefonoWhatsApp: '6183316933',
        fechaRegistro: '2024-01-15',
        fechaRegistroFormateada: '15 ene 2024',
        domicilios: [
            {
                nombreDomicilio: 'Principal',
                pais: 'México',
                estado: 'Durango',
                numeroExterior: '#129',
                calle: 'Calle de los Olivos #452',
                numeroInterior: '#190',
                codigoPostal: '30040',
                municipio: 'Victoria Durango'
            },
            {
                nombreDomicilio: 'Secundario',
                pais: 'México',
                estado: 'Durango',
                numeroExterior: '#45',
                calle: 'Boulevard de la Juventud',
                numeroInterior: '3B',
                codigoPostal: '34120',
                municipio: 'Durango'
            }
        ],
        datosFiscales: {
            regimenFiscal: 'Régimen General de Ley',
            razonSocial: 'José Manuel Díez Calderón',
            usoCFDI: 'Gastos en general'
        },
        constanciaFiscal: false
    },
    {
        id: 'USR-002',
        RFC: 'LOMR850620ABC',
        EstadoUsuario: true,
        nombre: 'María Fernanda López Ramírez',
        correoElectronico: 'maria.lopez@example.com',
        limiteCredito: 750000,
        diasCredito: 30,
        limiteDocumentosVencido: 20,
        permisoExcederLimiteCredito: true,
        telefonoFijo: '5551234567',
        telefonoWhatsApp: '5559876543',
        fechaRegistro: '2024-01-20',
        fechaRegistroFormateada: '20 ene 2024',
        domicilios: [
            {
                nombreDomicilio: 'Principal',
                pais: 'México',
                estado: 'CDMX',
                numeroExterior: '#45',
                calle: 'Av. Insurgentes Sur #123',
                numeroInterior: '#12',
                codigoPostal: '01020',
                municipio: 'Álvaro Obregón'
            }
        ],
        datosFiscales: {
            regimenFiscal: 'Régimen de Incorporación Fiscal',
            razonSocial: 'María Fernanda López Ramírez',
            usoCFDI: 'Gastos médicos'
        },
        constanciaFiscal: true
    },
    {
        id: 'USR-003',
        RFC: 'HEGC900101XYZ',
        EstadoUsuario: false,
        nombre: 'Carlos Hernández Gómez',
        correoElectronico: 'carlos.hg@example.com',
        limiteCredito: 0,
        diasCredito: 0,
        limiteDocumentosVencido: 0,
        permisoExcederLimiteCredito: false,
        telefonoFijo: '4421234567',
        telefonoWhatsApp: '4427654321',
        fechaRegistro: '2024-01-25',
        fechaRegistroFormateada: '25 ene 2024',
        domicilios: [
            {
                nombreDomicilio: 'Principal',
                pais: 'México',
                estado: 'Querétaro',
                numeroExterior: '#78',
                calle: 'Calle Corregidora #56',
                numeroInterior: '#5',
                codigoPostal: '76000',
                municipio: 'Querétaro'
            },
            {
                nombreDomicilio: 'Oficina',
                pais: 'México',
                estado: 'Querétaro',
                numeroExterior: '#120',
                calle: 'Boulevard Bernardo Quintana',
                numeroInterior: 'Piso 3',
                codigoPostal: '76020',
                municipio: 'Querétaro'
            },
            {
                nombreDomicilio: 'Segunda residencia',
                pais: 'México',
                estado: 'Guanajuato',
                numeroExterior: '#8',
                calle: 'Calle del Sol',
                numeroInterior: 'A',
                codigoPostal: '36000',
                municipio: 'Celaya'
            }
        ],
        datosFiscales: {
            regimenFiscal: 'Régimen General de Ley',
            razonSocial: 'Carlos Hernández Gómez',
            usoCFDI: 'Honorarios profesionales'
        },
        constanciaFiscal: false
    },
    {
        id: 'USR-004',
        RFC: 'MADA950505LMN',
        EstadoUsuario: true,
        nombre: 'Ana Sofía Martínez Delgado',
        correoElectronico: 'ana.sofia@example.com',
        limiteCredito: 1000000,
        diasCredito: 45,
        limiteDocumentosVencido: 25,
        permisoExcederLimiteCredito: true,
        telefonoFijo: '3331239876',
        telefonoWhatsApp: '3336547890',
        fechaRegistro: '2024-02-01',
        fechaRegistroFormateada: '1 feb 2024',
        domicilios: [
            {
                nombreDomicilio: 'Principal',
                pais: 'México',
                estado: 'Jalisco',
                numeroExterior: '#20',
                calle: 'Av. Juárez #789',
                numeroInterior: '#3',
                codigoPostal: '44100',
                municipio: 'Guadalajara'
            }
        ],
        datosFiscales: {
            regimenFiscal: 'Régimen de Actividades Empresariales',
            razonSocial: 'Ana Sofía Martínez Delgado',
            usoCFDI: 'Actividades empresariales'
        },
        constanciaFiscal: true
    },
    {
        id: 'USR-005',
        RFC: 'TOML880808OPQ',
        EstadoUsuario: true,
        nombre: 'Luis Alberto Torres Morales',
        correoElectronico: 'luis.torres@example.com',
        limiteCredito: 600000,
        diasCredito: 40,
        limiteDocumentosVencido: 20,
        permisoExcederLimiteCredito: true,
        telefonoFijo: '8181234567',
        telefonoWhatsApp: '8187654321',
        fechaRegistro: '2024-02-10',
        fechaRegistroFormateada: '10 feb 2024',
        domicilios: [
            {
                nombreDomicilio: 'Principal',
                pais: 'México',
                estado: 'Nuevo León',
                numeroExterior: '#100',
                calle: 'Calle Hidalgo #321',
                numeroInterior: '#15',
                codigoPostal: '64000',
                municipio: 'Monterrey'
            },
            {
                nombreDomicilio: 'Sucursal',
                pais: 'México',
                estado: 'Nuevo León',
                numeroExterior: '#200',
                calle: 'Av. Revolución',
                numeroInterior: 'Local 5',
                codigoPostal: '64100',
                municipio: 'San Pedro Garza García'
            }
        ],
        datosFiscales: {
            regimenFiscal: 'Régimen General de Ley',
            razonSocial: 'Luis Alberto Torres Morales',
            usoCFDI: 'Actividades empresariales'
        },
        constanciaFiscal: true
    }
  ];

  ngOnInit(): void {
    this.cargarDatosConSkeleton();
  }

  cargarDatosConSkeleton(): void {
    this.loadingData = true;
    
    setTimeout(() => {
      this.usuariosOriginales = [...this.usuarios];
      this.usuariosFiltrados = [...this.usuarios];
      this.loadingData = false;
    }, 2000);
  }

  get totalPaginas(): number {
    return Math.ceil(this.usuariosFiltrados.length / this.filasPorPagina);
  }

  get totalItemsRegistros(): string {
    if (this.usuariosFiltrados.length === 0) return '0-0 de 0';
    const inicio = (this.paginaActual - 1) * this.filasPorPagina + 1;
    const fin = Math.min(this.paginaActual * this.filasPorPagina, this.usuariosFiltrados.length);
    return `${inicio}-${fin} de ${this.usuariosFiltrados.length}`;
  }

  get usuariosPaginados(): any[] {
    const inicio = (this.paginaActual - 1) * this.filasPorPagina;
    return this.usuariosFiltrados.slice(inicio, inicio + this.filasPorPagina);
  }

  buscarUsuarios() {
    this.paginaActual = 1;
    
    this.usuariosFiltrados = this.usuariosOriginales.filter((usuario) => {
      const coincideTexto = this.filtroTexto
        ? usuario.RFC.toLowerCase().includes(this.filtroTexto.toLowerCase()) ||
          usuario.nombre.toLowerCase().includes(this.filtroTexto.toLowerCase())
        : true;

      const coincideEstatus = this.filtroEstatus !== ''
        ? usuario.permisoExcederLimiteCredito.toString() === this.filtroEstatus
        : true;

      const coincideFechaInicio = this.filtroFechaInicio
        ? usuario.fechaRegistro >= this.filtroFechaInicio
        : true;

      const coincideFechaFin = this.filtroFechaFin
        ? usuario.fechaRegistro <= this.filtroFechaFin
        : true;

      return coincideTexto && coincideEstatus && coincideFechaInicio && coincideFechaFin;
    });
  }

  limpiarFiltros() {
    this.filtroTexto = '';
    this.filtroEstatus = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.buscarUsuarios();
  }

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

  verDetalleUsuario(usuario: any) {
    this.usuarioSeleccionado = usuario;
    this.modalVisible = true;
  }

  cerrarModal() {
    this.modalVisible = false;
    this.usuarioSeleccionado = null;
  }

  registrarUsuario() {
    console.log('Registrar usuario');
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      minimumFractionDigits: 2
    }).format(value);
  }
}