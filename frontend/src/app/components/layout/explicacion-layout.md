# Directorio `layout`

## Propósito
Contiene los componentes estructurales que definen la arquitectura visual de la aplicación. Estos componentes actúan como plantillas o envolturas que se mantienen persistentes en múltiples páginas.


## Características principales
- **Componentes envolventes**: Contienen `<router-outlet>` para renderizar páginas hijas
- **Persistencia**: Se mantienen al navegar entre rutas
- **No reutilizables** a nivel de página (excepto header/footer)
- **Poco o nulo estado interno**