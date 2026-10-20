# GOOD NIGHT: guía de playtest (SIN SPOILERS)

Para la sesión de prueba con jugadores nuevos. Este documento **no** revela soluciones: el que observa anota, el que juega no lo lee.

## Cómo organizar la prueba

1. Una persona juega, otra observa y anota. Rotar entre capítulos si hay tiempo.
2. **No ayudar** al jugador. Si lleva más de 3 minutos atascado, recordarle solo que el teléfono (Tab → app Objetivos) tiene pistas.
3. Pedirle que piense en voz alta: qué cree que tiene que hacer y por qué.
4. Anotar la **hora** de cada momento importante (inicio de capítulo, atascos, muertes, sustos que funcionaron).
5. Al final, hacer las preguntas de la última sección.

Resolución recomendada: 1920×1080. Repetir una partida corta en 1366×768 para comprobar textos.

---

## Hoja del observador (por capítulo)

Marcar ✔ / ✘ y anotar comentarios.

| Pregunta | Cap 1 | Cap 2 | Cap 3 | Cap 4 | Cap 5 |
|---|---|---|---|---|---|
| ¿El jugador sabía qué hacer al empezar el capítulo? | | | | | |
| ¿El objetivo en pantalla era claro y siempre visible? | | | | | |
| ¿Encontró lo que buscaba sin ayuda externa? | | | | | |
| ¿Usó las pistas del teléfono? ¿Le sirvieron? | | | | | |
| ¿Entendió los puzzles (no los resolvió al azar)? | | | | | |
| ¿Reconocía en qué habitación estaba? | | | | | |
| ¿Hubo algún susto que funcionó? ¿Cuál? | | | | | |
| ¿Hubo algún momento aburrido o demasiado largo? | | | | | |
| ¿Sabía qué hacer después de cada escena? | | | | | |
| Muertes (cuántas y por qué) | | | | | |
| Minutos que tardó | | | | | |

## Lo que hay que evaluar

**Claridad**
- El objetivo actual está siempre visible arriba a la izquierda y las tareas se marcan al completarlas.
- Cuando el objetivo es "investiga…" el jugador entiende el problema aunque no sepa la solución.
- Las pistas del teléfono ayudan sin regalar la respuesta en el primer nivel.

**Teléfono**
- Se abre y se cierra con Tab de forma fluida; la mano lo sostiene de forma creíble.
- Los mensajes se leen bien (tamaño, contraste) en 1920×1080 y en 1366×768.
- Las notificaciones aparecen sin abrir el teléfono y no tapan el texto.
- Con el teléfono abierto no se activa nada del mundo por accidente.

**Escenario**
- Las habitaciones se distinguen (dormitorio, baño, pasillo, sala, cocina).
- Los muebles están bien apoyados (nada volcado, flotando ni atravesando paredes).
- Se ve lo suficiente para moverse sin linterna salvo en los momentos de apagón.
- Ningún mueble bloquea el paso ni deja al jugador atrapado.

**Interacciones**
- Todo objeto que muestra "[E] …" hace algo visible o audible.
- Puertas, ventanas, cajones, lámparas, ventilador, computadora y TV responden.
- Los documentos se pueden leer y girar; quedan guardados en el teléfono (app Objetos).

**Terror**
- La entidad se ve con claridad cuando aparece (en cámaras y en persona).
- Las persecuciones dan tiempo a reaccionar; el jugador entiende cómo esconderse.
- Los sustos no se repiten ni se sienten baratos; hay momentos de calma entre ellos.
- El volumen de los sustos no es molesto (sin picos que lastimen).

**Final**
- Se llega a un final, aparecen los créditos y se vuelve al menú.
- El jugador entiende (o discute) qué pasó. ¿Notó que había más de un final?

---

## Checklist técnico (equipo de desarrollo)

