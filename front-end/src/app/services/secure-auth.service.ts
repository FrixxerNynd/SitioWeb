// front-end/src/app/services/secure-auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';

export interface User {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  rol: string;
  idAgente: number | null;
}

export interface AuthResponse {
  user: User;
  token: string;
  refreshToken?: string;
  expiresIn: string;
}

// Interfaces para recuperación de cuenta
export interface RecuperarCuentaRequest {
  email: string;
  rfc: string;
}

export interface CambiarPasswordRecuperacionRequest {
  email: string;
  nuevoPassword: string;
  token: string;
}

export interface CambiarPasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface ApiResponse {
  message: string;
  success: boolean;
  token?: string;
}

@Injectable({ providedIn: 'root' })
export class SecureAuthService {
  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private apiUrl = environment.apiCabsUrl;

  // ==================== LOGIN ====================
  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    console.log('🔐 Intentando login en:', `${this.apiUrl}/api/auth/login`);
    
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          console.log('📡 Respuesta del backend:', response);
          if (response && response.user) {
            localStorage.setItem('user', JSON.stringify(response.user));
            
            if (response.token) {
              this.cookieService.setCookie('token', response.token, 7);
              console.log('✅ Token guardado en cookie');
            }
            console.log('✅ Usuario guardado:', response.user);
          } else {
            console.error('❌ Login fallido: respuesta sin usuario');
          }
        })
      );
  }

  // ==================== RECUPERACIÓN DE CUENTA ====================
  
  /**
   * Paso 1: Solicitar recuperación de cuenta enviando email y RFC
   * @param request - { email, rfc }
   * @returns Observable con la respuesta del servidor
   */
  recuperarCuenta(request: RecuperarCuentaRequest): Observable<ApiResponse> {
    console.log('📧 Solicitando recuperación de cuenta para:', request.email);
    
    return this.http.post<ApiResponse>(
      `${this.apiUrl}/api/Auth/recuperar-cuenta`, 
      request
    ).pipe(
      tap(response => {
        if (response.success) {
          console.log('✅ Solicitud de recuperación exitosa:', response.message);
        } else {
          console.error('❌ Error en solicitud de recuperación:', response.message);
        }
      })
    );
  }

  /**
   * Paso 2: Cambiar contraseña usando el token de recuperación
   * @param request - { email, nuevoPassword, token }
   * @returns Observable con la respuesta del servidor
   */
  cambiarPasswordRecuperacion(request: CambiarPasswordRecuperacionRequest): Observable<ApiResponse> {
    console.log('🔑 Cambiando contraseña con token de recuperación para:', request.email);
    
    return this.http.post<ApiResponse>(
      `${this.apiUrl}/api/Auth/cambiar-contraseña-recuperacion`,
      request
    ).pipe(
      tap(response => {
        if (response.success) {
          console.log('✅ Contraseña cambiada exitosamente');
        } else {
          console.error('❌ Error al cambiar contraseña:', response.message);
        }
      })
    );
  }

  /**
   * Paso 3: Cambiar contraseña (usuario autenticado)
   * @param request - { oldPassword, newPassword }
   * @returns Observable con la respuesta del servidor
   */
  cambiarPassword(request: CambiarPasswordRequest): Observable<ApiResponse> {
    console.log('🔑 Cambiando contraseña para usuario autenticado');
    
    return this.http.post<ApiResponse>(
      `${this.apiUrl}/api/Auth/change-password`,
      request
    ).pipe(
      tap(response => {
        if (response.success) {
          console.log('✅ Contraseña cambiada exitosamente');
        } else {
          console.error('❌ Error al cambiar contraseña:', response.message);
        }
      })
    );
  }

  // ==================== UTILIDADES ====================
  
  logout(): void {
    localStorage.removeItem('user');
    this.cookieService.deleteCookie('token');
    console.log('👋 Sesión cerrada correctamente');
  }

  getToken(): string | null {
    return this.cookieService.getCookie('token');
  }

  getCurrentUser(): User | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser() && this.cookieService.hasCookie('token');
  }
}