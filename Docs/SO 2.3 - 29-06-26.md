# SO 2.3 - 29-06-26

## Resumen

Múltiples ajustes en backend y frontend: mejora en la búsqueda de productos (motor de búsqueda), logging de filtros en Redis, formato monetario con separadores de miles, formato de precios en tarjetas de producto, y corrección del filtro de marcas.

---

## 1. Motor de búsqueda — Backend

**Archivo:** `backend/src/services/exelService.js` (línea 225-231)

### Problema

El buscador interno de la página (`POST /api/productos` con `searchTerm`) solo revisaba los campos `nombre` y `sku` del producto. El sitio oficial de Exel XLStore busca en múltiples campos, incluyendo descripciones extendidas, por lo que los resultados no coincidían.

### Flujo de búsqueda

```
Frontend (Angular)                  Backend (Express)                    Redis
       |                                  |                               |
       | POST /api/productos              |                               |
       | { searchTerm: "Computadora" }    |                               |
       +--------------------------------->|                               |
       |                                  |                               |
       |                          1. Obtener TODAS las referencias       |
       |                             desde índices en Redis               |
       |                                  |                               |
       |                          2. Cargar datos completos               |
       |                             (nombre, descripcion, sku)           |
       |                             de CADA producto en memoria          |
       |                                  |                               |
       |                          3. Filtrar en Node.js:                  |
       |                             nombre.includes("computadora")  ✓    |
       |                             descripcion.includes("computadora")✓ |
       |                             sku.includes("computadora")     ✓    |
       |                                  |                               |
       |                          4. Paginar resultado                    |
       |                                  |                               |
       |  { data: [...], page, total }    |                               |
       |<---------------------------------+                               |
```

### Código — Antes

```js
// Solo buscaba en nombre y sku
if (searchTerm) {
  const term = searchTerm.toLowerCase();
  filtrados = filtrados.filter(p =>
    (p.nombre && p.nombre.toLowerCase().includes(term)) ||
    (p.sku && p.sku.toLowerCase().includes(term))
  );
}
```

### Código — Después

```js
if (searchTerm) {
  const term = searchTerm.toLowerCase();
  filtrados = filtrados.filter(p =>
    (p.nombre && p.nombre.toLowerCase().includes(term)) ||
    (p.descripcion && p.descripcion.toLowerCase().includes(term)) ||
    (p.sku && p.sku.toLowerCase().includes(term))
  );
}
```

### Ejemplo de petición

```
POST http://localhost:3000/api/productos?pageSize=24&page=1&stock=true
Content-Type: application/json

{
  "searchTerm": "Computadora"
}
```

**Motivo:** Exel XLStore maneja un motor de búsqueda que explora nombre, descripción extendida, SKU, etc. La página no contemplaba la descripción en sus filtros, por lo que se añadió el campo `descripcion` al filtro `searchTerm` para obtener resultados más completos.

---

## 2. Logging de filtros y respuestas — Backend

**Archivo:** `backend/src/services/exelService.js` (líneas 160, 198, 257, 300)

### Cambios

Se agregaron logs estratégicos en el método `getProductsRedis`:

| Ubicación | Log | Propósito |
|-----------|-----|-----------|
| Inicio del método (línea 160) | `Filtros aplicados — searchTerm: "...", categoria: [...], ...` | Ver exactamente qué parámetros llegan a Redis |
| Cuando no hay referencias (línea 198) | `Respuesta Redis — 0 productos encontrados...` | Distinguir entre "no hay datos" y "error" |
| Existentes (líneas 257, 300) | `Devolviendo página X/Y — Z/W productos desde Redis` | Confirmar paginación correcta |

### Ejemplo de salida en consola

```
[INFO] Filtros aplicados — searchTerm: "Computadora", categoria: [], subcategoria: [], marca: [], stock: true, precioMin: undefined, precioMax: undefined, page: 1, pageSize: 24
[INFO] Cache HIT catálogo completo: 5200 referencias
[INFO] Devolviendo página 1/217 — 24/5200 productos desde Redis (filtrados)
```

**Motivo:** Facilitar la depuración de búsquedas y filtros al poder correlacionar los parámetros de entrada con los resultados obtenidos desde Redis.

---

## 3. Formato de moneda con separadores de miles — Frontend

**Archivos modificados (5):**

- `front-end/src/app/pages/administrador/pages/lista-usuario/lista-usuario.ts:145`
- `front-end/src/app/pages/administrador/pages/lista-orden/detalles-orden/detalles-orden.ts:39`
- `front-end/src/app/pages/administrador/pages/lista-pre-registro/detalles-pre-registro/detalles-pre-registro.ts:219`
- `front-end/src/app/pages/administrador/pages/lista-usuario/detalles-usuario/detalles-usuario.ts:38`
- `front-end/src/app/pages/usuario/pages/orden-compra/lista-orden/detalles-orden/detalles-orden.ts:39`

### Antes

