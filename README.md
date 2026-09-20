# Game View Fullscreen

Muestra el Game view a **pantalla completa de verdad**: sin barra de título, sin pestaña y sin la
toolbar del Game (Display / Scale / Stats / Gizmos). Se ve el render y nada más.

No es el maximizar de `Shift+Space`, que agranda el panel *dentro* del editor y te deja el chrome
igual. Esto abre una GameView aparte en modo popup del tamaño del monitor.

## Atajo

| Tecla | Qué hace |
|---|---|
| `F11` | Entrar y salir |
| `Escape` | Salir |

También está en el menú, en **Kekspace › Game a Pantalla Completa**.

## Instalación

En Unity: **Window › Package Manager › + › Add package from git URL…** y pegá:

```
https://github.com/Gravixlsc/UP-GameViewFullscreen.git
```

Para fijar una versión, agregá el tag al final:

```
https://github.com/Gravixlsc/UP-GameViewFullscreen.git#1.0.2
```

> El repo es privado: Unity usa las credenciales de git de la máquina, así que hay que tener acceso
> al repo y git configurado con un credential helper (en Windows, el Git Credential Manager ya
> viene). Si el Package Manager tira un error de autenticación, probá `git ls-remote <url>` en una
> terminal: si eso pide credenciales, Unity va a fallar igual.

## Por qué abre una ventana nueva

Podría reusar la GameView que ya está dockeada, pero sacarla de su dock la deja fuera del layout al
volver: habría que re-acomodarla a mano cada vez. Una ventana nueva se abre y se cierra sin tocar tu
layout.

## Grabar: la Game Bar de Windows no sirve acá

Si grabás con `Win + Alt + R`, el video te va a mostrar **el editor de Unity**, no esta ventana. No es
un bug de la herramienta ni un problema de foco: es cómo elige la Game Bar qué capturar.

La Game Bar **no graba la ventana que tenés en foco, graba la ventana principal de la aplicación**.
Toma la ventana del frente, la resuelve a un proceso (acá `Unity.exe`) y captura la ventana principal
de ese proceso. La pantalla completa que abre esta herramienta es una ventana `WS_POPUP` *propiedad*
de la ventana principal del editor y sin entrada en la barra de tareas, así que para la Game Bar no
existe como aplicación: se queda con el editor.

Está verificado, no es una teoría: grabando con la ventana de pantalla completa al frente durante
toda la captura, la Game Bar guardó el archivo con el nombre de la ventana del editor y el video
muestra el editor (menús, Hierarchy, Project, Console).

Tampoco hay un arreglo limpio del lado de Unity. La ventana sí toma el foco del sistema, o sea que no
alcanza con hacerle click antes de grabar. Para que la Game Bar la viera como aplicación habría que
reescribirle los estilos nativos (`WS_EX_APPWINDOW`, sacarle el dueño) con P/Invoke sobre una ventana
que administra Unity: código Win32 solo para Windows y que se rompe con cualquier cambio del editor,
para arreglar un problema de grabación. No vale la pena.

**Grabá así:**

| Cómo | Detalle |
|---|---|
| **Recortes / Snipping Tool** | `Win + Shift + S`, cambiá al modo grabación y seleccioná el monitor entero. Graba una región de la pantalla, así que agarra cualquier cosa que se vea. Viene con Windows 11. |
| **OBS** | Fuente **Captura de pantalla** (Display Capture), *no* Captura de ventana. |

La Game Bar sigue siendo la opción cómoda para grabar un **build** (standalone o la pestaña del
navegador): ahí el juego sí es la ventana principal de su propia aplicación.

## Requisitos

Unity 6000.0 o superior. Es una herramienta de **editor**: el assembly está restringido a Editor, así
que no entra en los builds.
