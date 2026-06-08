# Componente UiEtiquetaComponent

Es un componente reutilizable que representa una etiqueta visual (badge) con icono y texto. Su propósito es resaltar estados, tendencias o clasificaciones de manera clara y consistente, utilizando variantes visuales como positivo, negativo o neutro.

## ¿Para qué sirve?

Este componente se utiliza para:

- Mostrar estados o indicadores
- Resaltar tendencias positivas o negativas
- Etiquetar información relevante
- Acompañar métricas o valores destacados
- Permitir interacción básica mediante clic

## Variantes disponibles

| Variante | Uso | Icono |
|----------|-----|-------|
| positivo | Tendencia favorable | Flecha hacia arriba |
| negativo | Tendencia desfavorable | Flecha hacia abajo |
| neutro | Información general | Información |

## Instalación

Importa el componente donde se utilizará la etiqueta:

```typescript
import { UiEtiquetaComponent } from './ruta/ui-etiqueta/ui-etiqueta.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [UiEtiquetaComponent],
})
export class DashboardComponent {}
