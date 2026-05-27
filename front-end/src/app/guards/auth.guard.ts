
import { Injectable, inject } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { SecureAuthService } from '../services/secure-auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  private authService = inject(SecureAuthService);
  private router = inject(Router);

  canActivate(): boolean {
    if (this.authService.isAuthenticated()) {
      return true;
    }
    
    // Redirigir al login si no está autenticado
    this.router.navigate(['/inicio-sesion']);
    return false;
  }
}