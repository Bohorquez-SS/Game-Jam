# GOOD NIGHT: guía completa para desarrolladores (CON SPOILERS)

Recorrido completo de principio a fin. Para cada tramo: objetivo, lugar, objeto, interacción, pista del juego, solución, evento de terror, condición para avanzar, punto de control y errores posibles.

> No se la pasen a los testers. Para ellos está `QA_CHECKLIST.md` (sin spoilers).

**Controles:** WASD moverse · Ratón mirar · Shift correr · Ctrl agacharse · E interactuar · Tab teléfono · F linterna · Esc pausa.
En estaciones (monitor, panel de alarma, cuadro eléctrico, VHS, consola): **clic** en botones y palancas, **Esc** para salir.
En el teléfono: **1–4** cambiar de app, **rueda** elegir, **E** abrir/revelar pista, **Q** o **Retroceso** volver.

**Pistas:** cada objetivo tiene 3 niveles de ayuda (general → habitación → concreta). Se desbloquean a los 45 s, 100 s y 170 s atascado en el mismo objetivo. Se revelan en el teléfono, app **2 Objetivos**, con **E**.

---

## Cambios del 18/10 (noche): cómo se juegan ahora

- **Lavarse la cara (cap. 1, opcional, tras activar la alarma):** en el baño, el espejo está ENCIMA DEL LAVABO y el botiquín a su derecha. Apunta al lavabo → **[E] Lavarse la cara**. Escena automática: grifo, te inclinas, silencio, al levantar la vista hay una figura detrás de ti en el espejo (golpe de sonido), te giras y no hay nadie. Una sola vez. Después, si te das vuelta y vuelves a mirar el espejo, el reflejo se queda mirándote → **verdad 2/4**.
- **Falsa mamá (cap. 2):** tras cerrar el cerrojo, la voz pide que abras. Objetivo: "Decide si debes abrir la puerta". **No abrir** = la escena sigue (golpes, silencio, pasos, cámaras). **Abrir** = [E] en el cerrojo (abrir) y luego [E] en la puerta: se abre despacio, no hay nadie, la voz te habla al oído y la entidad se abalanza → muerte → **checkpoint F2 (despertar 00:18)**, sin perder el cap. 1.
- **Fusible (cap. 2):** al abrir el cuadro, el zócalo **F2** (abajo a la derecha, rótulo "F2 · QUEMADO") tiene un fusible ennegrecido. El repuesto está en la **caja de herramientas** del piso, junto al cuadro: [E] abre la tapa, [E] **Recoger fusible de repuesto**. Vuelve al cuadro: el rótulo cambia a "▼ F2 · CAMBIAR ▼" y parpadea; **clic sobre el fusible quemado** para cambiarlo. Luego ALARMA + CÁMARAS + CERRADURAS = 7.
- **Cintas (cap. 3):** A = cajón del medio de la cómoda; B = "Revisar debajo de la cama" (si tardas, el protagonista da 3 pistas); C = botiquín del baño (al lado del espejo), de pie con etiqueta "C — porche". El objetivo muestra **"Cintas encontradas: n/3"**.
- **Videograbadora:** [E] bajo la TV abre la interfaz. **Paso 1:** clic en cada cinta para verla (unos 15 s; se ve en la pantalla de la interfaz). **Paso 2 (con las 3 vistas):** clic en una cinta = seleccionarla (se ilumina) → clic en **1º / 2º / 3º** = colocarla; clic en una posición ocupada = retirarla; se pueden intercambiar. **REPRODUCIR SECUENCIA** comprueba. Solución: **C (SÁB 22:10) → A (SÁB 22:47) → B (DOM 01:24)**. Error = "SECUENCIA INCONSISTENTE" + pista (1ª fechas, 2ª horas y hechos, 3ª casi directa). Correcto = "SECUENCIA RECONSTRUIDA" → verdad 3/4 → evento de la TV. Salir: botón SALIR o [Esc].
- **Habitación 06 (cap. 4):** mensaje del desconocido → llave en el abrigo del perchero → cerradura del armario del pasillo → el armario se desliza → interruptor 5 "06" en el cuadro (apagar otra cosa para no pasar de 7) → puerta metálica. Si te atascas ~55 s en cada paso, el protagonista da una pista.
- **Captura:** la entidad se abalanza a la cámara (golpe + destello) y vuelves al inicio del tramo (checkpoints de la tabla de arriba).

## Mapa