| # | Prueba | Cómo | OK |
|---|---|---|---|
| 1 | Compila sin errores | Consola de Unity limpia al abrir el proyecto | |
| 2 | Escena se construye | Tools → GOOD NIGHT → Build Complete Game | |
| 3 | Menú | Nueva partida / Continuar / Opciones / Salir | |
| 4 | Movimiento y colisiones | Recorrer todas las habitaciones; no atravesar paredes ni puertas cerradas | |
| 5 | Muebles | Tools → GOOD NIGHT → QA → Fotos de habitaciones y revisar `QA_Snapshots/` | |
| 6 | Objetivos persistentes | Siempre visibles salvo en paneles, teléfono y escenas | |
| 7 | Teléfono | Tab abre/cierra; E no toca el mundo con el teléfono abierto; llamadas se contestan con E | |
| 8 | Pistas | Tras 45 s/100 s/170 s se desbloquean los 3 niveles en la app Objetivos | |
| 9 | Puertas, ventanas, cajones | Animación + sonido | |
| 10 | Lámparas y ventilador | Encender/apagar; la luz cambia; las aspas giran y suenan | |
| 11 | Puzzles | Alarma, cuadro eléctrico, cintas, llave/armario, consola: aciertos y errores | |
| 12 | Cámaras | Imagen legible, REC/DIRECTO, figura visible | |
| 13 | Entidad | Detecta, persigue, pierde al jugador escondido, no sigue tras morir | |
| 14 | Checkpoints | Morir en cada tramo y comprobar que vuelve al inicio del tramo | |
| 15 | Finales | F11 (final A) y F12 (final B) llegan a créditos sin errores | |
| 16 | Depuración | F10 panel · F9 reiniciar tramo · Shift+F1..F5 abrir estaciones; en una build normal (no Development) nada de esto existe | |
| 18 | Sala | Se puede caminar del arco a la puerta principal, a la ventana y al panel de alarma sin chocar | |
| 19 | Cajones | Al abrirse se ve una caja con fondo y laterales; lo de adentro se puede tomar | |
| 20 | Cámaras | La pantalla de vigilancia ocupa casi toda la pantalla; CAM 01–04, ARCHIVO/DIRECTO y SALIR funcionan con clic | |
| 21 | Subtítulos | Grandes, abajo al centro, con fondo; se leen completos antes de desaparecer | |
| 17 | Build Windows | File → Build Profiles → Windows → Build y jugar fuera del editor | |

## Preguntas al terminar

1. ¿Qué crees que pasó esa noche? ¿Quién era quién?
2. ¿Qué momento te dio más miedo?
3. ¿En qué momento no sabías qué hacer?
4. ¿Algún puzzle te pareció injusto o adivinado?
5. ¿El teléfono se leía bien?
6. ¿Algo se veía raro (muebles, luces, texturas)?
7. Del 1 al 10, ¿lo recomendarías? ¿Qué cambiarías primero?


## Prueba de la entrega (18/10 noche)

Antes: **Tools → GOOD NIGHT → Build Complete Game** y Play.

| # | Qué probar | Cómo | OK |
|---|---|---|---|
| E1 | Cajones | Apuntar a cualquier parte del frente → "Abrir cajón"; con el cajón abierto, apuntar a la cinta/recibo → "Recoger…" sin cerrar el cajón | |
| E2 | Lavabo | Cap. 1 tras la alarma: [E] Lavarse la cara → susto en el espejo → control vuelve normal; no se repite | |
| E3 | Falsa mamá | F2: cerrar cerrojo; luego abrir cerrojo y puerta → susto y muerte → reaparece al despertar. Otra vez sin abrir → la historia sigue | |
| E4 | Fusible | F3: abrir cuadro sin repuesto → F2 QUEMADO, clic no hace nada; recoger repuesto → F2 parpadea; clic → cambiado, cámaras con energía | |
| E5 | Cintas | F5: las 3 se ven y se recogen; el contador sube 1/3 → 3/3; esperar sin buscar la B → pistas de voz | |
| E6 | Videograbadora | Ver las 3, ordenar mal (pista), ordenar C-A-B → reconstruida; el cursor queda libre y Esc sale | |
| E7 | Captura | F4 o F6: dejarse atrapar → la entidad se abalanza a la cara → pantalla de muerte → reintentar | |
| E8 | Entidad | Que cruce puertas del dormitorio, baño y 06 sin atascarse | |
| E9 | Habitación 06 | F7: llave → armario → 06 en el cuadro → puerta metálica → cinta de carrete → palanca | |
| E10 | Final | F8: consola → AISLAR COCINA (A) / AISLAR 06 (B) | |

## Hotfix final (después de Build Complete Game)

| # | Qué probar | OK |
|---|---|---|
| H1 | F7: perchero → "Revisar el abrigo de papá" → aparece la llave → se guarda → la cerradura del armario la acepta | |
| H2 | F7 completo hasta GUARDIÁN; F8: consola → final A o B → créditos | |
| H3 | VHS: la TV no tapa la videograbadora; en la interfaz la cinta se ve entera, sin la TV de fondo | |
| H4 | F4/F6: correr hacia la cocina y el fregadero; la entidad rodea la mesa y no se queda caminando contra la silla | |
| H5 | Esconderse con la entidad lejos u otra habitación y salir: no hay muerte. Con la entidad parada delante: sí | |
| H6 | Baño: sin bañera, se camina bien; botiquín se abre y se ve la cinta C | |
| H7 | Cap. 1: lavarse la cara → el espejo se ve, aparece el rostro con golpe de sonido | |
| H8 | Puerta principal (falsa mamá): igual que antes | |
