# SO 2.6 - 30-06-26

## Resumen

Múltiples ajustes en backend y frontend.

## 1. Pre-registro envío de constancia fiscal en FormData — Frontend

**Archivo:** `front-end/src/app/auth/pre-registro/pre-registro.service.ts`
**Archivo:** `front-end/src/app/auth/pre-registro/pages/datos-fiscales/datos-fiscales.ts`

### Problema

Por parte del frontend, el asistente de pre-registro enviaba los datos del formulario como un objeto JSON plano (`application/json`), lo que impedía que el archivo PDF/imagen de la constancia fiscal fuera transmitido al backend. Aunque en el primer paso del wizard se capturaba el archivo de la constancia, este no se almacenaba en el estado persistido (localStorage solo admite texto) ni se adjuntaba a la petición HTTP final.

### Solución

1. Modificar `datos-fiscales.ts` para que, al seleccionar el archivo de constancia, este sea guardado en memoria en una propiedad dentro del servicio `PreRegistroService` (el cual actúa como un singleton persistente en memoria).
2. Reescribir el método `registrar()` en `pre-registro.service.ts` para que ensamble y envíe un objeto `FormData` (`multipart/form-data`) en lugar de JSON.
3. Aplanar los campos del formulario y los del objeto anidado de la dirección (`direccion.calle`, `direccion.colonia`, etc.) usando notación de puntos, e incorporar el archivo binario del PDF bajo la clave `constanciaFiscal`.

### Código — Antes

**`pre-registro.service.ts`**

```typescript
  registrar(): Observable<RegistroClienteResponse> {
    const estado = this.obtenerEstado();

    const payload = {
      nombre: estado.nombre ?? '',
      apellidoPaterno: estado.apellidoPaterno ?? '',
      apellidoMaterno: estado.apellidoMaterno ?? '',
      RFC: estado.RFC ?? '',
      CURP: estado.CURP ?? '',
      telefono: estado.telefono ?? '',
      email: estado.email ?? '',
      email2: '',
      email3: '',
      contraseña: estado.contrasena ?? '',
      recaptchaToken: estado.recaptchaToken ?? '',
      direccion: estado.direccion,
    };

    return this.http.post<RegistroClienteResponse>(
      `${this.apiUrl}/api/Auth/registro-cliente`,
      payload,
    );
  }
```

**`datos-fiscales.ts`**

```typescript
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.loginForm.patchValue({ constancia: file });
      this.loginForm.get('constancia')?.updateValueAndValidity();
    }
  }
```

### Código — Después

**`pre-registro.service.ts`**

```typescript
  private archivoConstancia: File | null = null;

  guardarArchivoConstancia(file: File): void {
    this.archivoConstancia = file;
  }

  obtenerArchivoConstancia(): File | null {
    return this.archivoConstancia;
  }

  limpiar(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    this.archivoConstancia = null; // Se agregó para limpiar el archivo
  }

  registrar(): Observable<RegistroClienteResponse> {
    const estado = this.obtenerEstado();
    const formData = new FormData();

    formData.append('nombre', estado.nombre ?? '');
    formData.append('apellidoPaterno', estado.apellidoPaterno ?? '');
    formData.append('apellidoMaterno', estado.apellidoMaterno ?? '');
    formData.append('RFC', estado.RFC ?? '');
    formData.append('CURP', estado.CURP ?? '');
    formData.append('telefono', estado.telefono ?? '');
    formData.append('email', estado.email ?? '');
    formData.append('email2', '');
    formData.append('email3', '');
    formData.append('contraseña', estado.contrasena ?? '');
    formData.append('recaptchaToken', estado.recaptchaToken ?? '');

    if (estado.direccion) {
      formData.append('direccion.calle', estado.direccion.calle ?? '');
      formData.append('direccion.numeroExterior', estado.direccion.numeroExterior ?? '');
      if (estado.direccion.numeroInterior) {
        formData.append('direccion.numeroInterior', estado.direccion.numeroInterior);
      }
      formData.append('direccion.colonia', estado.direccion.colonia ?? '');
      formData.append('direccion.codigoPostal', estado.direccion.codigoPostal ?? '');
      formData.append('direccion.ciudad', estado.direccion.ciudad ?? '');
      if (estado.direccion.municipio) {
        formData.append('direccion.municipio', estado.direccion.municipio);
      }
      formData.append('direccion.estado', estado.direccion.estado ?? '');
      formData.append('direccion.pais', estado.direccion.pais ?? '');
      if (estado.direccion.telefono1) {
        formData.append('direccion.telefono1', estado.direccion.telefono1);
      }
    }

    if (this.archivoConstancia) {
      formData.append('constanciaFiscal', this.archivoConstancia, this.archivoConstancia.name);
    }

    return this.http.post<RegistroClienteResponse>(
      `${this.apiUrl}/api/Auth/registro-cliente`,
      formData,
    );
  }
```

**`datos-fiscales.ts`**

```typescript
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.loginForm.patchValue({ constancia: file });
      this.loginForm.get('constancia')?.updateValueAndValidity();
      this.preRegistroService.guardarArchivoConstancia(file); // Se agregó para guardar el archivo en el servicio
    }
  }
```

**Motivo:** El formulario enviado mandaba los datos en formato JSON y lo que se espera es que se manden en formato FormData para adjuntar el archivo de la constancia fiscal (`constanciaFiscal`) y estandarizar el envío de datos binarios y estructurados al backend.
