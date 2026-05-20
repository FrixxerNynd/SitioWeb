// front-end/src/environments/environment.ts
export const environment = {
    production: false,
    // API de Excel Norte (catálogo de productos)
    apiExcelUrl: 'https://api01.exeldelnorte.com.mx',
    // API de CABS (autenticación y backend propio) - ¡CORREGIDO!
    apiCabsUrl: 'http://localhost:5176'  // 👈 Este es el puerto correcto
};