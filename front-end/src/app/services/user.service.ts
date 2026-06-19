import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { firstValueFrom, Observable, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { CookieService } from './cookie.service';
import { ApiResponse, UsuarioRegistroRequest, Usuario } from '../interfaces/user.interfaces';

@Injectable({ providedIn: 'root' })
export class UserService {
    private http = inject(HttpClient);
    private cookieService = inject(CookieService);
    private baseUrl = environment.apiCabsUrl;

    /**
     * Obtiene todos los usuarios enlazados al admin autenticado.
     * El idCliente NUNCA se manda desde aquí: el backend lo extrae
   * del token (claim) gracias al interceptor que ya agrega el Bearer.
   * Solo enviamos el filtro que sí es legítimo mandar desde el cliente.
   */
  obtenerUsuarios(incluirInactivos: boolean = false): Observable<ApiResponse<Usuario[]>> {
    const params = new HttpParams().set('incluirInactivos', incluirInactivos);

    return this.http.get<ApiResponse<Usuario[]>>(`${this.baseUrl}/api/Auth/usuarios`, { params });
  }

  /**
   * Obtiene los datos del usuario actualmente autenticado.
   */
  obtenerUsuarioActual(): Observable<ApiResponse<Usuario>> {
    return this.http.get<ApiResponse<Usuario>>(`${this.baseUrl}/api/Auth/me`);
  }

  /**
   * Registra un nuevo usuario enlazado al cliente/admin autenticado.
   * idCliente queda fuera del payload; el backend debe resolverlo
   * desde el token, no desde este body.
   */
  registrarUsuario(usuario: UsuarioRegistroRequest): Observable<ApiResponse<Usuario>> {
    return this.http.post<ApiResponse<Usuario>>(`${this.baseUrl}/api/Auth/registro`, usuario);
  }

  inactivarUsuario(idUsuario: number): Observable<ApiResponse<Usuario>> {
    return this.http.delete<ApiResponse<Usuario>>(`${this.baseUrl}/api/Auth/delete-user/${idUsuario}`)
  }
}