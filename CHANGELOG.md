# GOOD NIGHT: registro de cambios

## 18/10/2026 (tarde): correcciones P0 para la entrega

### Escenario
- **Sala redistribuida** (`HouseRooms.cs` → `Living()`): TV y sofá centrados en x=-2.5, sofá separado de la ventana, sillón al rincón suroeste, lámpara de pie junto al sofá y perchero junto a la puerta (antes compartía rincón con la lámpara). Pasillo libre de 1,5 m entre el arco, la puerta principal y la cocina. Nodo de IA "sala S" movido fuera del sofá (`HouseActors.cs`).
- **Cajones con volumen real** (`HouseBuilder.DrawerTray`): fondo, laterales y trasera que se deslizan con el frente; el contenido se apoya dentro. El recibo pasó al cajón de ARRIBA de la cómoda (coincide con pistas y guía).
- **Escritorio**: monitor CRT compacto, teclado sobre el tablero, mouse con alfombrilla y cable, pantalla nueva de 1024×768 (`screen_desktop.png`, generada por `Tools/Textures/screengen.py`).
- **Puertas** (`DoorController.cs`): mientras giran desactivan sus colisiones, así no empujan ni atrapan al jugador (puerta del baño incluida).

### Puzzles y estaciones
- **Alarma**: números impresos en las 12 teclas (1-9, C, 0, OK), texto dentro de la pantalla del panel, cartel "PIN de 4 cifras + OK".
- **Cámaras** (`CameraDeskStation.cs`): interfaz de vigilancia a pantalla grande con CAM 01–04 (con nombre de habitación), ARCHIVO/DIRECTO, SALIR, REC/LIVE + hora, aviso de movimiento y ayuda. El error de energía queda 5 s a la vista.
- **Cuadro eléctrico**: rótulos cortos en dos líneas (antes se pisaban) y medidor de carga dentro del cuadro, a la vista.
- **Instrucciones en pantalla** (`FocusStation.Instructions`) para alarma, cuadro, VHS y consola de aislamiento.

### Interfaz
- Objetivos más grandes (título 31 px, tareas 26 px) y más opacos.
- Subtítulos de 36 px con fondo semitransparente y tiempo mínimo de lectura.
- Ambiente nocturno más claro en los capítulos 2–5 (`LightingManager.cs`).

### Jugabilidad
- Escondites: la E entra **y sale** (antes no se podía salir); ya no te expulsa fuera de la casa al entrar.

### Desarrollo
- Shift+F1..F5 abre alarma / cámaras / cuadro / VHS / consola. QA → "Fotos de cajones y detalles".
- `PROGRESS.md`, `QA_PLAYTEST.md`.

## 18/10/2026: revisión y reparación (sesión de QA)

### Auditoría
- Nueva herramienta **Tools → GOOD NIGHT → QA → Fotos de habitaciones** (`Editor/QASnapshots.cs`): saca 4 fotos por habitación + planta cenital + exterior en `QA_Snapshots/`.
- Revisadas todas las habitaciones con esas fotos: camas, sillas, cajoneras, cuadros, interruptores y puertas están bien orientados y apoyados. No se encontraron muebles volcados en la versión 2.
- Copia de seguridad previa en `_backups/v2_antes_reparacion_181026/` (scripts, texturas de madera y escena).

### Bugs corregidos
- **Teléfono + E:** con el teléfono abierto, E también activaba el objeto del mundo que estaba delante (cerrar una puerta al abrir un chat). Ahora E solo actúa sobre el teléfono (`InteractionSystem.cs`).
- **Llamada + E:** cuando sonaba el teléfono, E contestaba y además usaba lo que se miraba. Ahora solo contesta y el aviso dice "[E] Contestar el teléfono".
- El texto "[E] …" ya no aparece encima de la pantalla del teléfono (`HUDView.cs`).
- La notificación interna del teléfono ya no tapa la lista de chats al abrir Mensajes (`Phone3D.cs`).

### Gráficos
- Texturas de madera nuevas, de 1024 px y con mapa normal: vetas con anillos y arcos, poros y tablas con juntas en el piso (`wood_light`, `wood_dark`, `wood_floor`). Generador en `Tools/Textures/woodgen.py`. Sustituyen a las vetas onduladas tipo "muaré".

### Herramientas de desarrollo (solo editor / Development Build)
- **F9:** reinicia el punto de control actual.
- **F10:** panel de depuración con la etapa, el objetivo y sus tareas, los eventos completados, las verdades, el estado de puertas y ventanas, la energía y el estado y la posición de la entidad.
- `StoryManager.Flags` y `PowerPanel.DebugSummary()` para el panel.

### Documentos
- `WALKTHROUGH_DEVELOPERS.md`: guía completa con spoilers, puntos de control, pistas y errores posibles.
- `QA_CHECKLIST.md`: guía de prueba sin spoilers para jugadores nuevos + checklist técnico.
- `CHANGELOG.md`: este archivo.
