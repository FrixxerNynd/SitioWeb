import { PrismaClient } from '@prisma/client'
import { PrismaMssql } from '@prisma/adapter-mssql'
import logger from '../utils/logger.js'
import dotenv from 'dotenv';

dotenv.config();

const adapter = new PrismaMssql({
  server: process.env.DB_SERVER,
  port: 1433,
  database: process.env.DB_DATABASE,
  user: 'sa',
  password: process.env.DB_PASSWORD,
  options: {
    encrypt: true,
    trustServerCertificate: true,
  },
})

const prisma = new PrismaClient({ adapter })

// Verificar conexión al arrancar
prisma.$connect()
  .then(() => logger.info('✅ Prisma conectado a SQL Server'))
  .catch((err) => {
    logger.error(`❌ Error al conectar Prisma: ${err.message}`)
    process.exit(1)  // Detener el servidor si no hay DB
  })

export default prisma