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
        // Si es error 401 (No autorizado)
        if (error.status === 401) {
          console.warn('⚠️ Error 401 - Token inválido o expirado');
          
          // Limpiar sesión
          this.cookieService.deleteCookie('token');
          localStorage.removeItem('user');
          
          // Redirigir al login
          this.router.navigate(['/inicio-sesion']);
        }
        
        return throwError(() => error);
      })
    );
  }
}