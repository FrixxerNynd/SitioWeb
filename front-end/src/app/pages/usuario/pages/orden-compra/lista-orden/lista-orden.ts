import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Inputs } from '../../../../../components/shared/inputs/inputs';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../../components/shared/icono/icono.component';
import { DetallesOrdenComponent } from './detalles-orden/detalles-orden'; 

@Component({
  selector: 'app-lista-orden',
  standalone: true,
  imports: [CommonModule, FormsModule, Inputs, UiBoton, UiIconComponent, DetallesOrdenComponent],
  styleUrl: './lista-orden.css',
  templateUrl: './lista-orden.html',
})
export class ListaOrdenPague implements OnInit {
  // Filtros
  filtroId: string = '';
  filtroEstatus: string = '';
  filtroFechaInicio: string = '';
  filtroFechaFin: string = '';

  // Estadísticas
  totalGastado: number = 0;
  creditoDisponible: number = 0;
  totalOrdenes: number = 0;
  ordenesProcesando: number = 0;

  // Paginación
  filasPorPagina: number = 7;
  paginaActual: number = 1;
  opcionesFilasPorPagina: number[] = [7, 10, 25, 50];

  // Datos
  ordenesFiltradas: any[] = [];
  
  // Modal
  ordenSeleccionada: any = null;
  modalVisible: boolean = false;

  // Opciones para el select de estado
  opcionesEstatus = [
    { label: 'Todos', value: '' },
    { label: 'Pendientes / Procesando', value: 'Procesando' },
    { label: 'Entregadas', value: 'Entregada' },
    { label: 'Canceladas', value: 'Cancelada' },
  ];

  // Datos de ejemplo (mantén tus 15 órdenes aquí)

