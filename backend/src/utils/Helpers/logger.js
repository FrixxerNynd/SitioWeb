import wins from 'winston';
import path from 'path';
import { fileURLToPath } from 'url';

const filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(filename);

const logger = wins.createLogger({
    level: 'info',
    format: wins.format.combine(
        wins.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss'}),
        wins.format.errors({ stack: true }),
        wins.format.json()
    ),
    transports: [
        //Errores graves: ERROR.log
        new wins.transports.File({
            filename: path.join(__dirname, '../../../logs/ERROR.log'),
            level: 'error'
        }),
        //Advertencias: WARN.log
        new wins.transports.File({
            filename: path.join(__dirname, '../../../logs/WARN.log'),
            level: 'warn'
        }),
        //Todo lo demas: INFO.log
        new wins.transports.File({
            filename: path.join(__dirname, '../../../logs/INFO.log'),
            level: 'info'
        }),
    ]
});

if (process.env.NODE_ENV != 'production') {
    logger.add(new wins.transports.Console({
        format: wins.format.combine(
            wins.format.colorize(),
            wins.format.simple()
        )
    }));
}
export default logger;