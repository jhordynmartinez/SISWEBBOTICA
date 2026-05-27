?? INSTRUCCIONES DE USO - MÓDULO DE CLIENTES
==============================================

## ?? TABLA DE CONTENIDOS
1. Acceso al módulo
2. Gestión de clientes (CRUD)
3. Uso en ventas
4. Búsquedas
5. Solución de problemas

---

## 1?? ACCESO AL MÓDULO

### Por Menú:
```
Dashboard ? Clientes
```

### Por URL:
```
http://localhost/Cliente/Index
```

### Permisos Necesarios:
- ? Administrador (acceso completo)
- ? Vendedor (acceso completo)
- ? Usuario regular (sin acceso)

---

## 2?? GESTIÓN DE CLIENTES (CRUD)

### A) CREAR NUEVO CLIENTE

#### Opción 1: Desde el módulo de Clientes
1. Clientes ? Nuevo Cliente
2. Completar formulario:
   - **RUC/DNI** * (obligatorio, ej: 12345678)
   - **Nombre** * (obligatorio)
   - **Apellido** (opcional)
   - **Teléfono** (opcional, ej: 987654321)
   - **Email** (opcional, debe ser válido)
   - **Dirección** (opcional)
   - **¿Cliente VIP?** (checkbox, por defecto NO)
   - **Notas** (opcional)
3. Clic en "Registrar Cliente"
4. ? Cliente registrado exitosamente

#### Opción 2: Desde la venta (Modal rápido)
1. Al crear venta ? Clic flechita expandible
2. Se abre modal ? Tab "Registrar Nuevo"
3. Completar datos (RUC*, Nombre* obligatorios)
4. Clic "Registrar y Seleccionar"
5. ? Cliente registrado y seleccionado automáticamente

### B) VER LISTA DE CLIENTES

1. Clientes ? (Se abre lista)
2. Verás tabla con:
   - RUC/DNI
   - Nombre Completo + Fecha registro
   - Teléfono
   - Email
   - Dirección
   - Indicador VIP (si aplica)
   - Botones (Editar/Eliminar)
3. Total de clientes en badge azul

### C) BUSCAR CLIENTE

#### Opción 1: Por nombre/RUC/Teléfono
1. En lista de clientes ? Campo de búsqueda
2. Escribe nombre, RUC o teléfono
3. Clic "Buscar" o presiona Enter
4. ? Resultados se filtran automáticamente
5. Para limpiar ? Clic "Limpiar"

#### Opción 2: Búsqueda rápida en venta
1. Al crear venta ? Clic flechita
2. Se abre modal ? Tab "Buscar Cliente"
3. Escribe en el campo de búsqueda
4. ? Resultados aparecen en tiempo real
5. Clic en cliente para seleccionar

### D) EDITAR CLIENTE

1. Clientes ? (Busca el cliente)
2. Clic botón "Editar" (lápiz amarillo)
3. Se abre formulario con datos actuales
4. Modifica lo necesario
5. Clic "Guardar Cambios"
6. ? Cliente actualizado

Datos que se pueden cambiar:
- RUC/DNI (si no existe otro con ese número)
- Nombre, Apellido
- Teléfono, Email
- Dirección
- VIP (sí/no)
- Notas

NO se pueden cambiar:
- Fecha de Registro (automática)
- Estado (se modifica solo)

### E) ELIMINAR CLIENTE

1. Clientes ? (Busca el cliente)
2. Clic botón "Eliminar" (papelera roja)
3. Se abre confirmación
4. Revisa datos del cliente
5. Clic "Sí, Eliminar"
6. ? Cliente marcado como INACTIVO

?? IMPORTANTE:
- El cliente NO se elimina de la BD (soft delete)
- Si el cliente tiene ventas ? NO SE PUEDE ELIMINAR
- El estado cambia a "Inactivo"
- No aparecerá más en búsquedas

---

## 3?? USO EN VENTAS

### Usar cliente al crear venta:

```
ANTES:
Seleccionar cliente: [PÚBLICO GENERAL ?]

AHORA:
Cliente [Buscar...            ] [?]
```

### Flujo completo:

