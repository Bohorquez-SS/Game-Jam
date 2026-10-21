# GOOD NIGHT: evolución comprobada del proyecto

Las ocho etapas son **versiones acumulativas del mismo juego**. Cada entrega reemplaza el estado anterior conservando el historial de Git; no son ocho funcionalidades independientes ni ocho sprints certificados.

Esta tabla documenta los commits de desarrollo publicados y su integración. «Publicado por» corresponde a la autoría registrada en Git; no atribuye por sí sola la creación de cada recurso o línea de código. Las cantidades A/M/D se calculan contra el padre de cada commit, sin detectar renombrados.

| Etapa | Publicado por | Fecha Git | Commit | Alcance comprobable | A/M/D | Integración |
|---|---|---|---|---|---|---|
| 01 | Kevin Mollo | 2026-10-19 | [79c9f9f](https://github.com/Bohorquez-SS/Game-Jam/commit/79c9f9f54d7ccb91cb3358fd1296621454471ece) | Base de Unity 6.3 URP y proyecto inicial | 509/1/0 | PR #1 |
| 02 | Sebastián Condori | 2026-10-20 | [4936f24](https://github.com/Bohorquez-SS/Game-Jam/commit/4936f24af98161e0c796b9ac7903dcea1161400c) | Reconstrucción del núcleo: jugador FPS, estaciones y capítulos | 518/119/18 | PR #2 |
| 03 | Santiago Borquez | 2026-10-20 | [6acfcb6](https://github.com/Bohorquez-SS/Game-Jam/commit/6acfcb664ce11332da81fe133c9dc5fa730ebe8e) | Importación, materiales URP y escena | 138/183/62 | Merge normal en dev; sin PR |
| 04 | David Bustos | 2026-10-20 | [1f0c07f](https://github.com/Bohorquez-SS/Game-Jam/commit/1f0c07f1497e7cf20b69e7a72e5c7f5388a0856d) | Correcciones de teléfono, escondites, texturas y herramientas de revisión | 74/141/0 | PR #3 |
| 05 | Kevin Mollo | 2026-10-20 | [8bd9909](https://github.com/Bohorquez-SS/Game-Jam/commit/8bd9909c2838842f1630ce28f0483edbb749db5b) | Ajustes de sala, estaciones, cajones y puzles | 12/28/0 | PR #4 |
| 06 | Sebastián Condori | 2026-10-20 | [ca105a8](https://github.com/Bohorquez-SS/Game-Jam/commit/ca105a8139112bcbb69d137e1d88f25647cf0dfe) | Correcciones de capítulos 2–4 y rediseño de habitación 06 | 16/16/0 | PR #5 |
| 07 | Santiago Borquez | 2026-10-20 | [1ea0aa5](https://github.com/Bohorquez-SS/Game-Jam/commit/1ea0aa517587bb08c6316ee52ec9ab950d6cdb71) | Nuevo flujo del capítulo 5, decisión de identidad y finales | 5/14/0 | PR #6 |
| 08 | David Bustos | 2026-10-20 | [89c326f](https://github.com/Bohorquez-SS/Game-Jam/commit/89c326fea334efa301295859ff405040121cc8ba) | Remaster audiovisual, créditos y documentación de entrega | 233/105/0 | PR #7 |

La etapa 03 se integró mediante un merge normal (`4d1f593`), no mediante un PR #3. El PR #3 corresponde a la etapa 04. Los números de PR son identificadores de GitHub y no números de turno.

## Ramas y entrega

- Kevin: `Feature_Scene`, etapas 01 y 05.
- Sebastián: `Feature_PlayerMovement`, etapas 02 y 06.
- Santiago: `Feature_Interaction`, etapas 03 y 07.
- David: `Feature_UI`, etapas 04 y 08.
- `dev`: integración acumulativa. `main`: entrega publicada.

En la comparación inicial del 20/10, `dev` estaba en `1e6d70d` y `main` en `426bc26`; sus árboles eran idénticos. Los ocho commits de desarrollo estaban presentes en ambos historiales. Las correcciones posteriores de documentación se preparan por separado y no se presentan como una nueva etapa de la jam.

## Organización actual y siguiente revisión

[Miro](https://miro.com/app/board/uXjVEXDjUbM=/) contiene concepto, espacios, capítulos, puzles, decisión final, mecánicas, controles y referencias de herramientas/evolución. [Trello](https://trello.com/b/JUy925jS/good-night-desarrollo-del-juego) reúne la documentación y las etapas publicadas; estas tarjetas retrospectivas no prueban que Trello se usara durante toda la producción.

Las tareas futuras requieren responsable, objetivo observable, criterios de aceptación, evidencia y revisión antes de cerrarse. No se inventan fechas de trabajo, reuniones Scrum, horas, sprints ni asignaciones retrospectivas. La ejecución de la [matriz de pruebas](../QA_PLAYTEST.md) sigue pendiente.