```
                 ┌──────────────┬─────────┬───────────┐
                 │  DORMITORIO  │  BAÑO   │ HAB. 06   │  (06 = oculta detrás del
                 │ cama·escrit. │ espejo  │ (secreta) │   armario del pasillo)
                 │ armario      │ botiquín│           │
   ┌─────────────┴──puerta──────┴─puerta──┴─[ARMARIO]─┤
   │                    P A S I L L O                 │
   ├──────────────arco─────────────┬──puerta──────────┤
   │            SALA               │      COCINA      │
   │ TV+VHS · cómoda · perchero    ├paso   mesa       │
   │ monitor de cámaras            │ heladera·alacena │
   │ panel de alarma               │ cuadro eléctrico │
   └────────[PUERTA PRINCIPAL]─────┴──────────────────┘
```

## Puntos de control

| Tecla debug | Etapa (`Stage`) | Se guarda cuando… |
|---|---|---|
| F1 | `C1_Noche` | Nueva partida |
| F2 | `C2_Despertar` | Te duermes en la cama (00:18) |
| F3 | `C2_Camaras` | Termina la escena de la voz falsa en la puerta |
| F4 | `C2_Persecucion` | Justo antes del susto "No mires detrás de ti" |
| F5 | `C3_Cintas` | Sobrevives a la persecución 1 |
| F6 | `C3_Persecucion` | Antes de la persecución con tu voz |
| F7 | `C4_Protocolo` | Sobrevives a la persecución 2 |
| F8 | `C5_Final` | GUARDIÁN bloquea la casa (fin cap. 4) |

F9 reinicia el tramo actual · F10 panel de depuración · F11/F12 recorrido automático a final A/B · **Shift+F1..F5** abre directamente alarma / cámaras / cuadro / VHS / consola. Todo esto solo existe en el editor y en "Development Build".

---

## CAPÍTULO 1: Una noche normal (23:47)

| | |
|---|---|
| **Inicio** | Sentado frente a la PC. Mensaje de mamá: turno de noche, cerrar la casa y activar la alarma. |
| **Objetivo** | *Asegura la casa antes de dormir*: [ ] Apaga la computadora · [ ] Cierra la puerta principal · [ ] Cierra las ventanas (0/3) |
| **Dónde** | PC: escritorio del dormitorio. Puerta principal: pared sur de la sala. Ventanas: dormitorio, sala, cocina. |
| **Interacción** | E en cada cosa. La lista se marca según el estado REAL (si vuelves a abrir una ventana se desmarca). |
| **Terror** | Al cerrar la última ventana: golpes en el vidrio. Si es la del dormitorio y la miras, algo pálido se asoma un instante. |
| **Avanza cuando** | PC apagada + puerta cerrada + 3 ventanas cerradas. |
| **Errores posibles** | Abrir otra vez algo ya cerrado desmarca el punto (es intencional). |

**Activa la alarma (investigación)**
- Mamá: *"El código es el día en que cambiamos la cerradura."*
- Pista en el mundo: **recibo del cerrajero**, en la **cómoda de la sala, cajón de ARRIBA** (los cajones ahora son cajas reales: el recibo está dentro). Examinarlo (E) y **girarlo con el ratón**: fecha 23/08 y nota de mamá al dorso.
- Solución: panel de alarma junto a la puerta principal → **2 3 0 8** y **OK** (teclado 1-9 / C 0 OK, con números impresos e instrucciones en pantalla). Un código incorrecto da error y se puede reintentar sin límite.
- Pantalla: *ALARMA ACTIVADA · OCUPANTES: 2* → **Verdad 1/4** (automática).

**Prepárate para dormir**
- Opcional: **espejo del baño**: mirarlo unos segundos, darse vuelta y volver a mirarlo. El reflejo se mueve con retraso → **Verdad 2/4**.
- Terror: al pasar por el pasillo aparece una **figura al fondo**, la luz parpadea y desaparece. Llega un mensaje anónimo que se borra del historial.
- Avanza: E en la cama → *¿Intentar dormir?* → Sí. Fundido, reloj, **00:18**. **Checkpoint C2_Despertar.**

---

## CAPÍTULO 2: Alguien está dentro (00:18)

**A · El cerrojo**
- Objetivo: *Revisa el cerrojo sin hacer ruido*. Despiertas por pasos en el pasillo; mamá pregunta por el cerrojo interior.
- Ir agachado (Ctrl) a la puerta principal. Correr en la sala/pasillo provoca un golpe en la puerta (susto, no castigo).
- E sobre el **cerrojo** (pieza metálica junto a la manija).
- Terror: golpes, **voz de mamá** pidiendo que abras. Tu teléfono la llama solo; mamá contesta: *"Estoy en el trabajo. ¿Por qué me estás llamando?"*
- Objetivo *¿Mamá?*: **no abrir** el cerrojo ni la puerta (abrir = muerte → vuelve a C2_Despertar). Esperar: manija, silencio, pasos alrededor de la casa, golpes en la ventana de la sala.
- **Checkpoint C2_Camaras.**

