import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SecureAuthService, User } from '../../../services/secure-auth.service';
import { CookieService } from '../../../services/cookie.service';
import { UiIconComponent } from "../../shared/icono/icono.component";

@Component({
  selector: 'ui-navbar-component',
  standalone: true,
  imports: [RouterModule, CommonModule, UiIconComponent],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})

export class UiNavbarComponent implements OnInit, OnDestroy {
  private authService = inject(SecureAuthService);
  private cookieService = inject(CookieService);
  private router = inject(Router);
  
  dropOpciones: boolean = false;
  mostrarModalConfirmacion: boolean = false;
  mostrarSidebar: boolean = false;
  usuarioActual: User | null = null;
  private clickListener: any;

  ngOnInit() {
    this.cargarUsuarioActual();
    window.addEventListener('storage', this.handleStorageChange.bind(this));
    this.clickListener = this.onClickOutside.bind(this);
    document.addEventListener('click', this.clickListener);
  }

  ngOnDestroy() {
    window.removeEventListener('storage', this.handleStorageChange.bind(this));
    if (this.clickListener) {
      document.removeEventListener('click', this.clickListener);
    }
    document.body.style.overflow = '';
  }

  private handleStorageChange(event: StorageEvent) {
    if (event.key === 'user') {
      this.cargarUsuarioActual();
    }
  }

  private onClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const userMenuWrapper = document.querySelector('.relative.inline-block');
    
    if (userMenuWrapper && !userMenuWrapper.contains(target) && this.dropOpciones) {
      this.dropOpciones = false;
    }
  }

  cargarUsuarioActual() {
    this.usuarioActual = this.authService.getCurrentUser();
    console.log('👤 Usuario cargado en navbar:', this.usuarioActual);
  }

  toggleDropdown() {
    this.dropOpciones = !this.dropOpciones;
  }

  cerrarDropdown() {
    this.dropOpciones = false;
  }

  toggleSidebar() {
    this.mostrarSidebar = !this.mostrarSidebar;
    if (this.mostrarSidebar) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  cerrarSidebar() {
    this.mostrarSidebar = false;
    document.body.style.overflow = '';
  }

  abrirModalConfirmacion() {
    this.dropOpciones = false;
    this.mostrarSidebar = false;
    this.mostrarModalConfirmacion = true;
    document.body.style.overflow = 'hidden';
  }

  cerrarModal() {
    this.mostrarModalConfirmacion = false;
    document.body.style.overflow = '';
  }

  confirmarCerrarSesion() {
    console.log('🔒 Cerrando sesión...');
    this.authService.logout();
    this.cerrarModal();
    this.router.navigate(['/inicio-sesion']);
  }

  getInitials(): string {
    if (!this.usuarioActual) return 'U';
    
    const nombre = this.usuarioActual.nombre || '';
    const apellido = this.usuarioActual.apellido || '';
    
    if (nombre && apellido) {
      return `${nombre.charAt(0)}${apellido.charAt(0)}`.toUpperCase();
    }
    if (nombre) {
      return nombre.charAt(0).toUpperCase();
    }
    if (this.usuarioActual.email) {
      return this.usuarioActual.email.charAt(0).toUpperCase();
    }
    
    return 'U';
  }

  getNombreCompleto(): string {
    if (!this.usuarioActual) return 'Usuario';
    
    const nombre = this.usuarioActual.nombre || '';
    const apellido = this.usuarioActual.apellido || '';
    
    if (nombre && apellido) {
      return `${nombre} ${apellido}`;
    }
    if (nombre) {
      return nombre;
    }
    if (this.usuarioActual.email) {
      return this.usuarioActual.email.split('@')[0];
    }
    
    return 'Usuario';
  }

  getRolUsuario(): string {
    if (!this.usuarioActual) return 'Sin rol';
    
    const rol = this.usuarioActual.rol || '';
    return rol.charAt(0).toUpperCase() + rol.slice(1).toLowerCase();
  }
}