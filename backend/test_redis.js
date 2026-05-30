import { createClient } from 'redis';

const client = createClient({
    url: 'redis://localhost:6379'
});

client.on('connect', () => {
    console.log('✅ Conectado a Redis');
    client.quit();
    process.exit(0);
});

client.on('error', (err) => {
    console.log('❌ Error:', err.message);
    process.exit(1);
});

await client.connect();