  ordenesOriginales: any[] = [];

// Datos de ejemplo - 20 órdenes con diferentes estados y productos
ordenes: any[] = [
  {
    id: 'ORD-001',
    fecha: '2024-01-15',
    estado: 'Entregada',
    tipoPago: 'Tarjeta de Crédito',
    total: 1250.00,
    subtotal: 1000.00,
    flete: 100.00,
    iva: 150.00,
    cliente: 'Juan Pérez García',
    transportista: 'DHL Express',
    numFactura: 'FAC-001-2024',
    productos: [
      { nombre: 'Laptop HP 15.6" Core i5', sku: 'HP-156-I5-001', cantidad: 1, precioUnitario: 1000.00 },
      { nombre: 'Mouse Inalámbrico Logitech', sku: 'LOG-MOU-002', cantidad: 2, precioUnitario: 125.00 },
    ],
  },
  {
    id: 'ORD-002',
    fecha: '2024-01-20',
    estado: 'Procesando',
    tipoPago: 'Transferencia Bancaria',
    total: 890.00,
    subtotal: 712.00,
    flete: 90.00,
    iva: 88.00,
    cliente: 'María Guadalupe García López',
    transportista: 'Estafeta',
    numFactura: 'FAC-002-2024',
    productos: [
      { nombre: 'Monitor LG 24" Full HD', sku: 'LG-24FHD-003', cantidad: 1, precioUnitario: 712.00 },
    ],
  },
  {
    id: 'ORD-003',
    fecha: '2024-01-25',
    estado: 'Cancelada',
    tipoPago: 'Efectivo',
    total: 2300.00,
    subtotal: 1840.00,
    flete: 0,
    iva: 460.00,
    cliente: 'Carlos Alberto López Hernández',
    transportista: 'FedEx',
    numFactura: 'FAC-003-2024',
    productos: [
      { nombre: 'Teclado Mecánico RGB Redragon', sku: 'RED-KB-004', cantidad: 2, precioUnitario: 920.00 },
    ],
  },
  {
    id: 'ORD-004',
    fecha: '2024-02-01',
    estado: 'Procesando',
    tipoPago: 'PayPal',
    total: 3450.00,
    subtotal: 2760.00,
    flete: 90.00,
    iva: 600.00,
    cliente: 'Ana Sofía Rodríguez Martínez',
    transportista: 'DHL Express',
    numFactura: 'FAC-004-2024',
    productos: [
      { nombre: 'iPhone 13 128GB', sku: 'APP-IP13-005', cantidad: 1, precioUnitario: 15000.00 },
      { nombre: 'Funda Protectora iPhone', sku: 'FUN-IP-006', cantidad: 2, precioUnitario: 250.00 },
    ],
  },
  {
    id: 'ORD-005',
    fecha: '2024-02-10',
    estado: 'Entregada',
    tipoPago: 'Tarjeta de Débito',
    total: 560.00,
    subtotal: 448.00,
    flete: 90.00,
    iva: 22.00,
    cliente: 'Luis Fernando Martínez Sánchez',
    transportista: 'Estafeta',
    numFactura: 'FAC-005-2024',
    productos: [
      { nombre: 'Audífonos Bluetooth Sony', sku: 'SONY-AUD-007', cantidad: 1, precioUnitario: 448.00 },
    ],
  },
  {
    id: 'ORD-006',
    fecha: '2024-02-15',
    estado: 'Entregada',
    tipoPago: 'Tarjeta de Crédito',
    total: 3150.00,
    subtotal: 2520.00,
    flete: 90.00,
    iva: 540.00,
    cliente: 'Laura Patricia Fernández Torres',
    transportista: 'DHL Express',
    numFactura: 'FAC-006-2024',
    productos: [
      { nombre: 'Tablet Samsung Galaxy Tab S8', sku: 'SAM-TAB-008', cantidad: 1, precioUnitario: 2520.00 },
    ],
  },
  {
    id: 'ORD-007',
    fecha: '2024-02-20',
    estado: 'Procesando',
    tipoPago: 'Efectivo',
    total: 890.00,
    subtotal: 712.00,
    flete: 90.00,
    iva: 88.00,
    cliente: 'Roberto Carlos Sánchez Ruiz',
    transportista: 'FedEx',
    numFactura: 'FAC-007-2024',
    productos: [
      { nombre: 'Disco Duro Externo 1TB', sku: 'WD-1TB-009', cantidad: 2, precioUnitario: 356.00 },
    ],
  },
  {
    id: 'ORD-008',
    fecha: '2024-03-01',
    estado: 'Cancelada',
    tipoPago: 'Transferencia Bancaria',
    total: 1750.00,
    subtotal: 1400.00,
    flete: 90.00,
    iva: 260.00,
    cliente: 'Patricia Isabel Gómez Castro',
    transportista: 'Estafeta',
    numFactura: 'FAC-008-2024',
    productos: [
      { nombre: 'Impresora HP LaserJet', sku: 'HP-LJ-010', cantidad: 1, precioUnitario: 1400.00 },
    ],
  },
  {
    id: 'ORD-009',
    fecha: '2024-03-05',
    estado: 'Entregada',
    tipoPago: 'PayPal',
    total: 4280.00,
    subtotal: 3424.00,
    flete: 90.00,
    iva: 766.00,
    cliente: 'Javier Alejandro Morales Díaz',
    transportista: 'DHL Express',
    numFactura: 'FAC-009-2024',
    productos: [
      { nombre: 'Laptop Dell XPS 13', sku: 'DELL-XPS-011', cantidad: 1, precioUnitario: 3424.00 },
    ],
  },
  {
    id: 'ORD-010',
    fecha: '2024-03-10',
    estado: 'Procesando',
    tipoPago: 'Tarjeta de Crédito',
    total: 675.00,
    subtotal: 540.00,
    flete: 90.00,
    iva: 45.00,
    cliente: 'Carmen Rosa Ruiz Flores',
    transportista: 'FedEx',
    numFactura: 'FAC-010-2024',
    productos: [
      { nombre: 'Webcam HD Logitech C920', sku: 'LOG-WEB-012', cantidad: 1, precioUnitario: 540.00 },
    ],
  },
  {
    id: 'ORD-011',
    fecha: '2024-03-12',
    estado: 'Entregada',
    tipoPago: 'Tarjeta de Débito',
    total: 1250.00,
    subtotal: 1000.00,
    flete: 100.00,
    iva: 150.00,
    cliente: 'Diego Armando Torres Gómez',
    transportista: 'Estafeta',
    numFactura: 'FAC-011-2024',
    productos: [
      { nombre: 'Router WiFi TP-Link AX3000', sku: 'TPL-AX-013', cantidad: 2, precioUnitario: 500.00 },
    ],
  },
  {
    id: 'ORD-012',
    fecha: '2024-03-15',
    estado: 'Cancelada',
    tipoPago: 'Transferencia Bancaria',
    total: 890.00,
    subtotal: 712.00,
    flete: 90.00,
    iva: 88.00,
    cliente: 'Sofía Alejandra Ramírez Cruz',
    transportista: 'DHL Express',
    numFactura: 'FAC-012-2024',
    productos: [
      { nombre: 'Memoria USB 64GB Kingston', sku: 'KIN-USB-014', cantidad: 4, precioUnitario: 178.00 },
    ],
  },
  {
    id: 'ORD-013',
    fecha: '2024-03-18',
    estado: 'Procesando',
    tipoPago: 'Efectivo',
    total: 5300.00,
    subtotal: 4240.00,
    flete: 0,
    iva: 1060.00,
    cliente: 'Andrés Felipe Díaz Ortega',
    transportista: 'FedEx',
    numFactura: 'FAC-013-2024',
    productos: [
      { nombre: 'PlayStation 5', sku: 'SONY-PS5-015', cantidad: 1, precioUnitario: 4240.00 },
      { nombre: 'Control DualSense', sku: 'SONY-CTR-016', cantidad: 2, precioUnitario: 500.00 },
    ],
  },
  {
    id: 'ORD-014',
    fecha: '2024-03-20',
    estado: 'Entregada',
    tipoPago: 'PayPal',
    total: 890.00,
    subtotal: 712.00,
    flete: 90.00,
    iva: 88.00,
    cliente: 'Valentina Andrea Castro Méndez',
    transportista: 'Estafeta',
    numFactura: 'FAC-014-2024',
    productos: [
      { nombre: 'Silla Gamer Corsair', sku: 'COR-SIL-017', cantidad: 1, precioUnitario: 712.00 },
    ],
  },
  {
    id: 'ORD-015',
    fecha: '2024-03-22',
    estado: 'Procesando',
    tipoPago: 'Tarjeta de Crédito',
    total: 1250.00,
    subtotal: 1000.00,
    flete: 100.00,
    iva: 150.00,
    cliente: 'Fernando Javier Ortega Silva',
    transportista: 'DHL Express',
    numFactura: 'FAC-015-2024',
    productos: [
      { nombre: 'Micrófono Profesional Blue Yeti', sku: 'BLU-YET-018', cantidad: 1, precioUnitario: 1000.00 },
    ],
  },
  {
    id: 'ORD-016',
    fecha: '2024-03-25',
    estado: 'Entregada',
    tipoPago: 'Tarjeta de Débito',
    total: 3240.00,
    subtotal: 2592.00,
    flete: 90.00,
    iva: 558.00,
    cliente: 'Gabriela Elizabeth Navarro Ríos',
    transportista: 'FedEx',
    numFactura: 'FAC-016-2024',
    productos: [
      { nombre: 'Smart TV Samsung 55" 4K', sku: 'SAM-TV-019', cantidad: 1, precioUnitario: 2592.00 },
    ],
  },
  {
    id: 'ORD-017',
    fecha: '2024-03-28',
    estado: 'Cancelada',
    tipoPago: 'Transferencia Bancaria',
    total: 450.00,
    subtotal: 360.00,
    flete: 90.00,
    iva: 0,
    cliente: 'Ricardo Antonio Mendoza Jiménez',
    transportista: 'Estafeta',
    numFactura: 'FAC-017-2024',
    productos: [
      { nombre: 'Cable HDMI 2m', sku: 'HDMI-2M-020', cantidad: 3, precioUnitario: 60.00 },
      { nombre: 'Adaptador USB-C', sku: 'USBC-ADP-021', cantidad: 2, precioUnitario: 90.00 },
    ],
  },
  {
    id: 'ORD-018',
    fecha: '2024-04-01',
    estado: 'Procesando',
    tipoPago: 'Tarjeta de Crédito',
    total: 1890.00,
    subtotal: 1512.00,
    flete: 90.00,
    iva: 288.00,
    cliente: 'Paola Michelle Chávez Valencia',
    transportista: 'DHL Express',
    numFactura: 'FAC-018-2024',
    productos: [
      { nombre: 'Bocina Bluetooth JBL Flip 6', sku: 'JBL-FLIP-022', cantidad: 2, precioUnitario: 756.00 },
    ],
  },
  {
    id: 'ORD-019',
    fecha: '2024-04-05',
    estado: 'Entregada',
    tipoPago: 'PayPal',
    total: 4300.00,
    subtotal: 3440.00,
    flete: 90.00,
    iva: 770.00,
    cliente: 'Oscar Daniel Vega López',
    transportista: 'FedEx',
    numFactura: 'FAC-019-2024',
    productos: [
      { nombre: 'Nintendo Switch OLED', sku: 'NIN-SW-023', cantidad: 1, precioUnitario: 3440.00 },
      { nombre: 'The Legend of Zelda', sku: 'ZELDA-GAME-024', cantidad: 2, precioUnitario: 430.00 },
    ],
  },
  {
    id: 'ORD-020',
    fecha: '2024-04-08',
    estado: 'Procesando',
    tipoPago: 'Efectivo',
    total: 980.00,
    subtotal: 784.00,
    flete: 90.00,
    iva: 106.00,
    cliente: 'Daniela Fernanda Guzmán Castillo',
    transportista: 'Estafeta',
    numFactura: 'FAC-020-2024',
    productos: [
      { nombre: 'Cargador Rápido 65W', sku: 'CHG-65W-025', cantidad: 1, precioUnitario: 289.00 },
      { nombre: 'Funda para Laptop 15.6"', sku: 'FUN-LAP-026', cantidad: 1, precioUnitario: 195.00 },
      { nombre: 'Limpiapantallas', sku: 'CLN-SCR-027', cantidad: 2, precioUnitario: 150.00 },
    ],
  },
];  

