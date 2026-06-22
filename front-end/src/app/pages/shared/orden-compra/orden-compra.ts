// front-end/src/app/pages/shared/orden-compra/orden-compra.ts

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { UiBoton } from "../../../components/shared/boton/boton";
import { AuthLayout } from "../../../layouts/auth-layout/auth-layout";
import { UiIconComponent } from "../../../components/shared/icono/icono.component";
import { CartService, CartItem, PaymentType } from '../../../services/cart.service';

interface ProductoCarrito {
  id: string;
  nombre: string;
  precio: number;
  cantidad: number;
  subtotal: number;
  productId: string;
  sku: string;
}

interface Direccion {
  id: number;
  nombre: string;
  street: string;
  extNum: string;
  intNum: string;
  neighborhood: string;
  city: string;
  state: string;
  cp: string;
  country: string;
  esPrincipal: boolean;
}

@Component({
  selector: 'app-orden-compra',
  standalone: true,
  imports: [CommonModule, FormsModule, UiBoton, AuthLayout, UiIconComponent],
  templateUrl: './orden-compra.html',
  styleUrl: './orden-compra.css',
})
export class PageOrdenCompra implements OnInit {
  private cartService = inject(CartService);
  private router = inject(Router);

  pasoActual: number = 1;
  tipoEntrega: 'domicilio' | 'sucursal' = 'domicilio';

  estadoCambiarTipoPago: boolean = false;
  tipoPagoTemp: PaymentType = 'Transferencia';
  isLoading: boolean = false;
  errorMessage: string = '';

  productosCarrito: ProductoCarrito[] = [];
  totalCarrito: number = 0;
  subtotalCarrito: number = 0;
  fleteCarrito: number = 0;

  direcciones: Direccion[] = [];
  direccionSeleccionada: Direccion | null = null;

  tiposPago: PaymentType[] = ['Efectivo', 'Transferencia', 'Crédito'];
  tipoPagoSeleccionado: PaymentType = 'Transferencia';

  mostrarFormularioDireccion: boolean = false;
  nuevaDireccion = {
    pais: 'México',
    estado: '',
    ciudad: '',
    codigoPostal: '',
    colonia: '',
    calle: '',
    nExterior: '',
    nInterior: ''
  };
  guardandoDireccion: boolean = false;

  mostrarFormularioEdicion: boolean = false;
  direccionEnEdicion: Direccion | null = null;
  direccionEditada = {
    pais: 'México',
    estado: '',
    ciudad: '',
    codigoPostal: '',
    colonia: '',
    calle: '',
    nExterior: '',
    nInterior: ''
  };
  guardandoEdicion: boolean = false;

  sucursal = {
    nombre: 'Sucursal Centro',
    calle: 'Beatriz Prada',
    numero: '450',
    colonia: 'Col. Jardines de Durango',
    ciudad: 'Victoria de Durango',
    estado: 'Durango',
    codigoPostal: '34020',
    pais: 'México',
    horario: 'Lunes a Viernes 9:00 AM - 6:00 PM, Sábado 9:00 AM - 2:00 PM y domingo estamos cerrados.'
  };

  ngOnInit(): void {
    this.cargarDatos();
  }

