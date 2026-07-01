import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DireccionDto {
  calle: string;
  numeroExterior: string;
  numeroInterior?: string;
  colonia: string;
  codigoPostal: string;
  ciudad: string;
  municipio?: string;
  estado: string;
  pais: string;
  telefono1?: string;
}

export interface PreRegistroState {
  RFC?: string;
  CURP?: string;
  nombre?: string;
  apellidoPaterno?: string;
  apellidoMaterno?: string;
  telefono?: string;
  whatsapp?: string;
  email?: string;
  contrasena?: string;
  recaptchaToken?: string;
  direccion?: DireccionDto;
}

export interface RegistroClienteResponse {
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class PreRegistroService {
  private http = inject(HttpClient);
  private readonly STORAGE_KEY = 'preRegistro';
  private readonly apiUrl = environment.apiCabsUrl;
  private archivoConstancia: File | null = null;

  /** Guarda el archivo binario de la constancia fiscal en memoria */
  guardarArchivoConstancia(file: File): void {
    this.archivoConstancia = file;
  }

  /** Obtiene el archivo binario de la constancia fiscal guardado en memoria */
  obtenerArchivoConstancia(): File | null {
    return this.archivoConstancia;
  }

  /** Guarda datos parciales del paso actual, fusionando con lo ya almacenado */
  guardarPaso(datos: Partial<PreRegistroState>): void {
    const actual = this.obtenerEstado();
    const nuevo = { ...actual, ...datos };
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(nuevo));
  }

  /** Lee el estado acumulado de todos los pasos */
  obtenerEstado(): PreRegistroState {
    const raw = localStorage.getItem(this.STORAGE_KEY);
    return raw ? JSON.parse(raw) : {};
  }

  /** Limpia el estado tras un registro exitoso */
  limpiar(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.archivoConstancia = null;
  }

  /**
   * Envía el formulario completo como FormData al endpoint POST /api/auth/registro-cliente.
   * Requiere que los 4 pasos hayan guardado sus datos previamente.
   */

  registrar(): Observable<RegistroClienteResponse> {
    const estado = this.obtenerEstado();
    
    const formData = new FormData();

    formData.append('nombre', estado.nombre ?? '');
    formData.append('apellidoPaterno', estado.apellidoPaterno ?? '');
    formData.append('apellidoMaterno', estado.apellidoMaterno ?? '');
    formData.append('RFC', estado.RFC ?? '');
    formData.append('CURP', estado.CURP ?? '');
    formData.append('telefono', estado.telefono ?? '');
    formData.append('email', estado.email ?? '');
    formData.append('email2', '');
    formData.append('email3', '');
    formData.append('contraseña', estado.contrasena ?? '');
    formData.append('recaptchaToken', estado.recaptchaToken ?? '');

    if (estado.direccion) {
      formData.append('direccion.calle', estado.direccion.calle ?? '');
      formData.append('direccion.numeroExterior', estado.direccion.numeroExterior ?? '');
      if (estado.direccion.numeroInterior) {
        formData.append('direccion.numeroInterior', estado.direccion.numeroInterior);
      }
      formData.append('direccion.colonia', estado.direccion.colonia ?? '');
      formData.append('direccion.codigoPostal', estado.direccion.codigoPostal ?? '');
      formData.append('direccion.ciudad', estado.direccion.ciudad ?? '');
      if (estado.direccion.municipio) {
        formData.append('direccion.municipio', estado.direccion.municipio);
      }
      formData.append('direccion.estado', estado.direccion.estado ?? '');
      formData.append('direccion.pais', estado.direccion.pais ?? '');
      if (estado.direccion.telefono1) {
        formData.append('direccion.telefono1', estado.direccion.telefono1);
      }
    }

    if (this.archivoConstancia) {
      formData.append('constanciaFiscal', this.archivoConstancia, this.archivoConstancia.name);
    }

    return this.http.post<RegistroClienteResponse>(
      `${this.apiUrl}/api/Auth/registro-cliente`,
      formData,
    );
  }
}
