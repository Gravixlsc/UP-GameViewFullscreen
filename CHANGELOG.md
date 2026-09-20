# Changelog

Este formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/)
y el versionado es [SemVer](https://semver.org/lang/es/).

## [1.0.2] - 2026-09-20

### Documentado
- Por qué la Game Bar de Windows graba el editor y no la ventana de pantalla completa, y con qué
  grabar en su lugar. Está verificado: la Game Bar captura la ventana principal de la aplicación,
  no la que tiene el foco. No hay arreglo limpio del lado de Unity.

### Arreglado
- `package.json` decía 1.0.0 cuando el último tag era 1.0.1. De acá en adelante el número del
  package y el del tag van juntos.

## [1.0.1] - 2026-09-20

### Arreglado
- Dos choques al instalarlo en un proyecto que ya tenía el script suelto.

## [1.0.0] - 2026-09-20

### Agregado
- Primera versión, extraída del proyecto Kekspace para poder reusarla en otros proyectos.
- Pantalla completa real del Game view, sin chrome del editor, con `F11` para entrar y salir y
  `Escape` para salir.
