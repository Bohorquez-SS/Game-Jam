# GOOD NIGHT: verificación de la entrega final

Revisión del **20/10/2026**. Referencia proporcionada: `GoodNightFinal.zip` (268.894.919 bytes).

SHA-256 del ZIP:

```text
dfa9c541dace8c640941ccc5c5ae444d71c62e07a3f7105656c2cb2b23c6798f
```

## Comparación inicial con GitHub

| Referencia | SHA verificado |
|---|---|
| `main` | `426bc2602459bd3b9f3d64076e3934753c4edb46` |
| `dev` | `1e6d70d6a64f61c8d533abd588606f0e3aa224a3` |

Los árboles de estas referencias eran idénticos, con **1.427 archivos versionados**. La comparación por ruta y contenido confirmó:

| Alcance | Archivos | Faltantes | Extras | Diferentes |
|---|---:|---:|---:|---:|
| `Assets` | 1.380 | 0 | 0 | 0 |
| `Packages` | 2 | 0 | 0 | 0 |
| `ProjectSettings` | 24 | 0 | 0 | 0 |
| `Tools` | 12 | 0 | 0 | 0 |

Por tanto, **el proyecto completo de Unity del ZIP final ya estaba publicado**. La documentación local del ZIP no se copió indiscriminadamente: se contrastó con el código y se corrigieron las referencias obsoletas.

## Integridad y estructura

- CRC del archivo ZIP completo: correcto, sin entradas corruptas detectadas.
- 1.406 archivos de proyecto: **123 C#, 1 escena, 1 prefab, 112 materiales, 115 PNG, 160 WAV, 68 FBX, 84 OBJ, 2 shaders**, además de metadatos y configuración.
- **704 `.meta` con GUID válidos y únicos**; sin activos o carpetas de Assets sin su `.meta`, ni metadatos huérfanos en el alcance revisado.
- Verificados los 115 PNG mediante decodificador y los 160 WAV mediante lectura de sus tramas; JSON de configuración válido.
- Las 87 voces indexadas tienen un hash FNV-1a consistente con su texto y el WAV correspondiente.
- Escena principal presente, sin referencias serializadas de script con `fileID: 0` ni marcadores de conflicto detectados. Esto no certifica todas las referencias o todos los comportamientos en ejecución.
- Unity **6000.3.26f1**, URP **17.3.0**, `Packages` y `ProjectSettings` conservados.

El ZIP incluye tres builds de Windows, respaldos, logs, capturas de revisión, `UserSettings` y temporales locales. Son salidas o archivos auxiliares del entorno de trabajo; no se añaden al repositorio para sustituir el código fuente. No se publica el ZIP.

## Corrección posterior a la comparación

La copia de entrega conservaba textos de una consola anterior. El código real de `StoryC5.cs` ya exigía restaurar la energía, regresar a 06, guardar `C5_Decision` y elegir entre dos identidades, sin un mínimo de verdades. Se alinean:

- `HintBook.cs`: entradas de tres niveles para los tres objetivos reales de C5, en español e inglés; palanca auxiliar situada en la pared este.
- `IsolationStation.cs`: ayuda, leyenda e instrucciones; se retira el texto de una rama de ayuda inalcanzable. Las acciones de los botones y sus condiciones se conservan.
- `HouseRooms.cs`, `StoryC4.cs` y la escena: manual GUARDIÁN y pista guardada. En la escena solo cambian `isolationDoc.textEs` y `textEn`; ningún otro componente o campo.
- `StoryC5.cs`, `EndingManager.cs` y `HouseActors.cs`: comentarios obsoletos; no cambia la lógica de capítulos o finales.
- `CreditsPanel.cs`: Kevin Mollo y lista de herramientas, conservando las licencias de recursos.
- README, recorrido, plan de pruebas, líneas de voz, registro de cambios y documentación de evolución/verificación.

Tras este ajuste hay **nueve archivos del núcleo intencionalmente distintos del ZIP**, todos C# o texto del manual serializado. Se conservan todos los archivos del núcleo, sus `.meta` y GUID, los recursos binarios, las configuraciones, los scripts de `Tools` y los diálogos con voz. No se declara identidad byte por byte del núcleo después de aplicar estas correcciones.

Comprobaciones del parche: delimitadores C# equilibrados, cambios de los capítulos/finales limitados a comentarios o literales, acciones de consola sin cambios, componente del manual comparado como YAML y comprobación de diferencias de Git sin errores de espacio o conflictos. Estas verificaciones **no equivalen a compilar C#**.

## Historial, diseño y trabajo pendiente

Los ocho commits originales y sus merges se conservan; consultar [Evolución del proyecto](EVOLUCION_DEL_PROYECTO.md). La rama de corrección es `Feature_PlayerMovement`, actualizada desde `dev`, con autor **Sebastián Condori** (`cqs0036301@est.univalle.edu`). El flujo de integración autorizado es `Feature_PlayerMovement` → `dev` → `main`, mediante merges que conservan los commits de desarrollo. El estado de publicación se comprueba en las referencias y Pull Requests de GitHub.

En [Miro](https://miro.com/app/board/uXjVEXDjUbM=/) se corrigieron 22 elementos y se añadieron tres secciones de herramientas, evolución y verificación. Se conservaron las ocho secciones anteriores y sus conexiones. En [Trello](https://trello.com/b/JUy925jS/good-night-desarrollo-del-juego) se crearon cinco tarjetas de referencia y ocho de etapas publicadas, sin asignaciones ni resultados de pruebas inventados.

**No se dispone de Unity en el entorno de revisión.** No se ejecutó la escena, no se compiló el proyecto ni se jugaron los finales. Quedan pendientes las pruebas de importación, presentación, audio, checkpoints, ambos finales y build Windows de [QA_PLAYTEST.md](../QA_PLAYTEST.md). El resultado permite confirmar integridad y coherencia estática, no funcionamiento perfecto del juego.
