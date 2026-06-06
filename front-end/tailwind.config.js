module.exports = {
  // Prefijo para evitar conflictos con Bootstrap
    prefix: 'tw-',
    
    // Rutas donde Tailwind buscará las clases
    content: [
        "./src/**/*.{html,ts}",
        "./src/**/*.{html,js,ts}",
        "./projects/**/*.{html,ts}"
    ],
    
    theme: {
        extend: {},
    },
    
    plugins: [],
}