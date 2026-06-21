import swaggerJSDoc from 'swagger-jsdoc';

const options = {
  definition: {
    openapi: '3.0.0',
    info: {
      title: 'CABS - API de SitioWeb',
      version: '1.0.0',
      description:
        'Servicio backend en Node.js/Express para gestión del carrito de compras CABS y consumo del catálogo Exel del Norte.',
    },
    servers: [
      {
        url: 'http://localhost:3000',
        description: 'Servidor de Desarrollo Local',
      },
    ],
    components: {
      securitySchemes: {
        cokieAuth: {
          type: 'apiKey',
          in: 'cookie',
          name: 'auth_token',
          description: 'Token de autenticación almacenado en una cookie ',
        },
      },
      schemas: {
        Percentage: {
          type: 'object',
          properties: {
            id: { type: 'integer', example: 1 },
            id_categoria: { type: 'string', example: '15' },
            nombre_categoria: { type: 'string', example: 'Cemento y derivados' },
            porcentaje: { type: 'number', example: 15.5 },
          },
        },
        PedidoResponse: {
          type: 'object',
          properties: {
            success: { type: 'boolean', example: true },
            data: {
              type: 'object',
              properties: {
                id: { type: 'integer', example: 12 },
                userId: { type: 'integer', example: 7 },
                paymentType: { type: 'string', example: 'Transferencia' },
                deliveryMethod: { type: 'string', example: 'Domicilio' },
                subtotal: { type: 'number', example: 1580.00 },
                shippingCost: { type: 'number', example: 90.00 },
                total: { type: 'number', example: 1670.00 },
                status: { type: 'string', example: 'PAGADO_PENDIENTE_SURTIDO' },
                items: {
                  type: 'array',
                  items: {
                    type: 'object',
                    properties: {
                      productId: { type: 'integer', example: 101 },
                      sku: { type: 'string', example: 'SKU-001' },
                      name: { type: 'string', example: 'Cemento Portland 50kg' },
                      price: { type: 'number', example: 790.00 },
                      quantity: { type: 'integer', example: 2 },
                      totalPrice: { type: 'number', example: 1580.00 },
                    },
                  },
                },
              },
            },
          },
        },
      },
    },
  },
  // Swagger escanea los JSDoc que están dentro de cart.routes.js
  apis: [
    './src/routes/cart.routes.js',
<<<<<<< Updated upstream
    './src/routes/address.routes.js',
    './src/routes/order.routes.js',
    './src/routes/exel.routes.js',
    './app.js',
  ],
=======
    './src/routes/exel.routes.js',
    './src/routes/percentage.routes.js',
    './src/routes/order.routes.js',
    './app.js'],
>>>>>>> Stashed changes
};

const swaggerSpec = swaggerJSDoc(options);

export default swaggerSpec;