  async cargarDatos() {
    this.isLoading = true;
    this.errorMessage = '';

    try {
      const cartResponse = await this.cartService.getCart();

      if (cartResponse.success && cartResponse.data) {
        const data = cartResponse.data;
        const productosRaw = data.productos || [];

        if (productosRaw.length > 0) {
          this.productosCarrito = productosRaw.map((item: string, index: number) => {
            const match = item.match(/^(.*?)\s*x(\d+)$/);
            let nombre = item;
            let cantidad = 1;

            if (match) {
              nombre = match[1].trim();
              cantidad = parseInt(match[2], 10);
            }

            let precio = 0;
            let productId = `item-${index}`;

            if (data.items && data.items.length > index) {
              const itemDetail = data.items[index];
              if (itemDetail) {
                productId = itemDetail.productId || productId;
                precio = itemDetail.price || 0;
              }
            }

            return {
              id: productId,
              productId: productId,
              sku: '',
              nombre: nombre,
              precio: precio,
              cantidad: cantidad,
              subtotal: precio * cantidad
            };
          });
        } else {
          this.productosCarrito = [];
        }

        this.subtotalCarrito = data.subtotal || 0;
        this.fleteCarrito = data.flete || 0;
        this.totalCarrito = data.total || 0;

        // ✅ ACTUALIZAR TIPO DE PAGO DESDE EL CARRITO
        if (data.paymentType) {
          console.log('💳 Tipo de pago desde el carrito:', data.paymentType);
          this.tipoPagoSeleccionado = data.paymentType as PaymentType;
          this.tipoPagoTemp = this.tipoPagoSeleccionado;
        }
      }

      try {
        const addressesResponse = await this.cartService.getAddresses();

        if (addressesResponse?.success && addressesResponse?.data) {
          if (addressesResponse.data.length === 0) {
            this.direcciones = [];
          } else {
            this.direcciones = addressesResponse.data.map((addr: any) => ({
              id: addr.id,
              nombre: addr.nombre || `Dirección ${addr.id}`,
              street: addr.street || '',
              extNum: addr.extNum || '',
              intNum: addr.intNum || '',
              neighborhood: addr.neighborhood || '',
              city: addr.city || '',
              state: addr.state || '',
              cp: addr.cp || '',
              country: addr.country || 'México',
              esPrincipal: addr.esPrincipal || false
            }));

            const principal = this.direcciones.find(d => d.esPrincipal);
            this.direccionSeleccionada = principal || this.direcciones[0] || null;
          }
        } else {
          this.direcciones = [];
        }
      } catch (addressError: any) {
        this.direcciones = [];
      }

    } catch (error) {
      this.errorMessage = 'Error al cargar los datos del carrito. Intenta de nuevo.';
    } finally {
      this.isLoading = false;
    }
  }

  abrirFormularioDireccion() {
    this.nuevaDireccion = {
      pais: 'México',
      estado: '',
      ciudad: '',
      codigoPostal: '',
      colonia: '',
      calle: '',
      nExterior: '',
      nInterior: ''
    };
    this.mostrarFormularioDireccion = true;
  }

  cerrarFormularioDireccion() {
    this.mostrarFormularioDireccion = false;
    this.nuevaDireccion = {
      pais: 'México',
      estado: '',
      ciudad: '',
      codigoPostal: '',
      colonia: '',
      calle: '',
      nExterior: '',
      nInterior: ''
    };
  }

  async guardarNuevaDireccion() {
    const { estado, ciudad, codigoPostal, colonia, calle, nExterior } = this.nuevaDireccion;
    
    if (!estado || !ciudad || !codigoPostal || !colonia || !calle || !nExterior) {
      alert('Por favor completa todos los campos obligatorios');
      return;
    }

    this.guardandoDireccion = true;

    try {
      const addressData = {
        country: this.nuevaDireccion.pais,
        state: this.nuevaDireccion.estado,
        city: this.nuevaDireccion.ciudad,
        cp: this.nuevaDireccion.codigoPostal,
        neighborhood: this.nuevaDireccion.colonia,
        street: this.nuevaDireccion.calle,
        extNum: this.nuevaDireccion.nExterior,
        intNum: this.nuevaDireccion.nInterior || ''
      };
      
      console.log('📍 Enviando nueva dirección:', addressData);
      
      const response = await this.cartService.addAddress(addressData);
      console.log('✅ Dirección guardada:', response);

      if (response.success) {
        alert('Dirección registrada exitosamente');
        this.cerrarFormularioDireccion();
        await this.cargarDatos();
        if (this.pasoActual === 2 && this.direcciones.length > 0) {
          this.direccionSeleccionada = this.direcciones[this.direcciones.length - 1];
        }
      } else {
        alert('Error al guardar la dirección: ' + (response.message || 'Intenta de nuevo'));
      }
    } catch (error: any) {
      console.error('Error al guardar dirección:', error);
      const mensaje = error?.error?.message || error?.message || 'Error al guardar la dirección';
      alert('Error al guardar la dirección: ' + mensaje);
    } finally {
      this.guardandoDireccion = false;
    }
  }

  editarDireccion(direccion: Direccion) {
    this.direccionEditada = {
      pais: direccion.country || 'México',
      estado: direccion.state || '',
      ciudad: direccion.city || '',
      codigoPostal: direccion.cp || '',
      colonia: direccion.neighborhood || '',
      calle: direccion.street || '',
      nExterior: direccion.extNum || '',
      nInterior: direccion.intNum || ''
    };
    this.direccionEnEdicion = direccion;
    this.mostrarFormularioEdicion = true;
  }