1. Venta ? Crear Nueva Venta
2. Ver campo de cliente arriba
3. Opciones:

   **Opción A: Usar PÚBLICO GENERAL**
   ?? No hacer nada (es por defecto)
   ?? Continuar agregando productos

   **Opción B: Seleccionar cliente registrado**
   ?? Clic flechita [?]
   ?? Se abre modal
   ?? Tab "Buscar Cliente"
   ?? Escribe nombre o RUC
   ?? Espera resultados (tiempo real)
   ?? Haz clic en el cliente
   ?? Modal se cierra automáticamente
   ?? Cliente seleccionado en el campo
   ?? Continúa con la venta

   **Opción C: Registrar nuevo cliente**
   ?? Clic flechita [?]
   ?? Se abre modal
   ?? Tab "Registrar Nuevo"
   ?? Completa RUC/DNI y Nombre (obligatorios)
   ?? Completa otros datos (opcionales)
   ?? Clic "Registrar y Seleccionar"
   ?? Cliente se guarda en BD
   ?? Modal se cierra automáticamente
   ?? Cliente seleccionado en el campo
   ?? Continúa con la venta

4. Continúa normal:
   ?? Busca producto
   ?? Agrega cantidad
   ?? Selecciona método de pago
   ?? Genera comprobante

---

## 4?? BÚSQUEDAS

### Buscar en la lista de clientes:

**Por Nombre:**
```
Buscar: "Juan"
Resultado: Juan Pérez, Juan García, Juanita López
```

**Por RUC/DNI:**
```
Buscar: "12345678"
Resultado: Cliente con RUC 12345678
```

**Por Teléfono:**
```
Buscar: "987654"
Resultado: Clientes con ese teléfono
```

**Por Email:**
```
Buscar: "juan@"
Resultado: Clientes con email que contiene "juan@"
```

### Búsqueda en modal de venta:

- ?? Búsqueda en tiempo real (mientras escribes)
- ?? Muestra máximo 10 resultados
- ?? Haz clic en el resultado para seleccionar
- ? Si no encuentras ? Registra nuevo en Tab 2

---

## 5?? SOLUCIÓN DE PROBLEMAS

### P: ¿Qué pasa si ingreso un RUC que ya existe?
R: El sistema valida automáticamente:
   - En Create: Muestra error "RUC/DNI ya registrado"
   - En Edit: Solo permite si es del mismo cliente

### P: ¿Puedo cambiar el RUC/DNI de un cliente?
R: Sí, pero el nuevo RUC/DNI debe ser único (no puede existir en otro cliente)

### P: ¿Qué diferencia hay entre PÚBLICO GENERAL y cliente registrado?
R: 
   - PÚBLICO GENERAL: No requiere datos, es anónimo
   - Cliente Registrado: Tiene datos guardados, puede marcarse VIP

### P: ¿Qué es un cliente VIP?
R: Un marcador especial que te permite identificar clientes importantes
   - Aparece con estrella amarilla ?
   - Solo informativo (puedes crear reportes luego)

### P: ¿Qué sucede si borro un cliente?
R: No se elimina realmente:
   - Cambia a estado "Inactivo"
   - No aparece en búsquedas
   - Los datos sigue en BD
   - Mantiene historial de ventas

### P: ¿Por qué no puedo eliminar algunos clientes?
R: Si el cliente tiene ventas asociadas:
   - Sistema muestra: "No se puede eliminar, tiene X venta(s)"
   - Solución: Los clientes no se eliminan si tienen historial

### P: ¿Dónde veo cuántos clientes tengo?
R: En la lista de clientes ? Badge azul arriba a la derecha
   - Ej: "125 cliente(s)"

### P: ¿El campo de búsqueda en venta es sensible a mayúsculas?
R: NO. Busca:
   - "juan" = "JUAN" = "Juan"
   - "12345" encontrará "123456" (contiene)
   - "987" encontrará "9876543210" (contiene)

### P: ¿Qué datos son obligatorios al registrar un cliente?
R: Solo 2:
   1. RUC/DNI (máx 20 caracteres)
   2. Nombre (máx 100 caracteres)

   Todo lo demás es opcional.

---

## ?? CONTACTO / SOPORTE

Si encuentras problemas:
1. Verifica que tengas rol Administrador o Vendedor
2. Intenta refrescar la página (F5)
3. Comprueba la conexión a Internet
4. Contacta al administrador del sistema

---

**Última actualización:** 2025
**Versión:** 1.0
**Estado:** ? FUNCIONAL

==============================================
