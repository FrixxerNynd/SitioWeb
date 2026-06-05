import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Inputs } from '../../../../../components/shared/inputs/inputs';

@Component({
  selector: 'app-lista-orden',
  standalone: true,
  imports: [CommonModule, FormsModule, Inputs],
  templateUrl: './lista-orden.html',
  styleUrl: './lista-orden.css',
})
export class ListaOrdenPague implements OnInit {
  filtroId: string = '';
  filtroEstatus: string = '';
  filtroFechaInicio: string = '';
  filtroFechaFin: string = '';

  totalGastado: number = 0;
  creditoDisponible: number = 0;
  totalOrdenes: number = 0;
  ordenesProcesando: number = 0;

  filasPorPagina: number = 7;
  paginaActual: number = 1;
  opcionesFilasPorPagina: number[] = [7, 10, 25, 50];

  ordenesFiltradas: any[] = [];

  get totalPaginas(): number {
    return Math.ceil(this.ordenesFiltradas.length / this.filasPorPagina);
  }

  get totalItemsRegistros(): string {
    const inicio = (this.paginaActual - 1) * this.filasPorPagina + 1;
    const fin = Math.min(this.paginaActual * this.filasPorPagina, this.ordenesFiltradas.length);
    return `${inicio}-${fin} de ${this.ordenesFiltradas.length}`;
  }

  get ordenesPaginadas(): any[] {
    const inicio = (this.paginaActual - 1) * this.filasPorPagina;
    return this.ordenesFiltradas.slice(inicio, inicio + this.filasPorPagina);
  }

  opcionesEstatus = [
    { label: 'Pendientes', value: 'Procesando' },
    { label: 'Entregadas', value: 'Entregada' },
    { label: 'Canceladas', value: 'Cancelada' },
  ];

  ordenes: any[] = [
    {
      id: 'ORD-001',
      fecha: '2024-01-15',
      estado: 'Entregada',
      tipoPago: 'Crédito',
      total: 1250.0,
      subtotal: 1000.0,
      flete: 100.0,
      iva: 150.0,
      cliente: 'Juan Pérez',
      transportista: 'DHL',
      numFactura: 'FAC-001',
      productos: [
        {
          nombre: 'Producto A',
          sku: 'SKU-001',
          cantidad: 2,
          precioUnitario: 500.0,
          imagen: 'https://placehold.co/48x48',
        },
        {
          nombre: 'Producto B',
          sku: 'SKU-002',
          cantidad: 1,
          precioUnitario: 250.0,
          imagen: 'https://placehold.co/48x48',
        },
      ],
    },
  ];

  ordenSeleccionada: any = null;

  ngOnInit(): void {
    this.ordenesFiltradas = [...this.ordenes];
  }

  seleccionarOrden(orden: any) {
    this.ordenSeleccionada = orden;
  }

  paginaAnterior() {
    if (this.paginaActual > 1) this.paginaActual--;
  }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) this.paginaActual++;
  }

  cambiarFilasPorPagina(event: Event) {
    this.filasPorPagina = Number((event.target as HTMLSelectElement).value);
    this.paginaActual = 1;
  }

  buscarOrdenes() {
    this.paginaActual = 1;
    this.ordenesFiltradas = this.ordenes.filter((orden) => {
      const coincideId = this.filtroId
        ? orden.id.toLowerCase().includes(this.filtroId.toLowerCase())
        : true;

      const coincideEstatus = this.filtroEstatus ? orden.estado === this.filtroEstatus : true;

      const coincideFechaInicio = this.filtroFechaInicio
        ? orden.fecha >= this.filtroFechaInicio
        : true;

      const coincideFechaFin = this.filtroFechaFin ? orden.fecha <= this.filtroFechaFin : true;

      return coincideId && coincideEstatus && coincideFechaInicio && coincideFechaFin;
    });
  }

  limpiarFiltros() {
    this.filtroId = '';
    this.filtroEstatus = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.buscarOrdenes();
  }
}