**B · Las cámaras (investigación)**
- Objetivo: *Revisa las cámaras de seguridad* → monitor en el escritorio de la sala (pared este). Se abre la **pantalla de vigilancia grande** con botones CAM 01–04, ARCHIVO/DIRECTO y SALIR (también teclas 1-4, Espacio, Esc).
- CAM 02 (pasillo) muestra la **puerta del baño ABIERTA** con **REC** arriba. En la casa está cerrada.
- Objetivo: *Descubre por qué la cámara muestra algo distinto*. Ir al baño y comprobar (está cerrada), volver y pulsar **ARCHIVO/DIRECTO** → *ERROR: energía insuficiente · fusible F2*.

**C · La energía (puzzle)**
- Objetivo: *Restaura la energía de las cámaras*.
- Pistas: **nota de papá** junto al cuadro de la cocina: *"El cuadro aguanta 7. Alarma 3 · Cámaras 2 · Cerraduras 2 · Luces 3."*
- Objeto: **fusible** en la **caja de herramientas** del piso de la cocina (abrir la tapa).
- Interacción: colocar el fusible en el zócalo F2 → subir **ALARMA + CÁMARAS + CERRADURAS** (3+2+2 = 7) y bajar **LUCES**.
- Error: pasar de 7 hace saltar el diferencial (todo se apaga, se puede reintentar).
- Al quedar sin luces: **linterna** sobre la mesada (F para encenderla).

**D · La figura**
- Objetivo: *Revisa la transmisión en directo* → monitor → ARCHIVO/DIRECTO.
- Terror: en DIRECTO aparece una **figura** en el pasillo que no existe en la casa real; al volver al monitor está **más cerca**; se corta la señal. La puerta del baño se abre sola. Mensaje: **"No mires detrás de ti."** **Checkpoint C2_Persecucion.** Al girarte, la entidad está detrás (**jumpscare**).

**E · Persecución 1**
- Objetivo: *Escóndete*. Escondites: **armario del dormitorio**, **alacena de la cocina**.
- La entidad **se detiene y gira la cabeza** antes de perseguir. Si te ve, corre; rompe la línea de visión.
- En el armario: se para delante, respira y se va. Salir con ella delante = muerte.
- Dura ~70 s hasta que se retira al baño. Mensaje: *"Bien. Ahora sabes que está dentro."* **Checkpoint C3_Cintas.**
- Errores posibles: si se pierde la pista de dónde está, el sonido de pasos y respiración es 3D.

---

## CAPÍTULO 3: La noche que ya ocurrió (01:05)

- Tu teléfono **envía solo** "¿Quién eres?". Respuesta: *"Soy el que llegó a las 22:10. Papá grabó todo. Hay tres cintas."*
- Objetivo: *Encuentra las cintas de papá* (0/3).

| Cinta | Dónde | Muestra |
|---|---|---|
| A | Cómoda de la sala, cajón del medio | 22:47 · cocina: un chico se sirve agua |
| B | Debajo de tu cama (E: "Revisar debajo de la cama") | 01:24 · pasillo: alguien con tu ropa camina hacia la cámara (hora que aún no pasó) |
| C | Botiquín del espejo del baño (abrir la puerta del espejo) | 22:10 · porche: alguien idéntico a ti entra con llave |

- Terror: al juntar la segunda cinta, **platos en la cocina**; al entrar, un **cajón se cierra de golpe** detrás de ti.
- Ver las cintas: **videograbadora** bajo la TV de la sala, clic en la ranura.
- Puzzle: aparecen las cajas junto a la VHS; clic en **orden cronológico C → A → B** (22:10 → 22:47 → 01:24). Orden incorrecto: se reinicia la secuencia, sin castigo.
- GUARDIÁN: *"23:46 DORMITORIO · SUJETO 2 (SIN INGRESO)"* → **Verdad 3/4**.
- Opcional: **mirar la foto familiar** sobre la cómoda **después** del puzzle: ya no apareces → **Verdad 4/4**.
- **Checkpoint C3_Persecucion.** La TV muestra tu dormitorio: tú mismo, de pie, mirando a la cámara.
- Persecución 2 (~75 s): la entidad usa **tu voz**. No ir hacia la voz; esconderse o mantener distancia.
- Mensaje: *"La llave está en el abrigo de papá. El cuarto que no existe está detrás del armario del pasillo."* **Checkpoint C4_Protocolo.**