```js
new Intl.NumberFormat('es-MX', {
  style: 'currency',
  currency: 'MXN',
  minimumFractionDigits: 2
})
// $1234.50  ← sin separador de miles
```

### Después

```js
new Intl.NumberFormat('es-MX', {
  style: 'currency',
  currency: 'MXN',
  minimumFractionDigits: 2,
  useGrouping: true
})
// $1,234.50  ← con separador de miles
```

### Comportamiento

| Entrada | Antes | Después |
|---------|-------|---------|
| `1234.5` | `$1234.50` | `$1,234.50` |
| `1234567.89` | `$1234567.89` | `$1,234,567.89` |
| `99.99` | `$99.99` | `$99.99` (sin cambio) |

**Motivo:** Habilitar explícitamente el separador de miles para mejorar la legibilidad de montos. El locale `es-MX` ya usa agrupación por defecto, pero se explicita para evitar comportamientos inconsistentes entre navegadores/entornos.

---

## 4. Formato de precios en tarjetas de producto — Frontend

**Archivos:**

- `front-end/src/app/components/shared/producto-card/producto-card.ts` (nuevo método `formatPrice`)
- `front-end/src/app/components/shared/producto-card/producto-card.html` (template actualizado)

### Antes

```html
<span class="precio-actual">${{ precio }}</span>
<span class="precio-original">${{ precioOriginal }}</span>
```

Renderizado: `$1234.5`

### Después

```html
<span class="precio-actual">${{ formatPrice(precio) }}</span>
<span class="precio-original">${{ formatPrice(precioOriginal) }}</span>
```

Nuevo método en `producto-card.ts`:

```ts
formatPrice(value: number): string {
  return value.toLocaleString('es-MX', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
}
```

Renderizado: `$1,234.50`

### Consideraciones

- Es únicamente visual — no modifica el valor numérico del producto
- `roundPrice` en `lista-producto.ts` sigue siendo quien pasa el `@Input() precio` al card como número
- `formatPrice` solo convierte a string al mostrarlo en el template
- No afecta el carrito de compras (se usa `product.precio` original)

---

## 5. Corrección del filtro de marcas — Frontend

**Archivo:** `front-end/src/app/services/exel-api-base.service.ts:142`

### Bug detectado

La API de marcas devuelve `{ id_marca: "5", nombre_marca: "3M" }`, pero el código leía `brand.id` (campo inexistente → `undefined`).

### Flujo del bug

```
API devuelve → { id_marca: "5", nombre_marca: "3M" }
                    ↑ Lee brand.id_marca (correcto)
                    ✗ brand.id → undefined

Frontend envía → { marca: [null] }  ← undefined se serializa a null

Redis busca   → indice:marca:null  ← no existe

Resultado     → 0 productos siempre
```

### Código — Antes

```ts
for (const brand of respuesta.data) {
  marcas.push({
    id: brand.id,        // ← undefined (el campo se llama id_marca)
    nombre: brand.nombre_marca,
  });
}
```

### Código — Después

```ts
for (const brand of respuesta.data) {
  marcas.push({
    id: brand.id_marca,  // ← "5" (correcto)
    nombre: brand.nombre_marca,
  });
}
```

### Evidencia del flujo corregido

```
API /marcas devuelve → { id_marca: "5", nombre_marca: "3M" }
                              ↑
IBrand { id: "5", nombre: "3M" }

POST /api/productos body → { marca: ["5"] }

Redis key → indice:marca:5  ← existe, contiene referencias

Resultado → productos filtrados por marca correctamente
```

**Motivo:** El campo `id_marca` es el identificador que usa la API de Exel para las marcas. Al leer `brand.id` se obtenía `undefined`, el filtro enviaba `null` a Redis y nunca coincidía con ningún índice, causando que el filtro de marcas siempre devolviera 0 resultados.

---

## Archivos modificados (9 en total)

| # | Archivo | Cambio |
|---|---------|--------|
| 1 | `backend/src/services/exelService.js` | Motor de búsqueda (descripción) + logging de filtros |
| 2 | `front-end/.../administrador/.../lista-usuario.ts` | `useGrouping: true` en formatCurrency |
| 3 | `front-end/.../administrador/.../detalles-orden.ts` | `useGrouping: true` en formatCurrency |
| 4 | `front-end/.../administrador/.../detalles-pre-registro.ts` | `useGrouping: true` en formatCurrency |
| 5 | `front-end/.../administrador/.../detalles-usuario.ts` | `useGrouping: true` en formatCurrency |
| 6 | `front-end/.../usuario/.../detalles-orden.ts` | `useGrouping: true` en formatCurrency |
| 7 | `front-end/.../producto-card/producto-card.ts` | Nuevo método `formatPrice` |
| 8 | `front-end/.../producto-card/producto-card.html` | Template con `formatPrice` |
| 9 | `front-end/.../exel-api-base.service.ts` | Fix `brand.id` → `brand.id_marca` |
