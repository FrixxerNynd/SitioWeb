import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Inputs } from '../../../../../components/shared/inputs/inputs';
import {
  ExcelNorteCatalogoService,
  Brand,
  Category,
  IProduct,
} from '../../../../../services/exel-api-base.service';

@Component({
  selector: 'app-lista-orden',
  standalone: true,
  imports: [CommonModule, FormsModule, Inputs],
  templateUrl: './lista-orden.html',
  styleUrl: './lista-orden.css',
})
export class ListaOrdenPague {
  filtroId: string = '';
  filtroEstatus: string = '';
  filtroFechaInicio: string = '';
  filtroFechaFin: string = '';

  totalGastado: number = 0;
  creditoDisponible: number = 0;
  totalOrdenes: number = 0;
  ordenesProcesando: number = 0;

  totalItemsRegistros: string = '1-7 de 100';
  paginaActual: string = '1/10';

  opcionesEstatus = [
    { label: 'Pendientes', value: 'Procesando' },
    { label: 'Entregadas', value: 'Entregada' },
    { label: 'Canceladas', value: 'Cancelada' },
  ];

  ordenes: any[] = [];
  ordenSeleccionada: any = null;

  buscarOrdenes() {
    console.log('Filtrando por:', {
      id: this.filtroId,
      estatus: this.filtroEstatus,
      inicio: this.filtroFechaInicio,
      fin: this.filtroFechaFin,
    });
  }

  limpiarFiltros() {
    this.filtroId = '';
    this.filtroEstatus = '';
    this.filtroFechaInicio = '';
    this.filtroFechaFin = '';
    this.buscarOrdenes();
  }

  seleccionarOrden(orden: any) {
    this.ordenSeleccionada = orden;
  }
}
