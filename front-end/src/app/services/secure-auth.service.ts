// front-end/src/app/services/secure-auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
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

@Injectable({ providedIn: 'root' })
export class SecureAuthService {
  private http = inject(HttpClient);
  private cookieService = inject(CookieService);
  private apiUrl = environment.apiCabsUrl;

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    console.log('🔐 Intentando login en:', `${this.apiUrl}/api/auth/login`);
    
    return this.http.post<AuthResponse>(`${this.apiUrl}/api/auth/login`, credentials)
      .pipe(
        tap(response => {
          console.log('📡 Respuesta del backend:', response);
          if (response && response.user) {
            localStorage.setItem('user', JSON.stringify(response.user));
            
            if (response.token) {
              // Guardar token en cookie en lugar de localStorage
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