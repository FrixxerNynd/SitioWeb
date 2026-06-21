import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../../components/shared/icono/icono.component';
import { ModalDetallesOrdenComponent } from './detalles-orden/detalles-orden'; 
import { OrdenService } from '../../../../../services/orden.service';
import { Order } from '../../../../../interfaces/order.interface';

const MAPPED_STATUS: Record<string, string> = {
  PAGADO_PENDIENTE_SURTIDO: 'Procesando',
  EN_PREPARACION: 'Procesando',
  EN_CAMINO: 'Procesando',
  ENTREGADO: 'Entregada',
  CANCELADO: 'Cancelada',
};

const mapOrder = (o: Order) => ({
  id: `ORD-${String(o.id).padStart(3, '0')}`,
  fecha: o.fechaPedido.slice(0, 10),
  estado: MAPPED_STATUS[o.estado] ?? o.estado,
  tipoPago: o.metodoPago,
  total: o.total,
  subtotal: o.subtotal,
  flete: o.flete,
  iva: 0,
  transportista: o.metodoEntrega,
  numFactura: '',
  productos: (o.productos ?? []).map((p) => ({
    nombre: p.nombre,
    sku: p.sku,
    cantidad: p.cantidad,
    precioUnitario: p.precio,
  })),
});

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent, ModalDetallesOrdenComponent],
  styleUrl: './lista-orden.css',
  templateUrl: './lista-orden.html',
})
export class ListaOrdenPague implements OnInit {
  private ordenService = inject(OrdenService);

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

  // Estado skeleton
  loadingData: boolean = true;

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

  ordenesOriginales: any[] = [];

  ngOnInit(): void {
    this.cargarOrdenes();
  }

  async cargarOrdenes(): Promise<void> {
    this.loadingData = true;
    try {
      const data = await this.ordenService.getAll();
      this.ordenesOriginales = data.map(mapOrder);
      this.ordenesFiltradas = [...this.ordenesOriginales];
      this.calcularEstadisticas();
    } catch (error) {
      console.error('Error al cargar órdenes:', error);
    } finally {
      this.loadingData = false;
    }
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