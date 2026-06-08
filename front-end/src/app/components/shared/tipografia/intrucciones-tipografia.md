# Componente UI-tipografia

Nos permite reutlizar clases de texto en diferntes tipos de estilos de tamaño y peso. 

# Instalación

```typescript

import { UiTipografiaComponent } from './ruta/atomic/tipografia/tipografia.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [UiTipografiaComponent],
  // ...
})
```

# Propiedades (Inputs)
----------------------------------------------------------------------------------------------------------------------------------------------------------
| Propiedad           | Tipo      | Valores                                        |  POR Defecto | Descripción                                          |
|---------------------|-----------|------------------------------------------------|--------------|------------------------------------------------------|
| `variante`          | `string`  | `'h1...h6'` \| `'p'` \| `'caption'`            | `'p'`        | Se define el estilo la jerarquia del texto           |
| `texto`             | `string`  | Cualquier texto                                | `'Botón'`    | texto que se muestra                                 |
| `color`             | `string`  | Cualquier color de texto en tailwind           | `'button'`   | Color del texto                                      |
----------------------------------------------------------------------------------------------------------------------------------------------------------

# Uso 
```html
  <app-ui-tipografia
    variante= 'p';
    texto = 'Texto de prueba';
    color= 'text-gray-800';
  >
    <app-ui-tipografia>
```



