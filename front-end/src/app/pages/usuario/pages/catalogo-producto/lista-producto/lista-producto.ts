// front-end/src/app/pages/usuario/pages/catalogo-producto/lista-producto/lista-producto.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SecureAuthService } from '../../../../../services/secure-auth.service';

@Component({
    selector: 'app-lista-producto',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './lista-producto.html',
    styleUrl: './lista-producto.css'
})
export class PageListaProducto implements OnInit {
    private authService = inject(SecureAuthService);
    private router = inject(Router);
    
    currentUser: any = null;

    ngOnInit() {
        // Verificar autenticación
        if (!this.authService.isAuthenticated()) {
            this.router.navigate(['/inicio-sesion']);
            return;
        }
        
        this.currentUser = this.authService.getCurrentUser();
        console.log('Usuario autenticado:', this.currentUser);
    }

    logout() {
        this.authService.logout();
        this.router.navigate(['/inicio-sesion']);
    }
}