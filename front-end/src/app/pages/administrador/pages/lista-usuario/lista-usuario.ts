import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from '../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../components/shared/icono/icono.component';
import { ModalDetallesUsuarioComponent } from './detalles-usuario/detalles-usuario';
import { ModalRegistrarUsuarioComponent } from './registrar-usuario/registrar-usuario';
import { UserService } from '../../../../services/user.service';
import { Usuario } from '../../../../interfaces/user.interfaces';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent, ModalDetallesUsuarioComponent, ModalRegistrarUsuarioComponent],
  styleUrl: './lista-usuario.css',
  templateUrl: './lista-usuario.html',
})
export class PagueListaUsuario implements OnInit {
  private userService = inject(UserService);

  // Filtros
  filtroTexto: string = '';
  filtroEstatus: string = '';

  // Paginación
  filasPorPagina: number = 7;
  paginaActual: number = 1;
  opcionesFilasPorPagina: number[] = [7, 10, 25, 50];

  // Estado skeleton
  loadingData: boolean = true;

  // Datos
  usuariosFiltrados: Usuario[] = [];
  
  // Modal detalles
  usuarioSeleccionado: Usuario | null = null;
  modalVisible: boolean = false;

  // Modal registro
  modalRegistroVisible: boolean = false;

  usuariosOriginales: Usuario[] = [];

  ngOnInit(): void {
    this.cargarDatosConSkeleton();
  }

  cargarDatosConSkeleton(): void {
    this.loadingData = true;
    
    this.userService.obtenerUsuarios(true).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.usuariosOriginales = response.data;
          this.usuariosFiltrados = [...this.usuariosOriginales];
        }
        this.loadingData = false;
      },
      error: (err) => {
        console.error('Error al obtener usuarios', err);
        this.loadingData = false;
      }
    });
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

  get usuariosPaginados(): Usuario[] {
    const inicio = (this.paginaActual - 1) * this.filasPorPagina;
    return this.usuariosFiltrados.slice(inicio, inicio + this.filasPorPagina);
  }

  buscarUsuarios() {
    this.paginaActual = 1;
    
    this.usuariosFiltrados = this.usuariosOriginales.filter((usuario) => {
      const textoLower = this.filtroTexto.toLowerCase();
      const coincideTexto = this.filtroTexto
        ? (usuario.nombre?.toLowerCase().includes(textoLower) || false) ||
          (usuario.apellido?.toLowerCase().includes(textoLower) || false) ||
          (usuario.email?.toLowerCase().includes(textoLower) || false)
        : true;

      const coincideEstatus = this.filtroEstatus !== ''
        ? usuario.activo.toString() === this.filtroEstatus
        : true;

      return coincideTexto && coincideEstatus;
    });
  }

  limpiarFiltros() {
    this.filtroTexto = '';
    this.filtroEstatus = '';
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

  verDetalleUsuario(usuario: Usuario) {
    this.usuarioSeleccionado = usuario;
    this.modalVisible = true;
  }

  cerrarModal() {
    this.modalVisible = false;
    this.usuarioSeleccionado = null;
  }

  abrirModalRegistro() {
    this.modalRegistroVisible = true;
  }

  onUsuarioRegistrado(usuario: Usuario) {
    console.log('Usuario registrado:', usuario);
    // Recargar la lista después de crear un usuario
    this.cargarDatosConSkeleton();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-MX', {
      style: 'currency',
      currency: 'MXN',
      minimumFractionDigits: 2
    }).format(value);
  }
}