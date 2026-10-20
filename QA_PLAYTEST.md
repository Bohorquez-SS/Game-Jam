# GOOD NIGHT: plan de pruebas

Estado al 20/10/2026: **pendiente de ejecutar en Unity**. Las comprobaciones estáticas están separadas en [Verificación final](Docs/VERIFICACION_FINAL.md). Las tablas siguientes son un plan, no resultados aprobados.

## Sesión con jugadores nuevos (sin soluciones)

Una persona juega y otra observa. No dar soluciones; después de tres minutos de atasco se puede recordar que Tab → Objetivos ofrece pistas. Registrar capítulo, hora, objetivo, atasco, muerte, reacción y resolución usada. Probar lectura a 1920×1080 y 1366×768.

| Observación | Cap. 1 | Cap. 2 | Cap. 3 | Cap. 4 | Cap. 5 |
|---|---|---|---|---|---|
| Comprende el objetivo | | | | | |
| Encuentra objetos sin ayuda externa | | | | | |
| Comprende y usa las pistas | | | | | |
| Entiende el puzle y su resultado | | | | | |
| Se orienta y puede moverse | | | | | |
| Sustos efectivos / momentos lentos | | | | | |
| Muertes y motivo | | | | | |
| Tiempo de capítulo | | | | | |

Comprobar legibilidad de teléfono y subtítulos, contraste, volumen, colisiones, respuesta de objetos y que una interacción del teléfono no active el mundo.

Al terminar preguntar qué cree que pasó, qué momento dio más miedo, dónde se atascó, si algún puzle fue injusto y qué cambiaría. No explicar los otros finales antes de registrar sus respuestas.

## Pruebas técnicas del equipo (CON SPOILERS)

Usar Unity **6000.3.26f1** y la escena `Assets/_GoodNight/Scenes/GoodNight_Main.unity`. Conservar una copia antes de regenerar la escena con **Tools → GOOD NIGHT → Build Complete Game**. Los atajos debug requieren editor o Development Build; no sustituyen el recorrido normal.

| # | Prueba | Procedimiento / criterio | Resultado |
|---|---|---|---|
| 1 | Importación y compilación | Consola sin errores; paquetes URP resueltos | Pendiente |
| 2 | Escena guardada y regenerada | Probar ambas por separado; mismo manual GUARDIÁN | Pendiente |
| 3 | Menú y opciones | Nueva partida, Continuar, idioma, audio, salir | Pendiente |
| 4 | Movimiento y colisiones | Todas las habitaciones; puertas y muebles no atrapan al jugador | Pendiente |
| 5 | Teléfono e interacción | Tab, chats, llamadas, inspección; E no actúa dos veces | Pendiente |
| 6 | Alarma y casa | PC, puerta, tres ventanas, recibo, PIN 2308, error y reintento | Pendiente |
| 7 | Espejo y falsa mamá | Susto una vez; abrir durante la voz falsa lleva a C2_Despertar | Pendiente |
| 8 | Fusible y cuadro | Repuesto en caja; F2 reemplazable; sobrecarga al superar 7 | Pendiente |
| 9 | Cámaras | Archivo/directo, error F2, nombres de habitación, salida Esc | Pendiente |
| 10 | Entidad y escondites | Detecta, persigue, pierde al jugador; no sigue tras morir | Pendiente |
| 11 | VHS | Recoger A/B/C; ver todas; ordenar mal y corregir; C-A-B reconstruye | Pendiente |
| 12 | Habitación 06 | Llave del abrigo, armario, circuito 06, puerta, carrete, grabadora, palanca | Pendiente |
| 13 | Inicio del capítulo 5 | Desde F8: Sujeto 0 activo; ALARMA + CÁMARAS + CERRADURAS = 7, LUCES/06 apagados | Pendiente |
| 14 | Regreso seguro | Tras restaurar energía, cocina no se sella con jugador dentro; volver a 06 prepara decisión | Pendiente |
| 15 | C5_Decision | Morir/reiniciar/Continuar; reaparecer en 06 con cocina sellada y doble presente | Pendiente |
| 16 | Dos identidades | Consola lista: AISLAR AL DOBLE y AISLARME A MÍ MISMO; ningún botón de dormitorio | Pendiente |
| 17 | Verdades y finales | Probar con 2 y 4 verdades; ambas elecciones disponibles en ambos casos | Pendiente |
| 18 | Final A | Elección normal → amanecer, cámara, créditos y menú | Pendiente |
| 19 | Final B | Elección normal → autoaislamiento, análisis no concluyente, créditos y menú | Pendiente |
| 20 | Pistas ES/EN | Tres objetivos de C5 encuentran su entrada; niveles 45/100/170 s; palanca indica pared este | Pendiente |
| 21 | Documentos ES/EN | Manual y pista guardada coinciden; texto legible y sin cortes | Pendiente |
| 22 | Checkpoints restantes | Recargar cada etapa y verificar puertas, inventario, objetivo y entidad | Pendiente |
| 23 | Audio e interfaz | Voces, subtítulos, tensión, volúmenes; 1080p y 1366×768 | Pendiente |
| 24 | Créditos | Nombres y licencias legibles; vuelta al menú | Pendiente |
| 25 | Build Windows | Compilar y recorrer ambos finales fuera del editor | Pendiente |
| 26 | Debug | F9/F10, F11/F12, Shift+F1..F5; ausentes en build no Development | Pendiente |

Cada prueba ejecutada debe registrar responsable, fecha, versión/commit, resultado y evidencia. Si falla, crear una incidencia reproducible en Trello; no marcarla como terminada solo por estar documentada.
