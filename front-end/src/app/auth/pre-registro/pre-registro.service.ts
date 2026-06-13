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
  }

  /**
   * Envía el formulario completo al endpoint POST /api/auth/registro-cliente.
   * Requiere que los 4 pasos hayan guardado sus datos previamente.
   */
  registrar(): Observable<RegistroClienteResponse> {
    const estado = this.obtenerEstado();

    const payload = {
      nombre: estado.nombre ?? '',
      apellidoPaterno: estado.apellidoPaterno ?? '',
      apellidoMaterno: estado.apellidoMaterno ?? '',
      RFC: estado.RFC ?? '',
      CURP: estado.CURP ?? '',
      telefono: estado.telefono ?? '',
      email: estado.email ?? '',
      email2: '',
      email3: '',
      contraseña: estado.contrasena ?? '',
      recaptchaToken: estado.recaptchaToken ?? '',
      direccion: estado.direccion,
    };

    return this.http.post<RegistroClienteResponse>(
      `${this.apiUrl}/api/Auth/registro-cliente`,
      payload,
    );
  }
}
