// front-end/src/app/services/secure-auth.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http'; // ← Agregar HttpHeaders
import { Observable, tap, BehaviorSubject } from 'rxjs'; // ← Quitar throwError si no se usa
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

// ← AGREGAR esta interfaz
export interface LogoutResponse {
  message: string;
  success?: boolean;
}

@Injectable({ providedIn: 'root' })
export class SecureAuthService {
  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private apiUrl = environment.apiCabsUrl;

  // BehaviorSubject para mantener el estado del usuario en tiempo real
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    this.restaurarSesion();
  }

  // ==================== RESTAURAR SESIÓN ====================
  
  private restaurarSesion(): void {
    const userJson = localStorage.getItem('user');
    const token = this.cookieService.getCookie('token') || localStorage.getItem('token');
    
    if (userJson && token) {
      try {
        const user = JSON.parse(userJson);
        this.currentUserSubject.next(user);
        console.log('🔄 Sesión restaurada para:', user.nombre);
      } catch (e) {
        console.error('Error al restaurar sesión:', e);
        this.currentUserSubject.next(null);
      }
    } else {
      this.currentUserSubject.next(null);
    }
  }

  // ==================== LOGIN ====================
  
  login(credentials: { email: string; password: string; recaptchaToken?: string }): Observable<AuthResponse> {
    console.log('🔐 Intentando login en:', `${this.apiUrl}/api/auth/login`);
    
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          console.log('📡 Respuesta del backend:', response);
          if (response && response.user) {
            localStorage.setItem('user', JSON.stringify(response.user));
            this.currentUserSubject.next(response.user);
            
            if (response.token) {
              localStorage.setItem('token', response.token);
              this.cookieService.setCookie('token', response.token, 7);
              console.log('✅ Token guardado en localStorage y cookie');
            }
            console.log('✅ Usuario guardado:', response.user);
          }
        })
      );
  }

  // ==================== LOGOUT ====================
  
  /**
   * Cierra la sesión del usuario tanto en el frontend como en el backend
   * Llama al endpoint /api/Auth/logout para invalidar el token
   */
  logout(): Observable<LogoutResponse> {
    console.log('🚪 Cerrando sesión...');
    
    const token = this.getToken();
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }
    
    return this.http.post<LogoutResponse>(
      `${this.apiUrl}/api/Auth/logout`,
      {},
      { headers }
    ).pipe(
      tap({
        next: (response) => {
          console.log('✅ Logout exitoso en el backend:', response.message);
          this.limpiarSesionLocal();
        },
        error: (error) => {
          console.error('❌ Error en logout del backend:', error);
          // Aún si falla el backend, limpiar la sesión local
          this.limpiarSesionLocal();
        }
      })
    );
  }

  /**
   * Limpia la sesión local (sin llamar al backend)
   * Útil para cuando el backend falla o para logout manual
   */
  limpiarSesionLocal(): void {
    localStorage.removeItem('user');
    localStorage.removeItem('token');
    this.cookieService.deleteCookie('token');
    this.currentUserSubject.next(null);
    console.log('🧹 Sesión local limpiada');
  }

  // ==================== RECUPERACIÓN DE CUENTA ====================
  
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
  
  getToken(): string | null {
    const token = this.cookieService.getCookie('token') || localStorage.getItem('token');
    console.log('🔑 Token recuperado:', token ? `${token.substring(0, 20)}...` : 'NO TOKEN');
    return token;
  }

  getCurrentUser(): User | null {
    const user = this.currentUserSubject.getValue();
    if (user) return user;
    
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try {
        const parsed = JSON.parse(userJson);
        this.currentUserSubject.next(parsed);
        return parsed;
      } catch (e) {
        return null;
      }
    }
    return null;
  }

  isAuthenticated(): boolean {
    const user = this.getCurrentUser();
    const token = this.getToken();
    const isAuth = !!(user && token);
    console.log('🔐 Autenticado:', isAuth);
    return isAuth;
  }
}