# Componente UiCardComponent

El **UiCardComponent** es un componente reutilizable que muestra información resumida en forma de tarjeta (card). Su propósito es presentar valores numéricos, indicadores, métricas o estados de manera clara, visual y consistente dentro de dashboards o paneles informativos.

## ¿Para qué sirve?

Este componente se utiliza para:

- Mostrar métricas o KPIs
- Presentar valores numéricos destacados
- Mostrar estados positivos, negratos o neutros
- Visualizar información resumida en dashboards
- Permitir la selección de una tarjeta

Es ideal para paneles de control, reportes y vistas de resumen.

## Instalación

Importa el componente donde se utilizará la card:

```typescript
import { UiCardComponent } from './ruta/ui-card/ui-card.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [UiCardComponent],
})
export class DashboardComponent {}


Uso básico
<app-ui-card
  nameIcono="chart-bar"
  title="Ventas mensuales"
  [valor]="12500">
</app-ui-card>

Uso con símbolo de moneda
<app-ui-card
  nameIcono="currency-dollar"
  title="Ingresos"
  [valor]="45000"
  [viewSimbolo]="true">
</app-ui-card>

Uso con porcentaje
<app-ui-card
  title="Tasa de éxito"
  [valor]="87.5">
</app-ui-card>


*El componente detecta automáticamente palabras como “tasa” o “éxito” en el título para formatear el valor como porcentaje.*

Uso con etiqueta de estado
<app-ui-card
  title="Crecimiento"
  [valor]="12"
  [viewLabel]="true"
  estadoEtiqueta="positivo"
  porcentaje="+12%">
</app-ui-card>

Uso seleccionable
<app-ui-card
  title="Usuarios activos"
  [valor]="320"
  [selected]="true"
  (seleccionar)="onSeleccionarCard()">
</app-ui-card>

