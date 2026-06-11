import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SecureAuthService, User } from '../../../services/secure-auth.service';
import { CookieService } from '../../../services/cookie.service';

@Component({
  selector: 'ui-navbar-component',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})

export class UiNavbarComponent implements OnInit, OnDestroy {
  private authService = inject(SecureAuthService);
  private cookieService = inject(CookieService);
  private router = inject(Router);
  
  dropOpciones: boolean = false;
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
  }

  private handleStorageChange(event: StorageEvent) {
    if (event.key === 'user') {
      this.cargarUsuarioActual();
    }
  }

  private onClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const userMenu = document.querySelector('.user-menu');
    
    if (userMenu && !userMenu.contains(target) && this.dropOpciones) {
      this.dropOpciones = false;
    }
  }

  cargarUsuarioActual() {
    this.usuarioActual = this.authService.getCurrentUser();
    console.log('👤 Usuario cargado en navbar:', this.usuarioActual);
  }

  toggleDropdown(event?: MouseEvent) {
    if (event) {
      event.stopPropagation();
    }
    this.dropOpciones = !this.dropOpciones;
  }

  cerrarSesion() {
    console.log('🔒 Cerrando sesión...');
    this.authService.logout();
    this.dropOpciones = false;
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
}