  ngOnInit(): void {
    // Guardar copia original
    this.ordenesOriginales = [...this.ordenes];
    // Inicializar con todos los datos
    this.ordenesFiltradas = [...this.ordenes];
    // Calcular estadísticas
    this.calcularEstadisticas();
  }

  // Getters para paginación
  get totalPaginas(): number {
    return Math.ceil(this.ordenesFiltradas.length / this.filasPorPagina);
  }

  get totalItemsRegistros(): string {
    if (this.ordenesFiltradas.length === 0) return '0-0 de 0';
    const inicio = (this.paginaActual - 1) * this.filasPorPagina + 1;
    const fin = Math.min(this.paginaActual * this.filasPorPagina, this.ordenesFiltradas.length);
    return `${inicio}-${fin} de ${this.ordenesFiltradas.length}`;
  }

  get ordenesPaginadas(): any[] {
    const inicio = (this.paginaActual - 1) * this.filasPorPagina;
    return this.ordenesFiltradas.slice(inicio, inicio + this.filasPorPagina);
  }

  // ==================== FILTROS ====================

  buscarOrdenes() {
    this.paginaActual = 1;
    
    this.ordenesFiltradas = this.ordenesOriginales.filter((orden) => {
      const coincideId = this.filtroId
        ? orden.id.toLowerCase().includes(this.filtroId.toLowerCase())
        : true;

      const coincideEstatus = this.filtroEstatus
        ? orden.estado === this.filtroEstatus
        : true;

      const coincideFechaInicio = this.filtroFechaInicio
        ? orden.fecha >= this.filtroFechaInicio
        : true;

      const coincideFechaFin = this.filtroFechaFin
        ? orden.fecha <= this.filtroFechaFin
        : true;

      return coincideId && coincideEstatus && coincideFechaInicio && coincideFechaFin;
    });

    this.calcularEstadisticas();
  }

  limpiarFiltros() {
    this.filtroId = '';
    this.filtroEstatus = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.buscarOrdenes();
  }

  // ==================== ESTADÍSTICAS ====================

  calcularEstadisticas() {
    this.totalGastado = this.ordenesFiltradas.reduce(
      (sum, orden) => sum + orden.total,
      0
    );

    this.totalOrdenes = this.ordenesFiltradas.length;

    this.ordenesProcesando = this.ordenesFiltradas.filter(
      (orden) => orden.estado === 'Procesando'
    ).length;

    const limiteCredito = 50000;
    this.creditoDisponible = limiteCredito - this.totalGastado;
    
    if (this.creditoDisponible < 0) this.creditoDisponible = 0;
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

  // ==================== MODAL DETALLES ====================

  verDetalleOrden(orden: any) {
    this.ordenSeleccionada = orden;
    this.modalVisible = true;
  }

  cerrarModal() {
    this.modalVisible = false;
    this.ordenSeleccionada = null;
  }
}