// usuario.models.ts

/**
 * DTO para registrar un nuevo usuario enlazado al cliente/admin autenticado.
 * NOTA: idCliente NO se incluye aquí a propósito. El backend lo obtiene
 * del claim del token (User.FindFirst("idCliente")), nunca del body.
 * Si el backend todavía exige idCliente en el JSON, hay que corregir
 * el controlador antes de usar este modelo.
 */
export interface UsuarioRegistroRequest {
  nombre: string;
  apellido: string;
  telefono: number;
  email: string;
  contrasena: string;
  confirmarContrasena: string;
  activo?: boolean;
}

/**
 * Forma de un usuario tal como lo regresa el backend.
 * Ajusta los campos según lo que realmente devuelva ObtenerTodosLosUsuariosAsync.
 */
export interface Usuario {
  id: number;
  nombre: string;
  apellido: string;
  telefono: number;
  email: string;
  activo: boolean;
  idCliente: number;
}

/**
 * Envelope genérico de respuesta que usa tu API
 * (success / data / count, o success / message / error).
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  count?: number;
  message?: string;
  error?: string;
}