---

## CAPÍTULO 4: El protocolo GUARDIÁN (01:40)

1. Objetivo *El cuarto que no existe*. **Abrigo de papá** en el perchero junto a la puerta principal → **llave "06"**.
2. Llave en la **cerradura del armario del pasillo** → el armario se desliza → **puerta metálica 06**.
3. Cuadro eléctrico: el interruptor 5 ahora es **06 (carga 2)**. Hay que apagar otro para encenderlo (p. ej. Alarma + Cerraduras + 06 = 7).
4. Entrar a la 06. Documentos opcionales: plano, notas de papá, **manual GUARDIÁN** (explica ECO y cámara CONGELADA, clave para el final).
5. **Cinta de carrete** encima del archivador → colocarla en la **grabadora**. Grabación de papá: las copias aparecen dentro de una habitación sin cruzar puertas y no saben que son copias.
6. Bajar la **palanca roja de ENERGÍA AUXILIAR**.
7. Terror: los monitores muestran tu cama: **alguien con tu cara** se incorpora mirando a la cámara. Bloqueo de GUARDIÁN, luz roja de emergencia, **aparición en la puerta de la 06** (jumpscare).
8. GUARDIÁN: *"Ocupantes: dos. Autorizados: uno. Aísle al duplicado antes del amanecer."* **Checkpoint C5_Final.**

---

## CAPÍTULO 5: ¿A quién dejaste salir? (02:13)

1. Objetivo *Aísla al duplicado*. La entidad anda suelta por la casa a oscuras (linterna, escondites).
2. Cuadro de la cocina: **ALARMA + CÁMARAS + CERRADURAS = 7**. GUARDIÁN: *"Consola de aislamiento disponible."* La entidad se retira.
3. GUARDIÁN **sella la cocina** con persianas. Dentro, alguien con tu voz: *"¿Mamá? La puerta no abre… Yo vivo aquí."*
4. Volver a la **consola de la 06**. Comparar sensores (pantalla de la consola) con cámaras (monitores):

| Zona | Sensor | Cámara | Conclusión |
|---|---|---|---|
| Cocina | ● fijo | DIRECTO: se ve al otro chico | Hay alguien de verdad |
| Dormitorio | parpadea a ritmo fijo (ECO) | REC 23:46 congelada | Grabación: no hay nadie |

- **AISLAR COCINA** → Final A.
- **AISLAR DORMITORIO** → muerte (vuelve a C5_Final).
- **AISLAR 06** (tú mismo) → Final B. **Solo aparece con 3 o más verdades.**

## Finales

- **A · "Buenos días":** encierras al de la cocina; sales al amanecer y llega mamá. Corte a la cámara: *SUJETO 1 (AUTORIZADO) · AISLADO*. Encerraste al verdadero.
- **B · "Buenas noches" (verdadero):** te aíslas tú; eres el SUJETO 2. El hijo real sale; mamá llega. Último mensaje: *"Cuídalo, ma."*
- Ambos: título GOOD NIGHT, nombre del final, verdades x/4, créditos y vuelta al menú.

## Las 4 verdades

| # | Verdad | Dónde | ¿Se puede perder? |
|---|---|---|---|
| 1 | Alarma: OCUPANTES 2 | Cap. 1, al activar la alarma | No |
| 2 | Reflejo con retraso | Cap. 1, espejo del baño | Sí (opcional) |
| 3 | SUJETO 2 sin ingreso | Cap. 3, ordenar cintas | No |
| 4 | Ya no estás en la foto | Cap. 3, foto familiar tras las cintas | Sí (opcional) |

## Muertes

- Cap. 2: abrir el cerrojo o la puerta durante la voz falsa.
- Persecuciones: ser atrapado, o salir del escondite con la entidad delante.
- Cap. 5: aislar el dormitorio.

Al morir: pantalla GOOD NIGHT y reintento desde el último punto de control.

## Errores conocidos / qué vigilar

- Aviso de Unity *"Reduced additional punctual light shadows resolution"*: hay muchas luces con sombra; es solo rendimiento, no rompe nada.
- Si una persecución se siente injusta, ajustar `EnemyAI` (velocidades/visión) y la duración en `StoryC2.cs`/`StoryC3.cs`.
- Si un tester no encuentra algo, revisar primero si miró las pistas del teléfono (app Objetivos).

**Duración estimada** jugando normal: 30–40 minutos.
