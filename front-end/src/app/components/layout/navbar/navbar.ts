// front-end/src/app/components/layout/navbar/navbar.ts

import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SecureAuthService, User } from '../../../services/secure-auth.service';
import { CookieService } from '../../../services/cookie.service';
import { UiIconComponent } from "../../shared/icono/icono.component";
import { UiBoton } from '../../shared/boton/boton';
import { CartService, CartItem } from '../../../services/cart.service';

interface ProductoCarrito {
  id: string;
  nombre: string;
  precio: number;
  cantidad: number;
  productId: string;
  sku: string;
  totalPrice: number;
}

@Component({
  selector: 'ui-navbar-component',
  standalone: true,
  imports: [RouterModule, CommonModule, UiIconComponent, UiBoton],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class UiNavbarComponent implements OnInit, OnDestroy {
  private authService = inject(SecureAuthService);
  private cookieService = inject(CookieService);
  private cartService = inject(CartService);
  private router = inject(Router);

  dropOpciones: boolean = false;
  mostrarModalConfirmacion: boolean = false;
  mostrarSidebar: boolean = false;
  carritoAbierto: boolean = false;
  usuarioActual: User | null = null;
  private clickListener: any;
  isLoading: boolean = false;

  productosCarrito: ProductoCarrito[] = [];
  totalCarrito: number = 0;

  get cantidadCarrito(): number {
    return this.productosCarrito.reduce((total, item) => total + item.cantidad, 0);
  }

  ngOnInit() {
    this.cargarUsuarioActual();
    this.cargarCarrito();
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
    if (event.key === 'user' || event.key === 'token') {
      this.cargarCarrito();
    }
  }

  private onClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const userMenuWrapper = document.querySelector('.user-menu-wrapper');
    const cartWrapper = document.querySelector('.cart-wrapper');

    if (userMenuWrapper && !userMenuWrapper.contains(target) && this.dropOpciones) {
      this.dropOpciones = false;
    }

    if (cartWrapper && !cartWrapper.contains(target) && this.carritoAbierto) {
      this.carritoAbierto = false;
    }
  }

  cargarUsuarioActual() {
    this.usuarioActual = this.authService.getCurrentUser();
    console.log('👤 Usuario cargado en navbar:', this.usuarioActual);
  }

  async cargarCarrito() {
    if (!this.authService.isAuthenticated()) {
      console.log('🔓 Usuario no autenticado, carrito vacio');
      this.productosCarrito = [];
      this.totalCarrito = 0;
      return;
    }

    try {
      this.isLoading = true;
      console.log('🛒 Cargando carrito...');
      
      const response = await this.cartService.getCart();
      console.log('📦 Respuesta del carrito:', response);

      if (response.success && response.data) {
        const items = response.data.items || [];
        console.log('📋 Items del carrito (con productId):', items);

        if (items.length > 0) {
          this.productosCarrito = items.map((item: CartItem) => ({
            id: item.productId,
            productId: item.productId,
            sku: item.sku || '',
            nombre: item.name || 'Producto sin nombre',
            precio: item.price || 0,
            cantidad: item.quantity || 1,
            totalPrice: item.totalPrice || (item.price * item.quantity) || 0
          }));

          this.totalCarrito = response.data.total || response.data.subtotal || 0;

          console.log('✅ Productos mapeados con productId:', this.productosCarrito);
          console.log('💰 Total del carrito:', this.totalCarrito);
          console.log('📊 Cantidad de productos:', this.cantidadCarrito);
          
          this.productosCarrito.forEach(p => {
            console.log(`📦 ${p.nombre} -> productId: ${p.productId}, cantidad: ${p.cantidad}`);
          });
        } else {
          console.log('🛒 El carrito está vacío');
          this.productosCarrito = [];
          this.totalCarrito = 0;
        }
      } else {
        console.log('⚠️ La respuesta no tiene success: true o data');
        this.productosCarrito = [];
        this.totalCarrito = 0;
      }
    } catch (error) {
      console.error('❌ Error al cargar carrito:', error);
      this.productosCarrito = [];
      this.totalCarrito = 0;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * 🔥 Navegar a la página de detalle del producto
   * @param productId - ID del producto (referencia)
   */
  irADetalleProducto(productId: string): void {
    if (!productId) {
      console.warn('⚠️ No se puede navegar: productId no válido');
      return;
    }
    
    console.log(`🔍 Navegando a detalle del producto: ${productId}`);
    this.cerrarCarrito(); // Cerrar el carrito al navegar
    this.router.navigate(['/catalogo-producto/detalles-producto', productId]);
  }

  async eliminarDelCarrito(productId: string) {
    try {
      console.log(`🗑️ Eliminando producto con productId: ${productId}`);
      
      const item = this.productosCarrito.find(p => p.productId === productId);
      
      const response = await this.cartService.removeItem(productId);
      console.log('✅ Respuesta de eliminación:', response);

      this.productosCarrito = this.productosCarrito.filter(p => p.productId !== productId);
      
      this.totalCarrito = this.productosCarrito.reduce(
        (total, p) => total + (p.precio * p.cantidad), 
        0
      );
      
      console.log(`✅ Producto ${item?.nombre || productId} eliminado del carrito`);
      console.log('💰 Nuevo total:', this.totalCarrito);
      
      await this.cargarCarrito();
    } catch (error) {
      console.error('❌ Error al eliminar del carrito:', error);
      await this.cargarCarrito();
    }
  }

  comprarCarrito() {
    this.cerrarCarrito();
    this.router.navigate(['/orden-compra/comprar']);
  }

  toggleDropdown() {
    this.dropOpciones = !this.dropOpciones;
    if (this.dropOpciones) {
      this.carritoAbierto = false;
    }
  }

  cerrarDropdown() {
    this.dropOpciones = false;
  }

  toggleCarrito() {
    this.carritoAbierto = !this.carritoAbierto;
    if (this.carritoAbierto) {
      this.dropOpciones = false;
      this.cargarCarrito();
    }
  }

  cerrarCarrito() {
    this.carritoAbierto = false;
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
    this.carritoAbierto = false;
    this.mostrarModalConfirmacion = true;
    document.body.style.overflow = 'hidden';
  }

  cerrarModal() {
    this.mostrarModalConfirmacion = false;
    document.body.style.overflow = '';
  }

  confirmarCerrarSesion() {
    console.log('🚪 Confirmando cierre de sesión...');

    this.isLoading = true;

    this.authService.logout().subscribe({
      next: (response) => {
        console.log('✅ Sesión cerrada correctamente:', response.message);
        this.isLoading = false;
        this.cerrarModal();
        this.productosCarrito = [];
        this.totalCarrito = 0;
        this.router.navigate(['/inicio-sesion']);
      },
      error: (error) => {
        console.error('❌ Error al cerrar sesión:', error);
        this.isLoading = false;
        this.cerrarModal();
        this.productosCarrito = [];
        this.totalCarrito = 0;
        this.router.navigate(['/inicio-sesion']);
      }
    });
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