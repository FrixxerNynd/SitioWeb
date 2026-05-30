import { Injectable, inject } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { SecureAuthService } from '../services/secure-auth.service';
import { CookieService } from '../services/cookie.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  private authService = inject(SecureAuthService);
  private cookieService = inject(CookieService);
  private router = inject(Router);

  canActivate(): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    // Verificar token en cookie
    const hasToken = this.cookieService.hasCookie('token');
    const hasUser = this.authService.getCurrentUser() !== null;
    
    if (hasToken && hasUser) {
      console.log('✅ AuthGuard: Usuario autenticado');
      return true;
    }
    
    console.log('❌ AuthGuard: Usuario NO autenticado, redirigiendo a login');
    
    // Limpiar datos inconsistentes
    if (!hasToken) {
      this.cookieService.deleteCookie('token');
    }
    if (!hasUser) {
      localStorage.removeItem('user');
    }
    
    // Redirigir al login
    return this.router.createUrlTree(['/inicio-sesion']);
  }
}