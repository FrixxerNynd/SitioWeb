// front-end/src/app/services/secure-auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

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
              localStorage.setItem('token', response.token);
            }
            console.log('✅ Usuario guardado:', response.user);
          } else {
            console.error('❌ Login fallido: respuesta sin usuario');
          }
        })
      );
  }

  logout(): void {
    localStorage.clear();
    console.log('👋 Sesión cerrada');
  }

  getCurrentUser(): User | null {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  isAuthenticated(): boolean {
    return !!this.getCurrentUser();
  }
}