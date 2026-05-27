import swaggerJSDoc from 'swagger-jsdoc';
import swaggerUi from 'swagger-ui-express';

const options = {
  definition: {
    openapi: '3.0.0',
    info: {
      title: 'API de Integración y Catálogo de Productos',
      version: '1.0.0',
      description: 'Servicio Gateway en Node.js con caché en Redis para el consumo de APIs externas de productos.',
    },
    servers: [
      {
        url: 'http://localhost:3000',
        description: 'Servidor de Desarrollo Local',
      },
    ],
    components: {
      securitySchemes: {
        // Configuramos Swagger para que acepte el JWT que emite tu microservicio de .NET
        BearerAuth: {
          type: 'http',
          scheme: 'bearer',
          bearerFormat: 'JWT',
          description: 'Introduce el token JWT generado por el microservicio de .NET',
        },
      },
    },
  },
  // Indicamos dónde buscar los comentarios para armar la documentación
  apis: ['./src/app.js', './src/routes/*.js'], 
};

const swaggerSpec = swaggerJSDoc(options);

export default swaggerSpec;