  cerrarFormularioEdicion() {
    this.mostrarFormularioEdicion = false;
    this.direccionEnEdicion = null;
    this.direccionEditada = {
      pais: 'México',
      estado: '',
      ciudad: '',
      codigoPostal: '',
      colonia: '',
      calle: '',
      nExterior: '',
      nInterior: ''
    };
  }

  async guardarEdicionDireccion() {
    const { estado, ciudad, codigoPostal, colonia, calle, nExterior } = this.direccionEditada;
    
    if (!estado || !ciudad || !codigoPostal || !colonia || !calle || !nExterior) {
      alert('Por favor completa todos los campos obligatorios');
      return;
    }

    if (!this.direccionEnEdicion) {
      alert('No hay una dirección seleccionada para editar');
      return;
    }

    this.guardandoEdicion = true;

    try {
      const addressData = {
        country: this.direccionEditada.pais,
        state: this.direccionEditada.estado,
        city: this.direccionEditada.ciudad,
        cp: this.direccionEditada.codigoPostal,
        neighborhood: this.direccionEditada.colonia,
        street: this.direccionEditada.calle,
        extNum: this.direccionEditada.nExterior,
        intNum: this.direccionEditada.nInterior || ''
      };
      
      console.log('📍 Actualizando dirección ID:', this.direccionEnEdicion.id, addressData);
      
      const response = await this.cartService.updateAddress(
        this.direccionEnEdicion.id,
        addressData
      );
      console.log('✅ Dirección actualizada:', response);

      if (response.success) {
        alert('Dirección actualizada exitosamente');
        this.cerrarFormularioEdicion();
        await this.cargarDatos();
      } else {
        alert('Error al actualizar la dirección: ' + (response.message || 'Intenta de nuevo'));
      }
    } catch (error: any) {
      console.error('Error al actualizar dirección:', error);
      const mensaje = error?.error?.message || error?.message || 'Error al actualizar la dirección';
      alert('Error al actualizar la dirección: ' + mensaje);
    } finally {
      this.guardandoEdicion = false;
    }
  }

  async eliminarDireccion(direccion: Direccion) {
    if (!confirm(`¿Estás seguro de que deseas eliminar la dirección "${direccion.nombre}"?`)) {
      return;
    }

    this.isLoading = true;

    try {
      console.log('📍 Eliminando dirección ID:', direccion.id);
      
      const response = await this.cartService.deleteAddress(direccion.id);
      console.log('✅ Dirección eliminada:', response);

      if (response.success) {
        alert('Dirección eliminada exitosamente');
        this.direcciones = this.direcciones.filter(d => d.id !== direccion.id);
        
        if (this.direccionSeleccionada?.id === direccion.id) {
          this.direccionSeleccionada = this.direcciones.length > 0 ? this.direcciones[0] : null;
        }
        
        if (this.mostrarFormularioEdicion && this.direccionEnEdicion?.id === direccion.id) {
          this.cerrarFormularioEdicion();
        }
      } else {
        alert('Error al eliminar la dirección: ' + (response.message || 'Intenta de nuevo'));
      }
    } catch (error: any) {
      console.error('Error al eliminar dirección:', error);
      const mensaje = error?.error?.message || error?.message || 'Error al eliminar la dirección';
      alert('Error al eliminar la dirección: ' + mensaje);
    } finally {
      this.isLoading = false;
    }
  }

  get totalProductos(): number {
    return this.productosCarrito.reduce((sum, p) => sum + p.cantidad, 0);
  }

  get subtotal(): number {
    return this.subtotalCarrito;
  }

  get total(): number {
    return this.totalCarrito;
  }

  get flete(): number {
    return this.fleteCarrito;
  }

  actualizarCantidad(producto: ProductoCarrito, nuevaCantidad: number) {
    if (nuevaCantidad < 1) return;
    producto.cantidad = nuevaCantidad;
    producto.subtotal = producto.precio * nuevaCantidad;
    this.recalcularTotales();
  }

  recalcularTotales() {
    this.subtotalCarrito = this.productosCarrito.reduce((sum, p) => sum + p.subtotal, 0);
    this.totalCarrito = this.subtotalCarrito + this.fleteCarrito;
  }

