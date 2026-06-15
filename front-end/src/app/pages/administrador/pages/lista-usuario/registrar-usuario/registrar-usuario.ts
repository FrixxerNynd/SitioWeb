import { Component, Input, Output, EventEmitter, signal, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UiBoton } from '../../../../../components/shared/boton/boton';
import { UiIconComponent } from '../../../../../components/shared/icono/icono.component';

@Component({
  selector: 'modal-registrar-usuario',
  standalone: true,
  imports: [CommonModule, FormsModule, UiBoton, UiIconComponent],
  templateUrl: './registrar-usuario.html',
  styleUrl: './registrar-usuario.css'
})
export class ModalRegistrarUsuarioComponent {
  @Input() visible: boolean = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() cerrar = new EventEmitter<void>();
  @Output() usuarioRegistrado = new EventEmitter<any>();

  mostrarEsqueleto = signal(false);
  errorDeConexion = signal(false);

  // Modelo del nuevo usuario
  nuevoUsuario = {
    nombre: '',
    apellidoPaterno: '',
    apellidoMaterno: '',
    correo: '',
    telefonoFijo: '',
    telefonoWhatsApp: '',
    password: ''
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['visible'] && this.visible === true) {
      this.cargarDatos();
    }
  }

  cargarDatos() {
    this.mostrarEsqueleto.set(true);
    this.errorDeConexion.set(false);
    // Limpiar formulario al abrir
    this.limpiarFormulario();
    setTimeout(() => {
      this.mostrarEsqueleto.set(false);
    }, 500);
  }

  limpiarFormulario() {
    this.nuevoUsuario = {
      nombre: '',
      apellidoPaterno: '',
      apellidoMaterno: '',
      correo: '',
      telefonoFijo: '',
      telefonoWhatsApp: '',
      password: ''
    };
  }

  registrarUsuario() {
    // Validar campos requeridos
    if (!this.nuevoUsuario.nombre || !this.nuevoUsuario.apellidoPaterno || 
        !this.nuevoUsuario.correo || !this.nuevoUsuario.password) {
      alert('Por favor complete los campos requeridos (*)');
      return;
    }

    // Validar formato de email
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.nuevoUsuario.correo)) {
      alert('Por favor ingrese un correo electrónico válido');
      return;
    }

    // Validar longitud de contraseña
    if (this.nuevoUsuario.password.length < 6) {
      alert('La contraseña debe tener al menos 6 caracteres');
      return;
    }

    // Emitir el usuario registrado
    this.usuarioRegistrado.emit(this.nuevoUsuario);
    
    // Cerrar el modal
    this.cerrarPanel();
  }

  cerrarPanel() {
    this.visibleChange.emit(false);
    this.cerrar.emit();
  }
}