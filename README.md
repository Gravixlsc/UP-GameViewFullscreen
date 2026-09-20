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
https://github.com/Gravixlsc/UP-GameViewFullscreen.git#1.0.0
```

> El repo es privado: Unity usa las credenciales de git de la máquina, así que hay que tener acceso
> al repo y git configurado con un credential helper (en Windows, el Git Credential Manager ya
> viene). Si el Package Manager tira un error de autenticación, probá `git ls-remote <url>` en una
> terminal: si eso pide credenciales, Unity va a fallar igual.

## Por qué abre una ventana nueva

Podría reusar la GameView que ya está dockeada, pero sacarla de su dock la deja fuera del layout al
volver: habría que re-acomodarla a mano cada vez. Una ventana nueva se abre y se cierra sin tocar tu
layout.

## Requisitos

Unity 6000.0 o superior. Es una herramienta de **editor**: el assembly está restringido a Editor, así
que no entra en los builds.
