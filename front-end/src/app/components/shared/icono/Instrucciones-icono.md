# Componente UI-Icono

Componente reutilizable de íconos SVG para Angular, basado en **Heroicons**, que permite mostrar diferentes íconos mediante una propiedad `name`, además de controlar su **tamaño** y **color** usando clases CSS.

Este componente centraliza todos los íconos de la aplicación y evita duplicar SVGs en múltiples vistas.

**Usar los iconos en figma de Heroicons**
---

# Instalación

```typescript

import { UiIconoComponent } from './ruta/atomic/icono/icono.component';

@Component({
  selector: 'app-mi-componente',
  standalone: true,
  imports: [UiIconoComponent],
  // ...
})
```

# Explicación de las variantes 

- **name:** Se escribe el nombre del icono que se quiere usar, si no se encutra saldra un mensaje "icono no disponible".

- **size:** Se escribe el tamaño del icono se debera de usar las clases de tailwind de tamaños.

- **color:** se escribe el color en texto, se debe de escribir la clase de tailwind.

```html
<app-ui-icono 
  name="check"
  size="h-6 w-6"
  color="text-gray-700" 
>
</app-ui-icono>
```

## ✨ Características

- Uso de un solo componente para múltiples íconos
- Renderizado dinámico mediante `@switch`
- Soporte para tamaños personalizados
- Soporte para colores mediante clases CSS
- SVGs escalables y accesibles
- Fácil de extender agregando nuevos íconos

---