  async eliminarProducto(productId: string) {
    try {
      await this.cartService.removeItem(productId);
      this.productosCarrito = this.productosCarrito.filter(p => p.productId !== productId);
      this.recalcularTotales();
      await this.cargarDatos();
    } catch (error) {
      alert('Error al eliminar el producto del carrito');
    }
  }

  cambiarTipoPago(tipo: PaymentType) {
    this.tipoPagoSeleccionado = tipo;
  }

  async guardarTipoPago() {
    try {
      this.isLoading = true;
      console.log('💳 Guardando tipo de pago:', this.tipoPagoSeleccionado);
      
      const response = await this.cartService.setPaymentType(this.tipoPagoSeleccionado);
      console.log('✅ Respuesta del servidor:', response);
      
      this.estadoCambiarTipoPago = false;
      this.tipoPagoTemp = this.tipoPagoSeleccionado;
      
      // ✅ Recargar datos para actualizar el carrito
      await this.cargarDatos();
      
      // ✅ Mostrar el tipo de pago actualizado
      console.log('💳 Tipo de pago actualizado a:', this.tipoPagoSeleccionado);
      
      alert('Tipo de pago actualizado correctamente');
    } catch (error: any) {
      console.error('❌ Error al guardar tipo de pago:', error);
      const mensaje = error?.error?.message || error?.message || 'Error al actualizar el tipo de pago';
      alert('Error al actualizar el tipo de pago: ' + mensaje);
    } finally {
      this.isLoading = false;
    }
  }

  cambiarTipoEntrega(tipo: 'domicilio' | 'sucursal') {
    this.tipoEntrega = tipo;
    if (tipo === 'domicilio' && this.direcciones.length === 0) {
      this.cargarDatos();
    }
  }

  seleccionarDireccion(direccion: Direccion) {
    this.direccionSeleccionada = direccion;
  }

  irAlSiguientePaso() {
    if (this.pasoActual === 1) {
      if (this.productosCarrito.length === 0) {
        alert('No hay productos en el carrito');
        return;
      }
      this.pasoActual = 2;
    }
  }

  irAlPasoAnterior() {
    if (this.pasoActual === 2) {
      this.pasoActual = 1;
    }
  }

  async confirmarPedido() {
    if (this.productosCarrito.length === 0) {
      alert('No hay productos en el carrito');
      return;
    }

    if (this.tipoEntrega === 'domicilio' && !this.direccionSeleccionada) {
      alert('Por favor selecciona una dirección de entrega');
      return;
    }

    try {
      this.isLoading = true;
      this.errorMessage = '';

      // ✅ Guardar tipo de pago antes de confirmar
      console.log('💳 Confirmando pedido con tipo de pago:', this.tipoPagoSeleccionado);
      await this.cartService.setPaymentType(this.tipoPagoSeleccionado);

      const addressId = this.tipoEntrega === 'domicilio' ? this.direccionSeleccionada?.id : undefined;
      await this.cartService.setDeliveryMethod(
        this.tipoEntrega === 'domicilio' ? 'Domicilio' : 'Sucursal',
        addressId
      );
      console.log('✅ Método de entrega guardado:', this.tipoEntrega);

      const result = await this.cartService.checkout();
      console.log('✅ Checkout completado:', result);

      if (result.success) {
        alert('Pedido confirmado con éxito');
        this.router.navigate(['/ordenes/lista-ordenes']);
      } else {
        this.errorMessage = result.message || 'Error al procesar el pedido';
        alert(this.errorMessage);
      }
    } catch (error: any) {
      console.error('❌ Error al confirmar pedido:', error);
      this.errorMessage = error.error?.message || error?.message || 'Error al procesar el pedido. Intenta de nuevo.';
      alert(this.errorMessage);
    } finally {
      this.isLoading = false;
    }
  }

  agregarDireccion() {
    this.abrirFormularioDireccion();
  }

  cambiarTipoPagoNuevo() {
    this.estadoCambiarTipoPago = true;
    this.tipoPagoTemp = this.tipoPagoSeleccionado;
  }

  cancelarCambioTipoPago() {
    this.estadoCambiarTipoPago = false;
    // ✅ Restaurar el valor original
    this.tipoPagoSeleccionado = this.tipoPagoTemp;
  }

  guardarCambioTipoPago() {
    this.guardarTipoPago();
  }
}