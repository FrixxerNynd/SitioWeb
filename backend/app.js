import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import helmet from 'helmet';
import morgan from 'morgan';
import swaggerUi from 'swagger-ui-express';
import swaggerSpec from './src/config/swagger.js';
import logger from './src/utils/logger.js';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3000;

//1.- Middlewares
app.use(helmet());
app.use(cors({
    origin: process.env.CORS_ORIGIN || '*',
    methods: ['GET', 'POST', 'PUT', 'DELETE'],
    allowedHeaders: ['Content-Type', 'Authorization']
}));

app.use(express.json());

//2.- Morgan para logging de solicitudes HTTP
const morganStream = {
    write: (message) => logger.info(message.trim())
};
app.use(morgan('combined', { stream: morganStream}));

//3.- Rutas de control y Health
app.get('/health', (req, res) => {
    res.status(200).json({
        status: 'UP',
        timestamp: new Date().toISOString(),
        service: 'Catalog Service Backend'
    });
});

//Done: Agregar configuracion swagger para documentar API
app.use('/api-docs', swaggerUi.serve, swaggerUi.setup(swaggerSpec));
//TODO: Agregar rutas para cruds de pedidos y gestion de usuarios.

//4.-  Manejo de errores global

app.use((err, req, res, next) => {
    logger.error(`${err.status || 500} - ${err.message} - ${req.originalUrl} - ${req.method} - ${req.ip}`);

    res.status(err.status || 500).json({
        error: {
            message: process.env.NODE_ENV === 'production' ? 'Internal Server Error' : err.message,
        }
    });
});

// 5.- Iniciar el servidor
app.listen(PORT, () => {
    logger.info (`Servidor iniciado en el puerto ${PORT} - Entorno: ${process.env.NODE_ENV || 'development'}`)
    logger.info(`📖 Documentación API disponible en http://localhost:${PORT}/api-docs`);
})