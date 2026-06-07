import { Routes } from '@angular/router';
import { PageInicioSesion } from './auth/inicio-sesion/inicio-sesion';
import { PageDatosFiscales } from './auth/pre-registro/pages/datos-fiscales/datos-fiscales';
import { PageDatosPersonales } from './auth/pre-registro/pages/datos-personales/datos-personales';
import { PageDomicilio } from './auth/pre-registro/pages/domicilio/domicilio';
import { PageCuenta } from './auth/pre-registro/pages/cuenta/cuenta';
import { PageSolicitarCorreo } from './auth/recuperar-contraseña/pages/solicitar-correo/solicitar-correo';
import { PageVerificarCodigo } from './auth/recuperar-contraseña/pages/verificar-codigo/verificar-codigo';
import { PageNuevaContrasena } from './auth/recuperar-contraseña/pages/nueva-contrasena/nueva-contrasena';
import { PageListaProducto } from './pages/usuario/pages/catalogo-producto/lista-producto/lista-producto';
// import { PageDetalleProducto } from './pages/usuario/pages/catalogo-producto/detalle-producto/detalle-producto';
import { ListaOrdenPague } from './pages/usuario/pages/orden-compra/lista-orden/lista-orden';
import { MainLayout } from './layouts/main-layout/main-layout';
import { AuthLayout } from './layouts/auth-layout/auth-layout';
//import { AuthGuard } from './guards/auth.guard'; 

export const routes: Routes = [
    {
        path: '',
        component: AuthLayout,
        children: [
            { path: 'inicio-sesion', component: PageInicioSesion },
            {
                path: 'pre-registro',
                children: [
                    { path: 'datos-fiscales', component: PageDatosFiscales },
                    { path: 'datos-personales', component: PageDatosPersonales },
                    { path: 'domicilio', component: PageDomicilio },
                    { path: 'cuenta', component: PageCuenta },
                    { path: '', redirectTo: 'datos-fiscales', pathMatch: 'full' }
                ]
            },
            {
                path: 'recuperar-contrasena',
                children: [
                    { path: 'solicitar-correo', component: PageSolicitarCorreo },
                    { path: 'verificar-codigo', component: PageVerificarCodigo },
                    { path: 'nueva-contrasena', component: PageNuevaContrasena },
                    { path: '', redirectTo: 'solicitar-correo', pathMatch: 'full' }
                ]
            },
            { path: '', redirectTo: 'inicio-sesion', pathMatch: 'full' }
        ]
    },
    {
        path: '',
        component: MainLayout,
       // canActivate: [AuthGuard],
        children: [
            {
                path: 'catalogo-producto',
                children: [
                    { path: 'lista-producto', component: PageListaProducto },
                    // { path: 'detalles-producto/:id', component: PageDetalleProducto },
                    { path: 'detalles-producto', redirectTo: 'lista-producto', pathMatch: 'full' },
                    { path: '', redirectTo: 'lista-producto', pathMatch: 'full' }
                ]
            },
            {
                path: 'ordenes',
                children: [
                    { path: 'lista-ordenes', component: ListaOrdenPague },
                ]
            }
        ]
    },
   ///  { path: '**', redirectTo: '/inicio-sesion' }
];