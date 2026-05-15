import { Routes } from '@angular/router';
import { PageInicioSesion } from './auth/inicio-sesion/inicio-sesion';
import { PageDatosFiscales } from './auth/pre-registro/pages/datos-fiscales/datos-fiscales';
import { PageDatosPersonales } from './auth/pre-registro/pages/datos-personales/datos-personales';
import { PageDomicilio } from './auth/pre-registro/pages/domicilio/domicilio';
import { PageCuenta } from './auth/pre-registro/pages/cuenta/cuenta';

export const routes: Routes = [
    {
        path: 'inicio-sesion',
        component: PageInicioSesion,
    },
    {
        path: 'pre-registro',
        children: [
            { path: 'datos-fiscales', component: PageDatosFiscales },
            { path: 'datos-personales', component: PageDatosPersonales },
            { path: 'domicilio', component: PageDomicilio },
            { path: 'cuenta', component: PageCuenta },
            { path: '', redirectTo: 'datos-fiscales', pathMatch: 'full' }
        ]
    }
];