import { Injectable, inject } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CookieService } from '../services/cookie.service';
import { Router } from '@angular/router';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private cookieService = inject(CookieService);
  private router = inject(Router);
  
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.cookieService.getCookie('token');
    
    let authReq = req;
    
    if (token) {
      authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      console.log('🔑 Token agregado a la petición');
    }
    
    return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error(`❌ Error HTTP ${error.status} en ${req.url}:`, error.message);
        
        // IMPORTANTE: Solo redirigir al login si:
        // 1. Es error 401
        // 2. NO es una petición a la API de Excel Norte (/api-exel)
        // 3. NO es una petición de login
        
        const isExcelApi = req.url.includes('/api-exel');
        const isLoginRequest = req.url.includes('/api/auth/login');
        
        if (error.status === 401 && !isExcelApi && !isLoginRequest) {
          console.warn('⚠️ Error 401 en API propia - Cerrando sesión');
          
          // Limpiar sesión
          this.cookieService.deleteCookie('token');
          localStorage.removeItem('user');
          
          // Redirigir al login
          this.router.navigate(['/inicio-sesion']);
        } else if (error.status === 401 && isExcelApi) {
          // Para API de Excel Norte, solo mostrar error en consola, NO redirigir
          console.warn('⚠️ Error 401 en API de Excel Norte - La API requiere autenticación, pero NO cerramos sesión');
          // NO limpiar sesión, NO redirigir
        }
        
        // Siempre propagar el error al componente para que lo maneje
        return throwError(() => error);
      })
    );
  }
}