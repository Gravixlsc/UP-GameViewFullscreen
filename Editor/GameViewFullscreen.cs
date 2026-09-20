using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kekspace.EditorTools
{
    /// <summary>
    /// Muestra el juego a pantalla completa REAL: sin la barra de título de la ventana,
    /// sin la pestaña y sin la toolbar del Game view (Display / Scale / Stats / Gizmos).
    ///
    /// No maximiza el panel dentro del editor como hace Shift+Space: abre una GameView
    /// aparte en modo popup (una ventana sin ningún chrome) del tamaño del monitor. Por
    /// eso se ve el render y nada más.
    ///
    /// Atajo: F11 para entrar y para salir. Escape también sale.
    /// </summary>
    /// Se usa una GameView NUEVA en vez de la que ya está dockeada porque sacar la
    /// existente de su dock la deja fuera del layout al volver: habría que re-acomodarla
    /// a mano cada vez.
    public static class GameViewFullscreen
    {
        private const string MenuPath = "Kekspace/Game a Pantalla Completa _F11";

        private static EditorWindow _fullscreen;

        [MenuItem(MenuPath, priority = 100)]
        private static void Toggle()
        {
            if (_fullscreen != null) Salir();
            else Entrar();
        }

        /// <summary>El check del menú refleja si ya está abierta, y de paso mantiene el
        /// item siempre habilitado para que el atajo sirva como salida.</summary>
        [MenuItem(MenuPath, validate = true)]
        private static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath, _fullscreen != null);
            return true;
        }

        /// <summary>Assembly de UnityEditor. No se puede usar `typeof(Editor)` como atajo:
        /// choca con el namespace `Editor` del proyecto.</summary>
        private static Assembly EditorAssembly => typeof(EditorWindow).Assembly;

        private static void Entrar()
        {
            System.Type gameViewType = EditorAssembly.GetType("UnityEditor.GameView");
            if (gameViewType == null)
            {
                Debug.LogError("[GameViewFullscreen] No encuentro UnityEditor.GameView en esta " +
                               "version del editor.");
                return;
            }

            _fullscreen = ScriptableObject.CreateInstance(gameViewType) as EditorWindow;
            if (_fullscreen == null) return;

            OcultarToolbar(_fullscreen, gameViewType);

            // La ventana se muestra ANTES de fijar el rect: al crearse toma un tamaño por
            // defecto y recien despues acepta la posicion final.
            MostrarSinBordes(_fullscreen);
            AplicarRect(_fullscreen, RectDelMonitor());
            _fullscreen.Focus();

            RegistrarEscape(_fullscreen);

            // Si el editor recarga los scripts, la referencia estatica se pierde y la
            // ventana quedaria abierta sin forma de cerrarla con el atajo.
            AssemblyReloadEvents.beforeAssemblyReload -= Salir;
            AssemblyReloadEvents.beforeAssemblyReload += Salir;
        }

        private static void Salir()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= Salir;

            if (_fullscreen == null) return;
            _fullscreen.Close();
            Object.DestroyImmediate(_fullscreen);
            _fullscreen = null;
        }

        /// <summary>Ventana pelada, sin borde ni barra de titulo. `ShowMode.NoShadow` y
        /// `ShowPopupWithMode` son internal de UnityEditor, asi que van por reflection; si
        /// alguna version los cambia, `ShowPopup()` deja el mismo resultado con sombra.</summary>
        private static void MostrarSinBordes(EditorWindow window)
        {
            System.Type showMode = EditorAssembly.GetType("UnityEditor.ShowMode");
            MethodInfo showPopupWithMode = typeof(EditorWindow).GetMethod("ShowPopupWithMode",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            if (showMode != null && showPopupWithMode != null)
            {
                object noShadow = System.Enum.Parse(showMode, "NoShadow");
                showPopupWithMode.Invoke(window, new[] { noShadow, (object)true });
                return;
            }

            window.ShowPopup();
        }

        /// <summary>
        /// Coloca la ventana SIN que Unity le recorte el rect.
        ///
        /// `EditorWindow.position` no sirve para esto: su setter pasa por FitRectToScreen,
        /// que mete 4 px de margen por lado y ademas descuenta la barra de tareas, asi que
        /// por mas que se le pida el monitor entero devuelve el area de trabajo. Escribir
        /// en el `ContainerWindow` —la ventana del sistema operativo— se saltea ese ajuste
        /// y el rect queda tal cual se pide.
        /// </summary>
        private static void AplicarRect(EditorWindow window, Rect rect)
        {
            var inst = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            try
            {
                object host = window.GetType().GetField("m_Parent", inst)?.GetValue(window);
                object container = host?.GetType().GetProperty("window", inst)?.GetValue(host, null);
                PropertyInfo pos = container?.GetType().GetProperty("position", inst);

                if (pos != null)
                {
                    pos.SetValue(container, rect, null);
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[GameViewFullscreen] No pude escribir el rect en el " +
                                 "ContainerWindow (" + e.Message + "); va a quedar el area " +
                                 "de trabajo en vez del monitor completo.");
            }

            window.position = rect;   // Peor pero funcional: Unity lo ajusta al area util.
        }

        /// <summary>`showToolbar` es internal: sin esto quedaria la barra de Display/Scale/
        /// Stats/Gizmos arriba del render, que es justo lo que se quiere sacar.</summary>
        private static void OcultarToolbar(EditorWindow window, System.Type gameViewType)
        {
            PropertyInfo prop = gameViewType.GetProperty("showToolbar",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (prop != null && prop.GetSetMethod(true) != null)
            {
                prop.SetValue(window, false, null);
                return;
            }

            // Fallback por si en otra version la propiedad no esta: el campo de respaldo.
            FieldInfo field = gameViewType.GetField("m_showToolbar",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) field.SetValue(window, false);
            else Debug.LogWarning("[GameViewFullscreen] No pude ocultar la toolbar del Game " +
                                  "view: va a quedar visible arriba del render.");
        }

        /// <summary>Escape como segunda salida. Se engancha en TrickleDown para ganarle al
        /// IMGUI del Game view, que si no se come la tecla cuando esta en Play.</summary>
        private static void RegistrarEscape(EditorWindow window)
        {
            VisualElement root = window.rootVisualElement;
            if (root == null) return;

            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Escape) return;
                evt.StopPropagation();
                Salir();
            }, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// Monitor COMPLETO donde esta el mouse, barra de tareas incluida.
        ///
        /// Unity no expone los bounds de un monitor: lo mas cercano es el area de trabajo
        /// que devuelve FitRectToMouseScreen, ya recortada con 4 px de margen por lado y
        /// sin la franja de la barra de tareas. Asi que:
        ///
        ///  - Si el mouse esta en el monitor principal, se usa `Screen.currentResolution`,
        ///    que es el dato exacto.
        ///  - En cualquier otro monitor se infla el area de trabajo por el margen medido en
        ///    el principal. Pasarse por un pixel no cuesta nada —lo que sobra queda fuera
        ///    de la pantalla— mientras que quedarse corto deja un borde a la vista.
        /// </summary>
        private static Rect RectDelMonitor()
        {
            Resolution res = Screen.currentResolution;
            var pantallaPrincipal = new Rect(0f, 0f, res.width, res.height);

            Rect? areaMouse = AreaDeTrabajo(null);
            if (areaMouse == null) return pantallaPrincipal;
            Rect area = areaMouse.Value;

            // Contenida en el monitor principal => es el principal, y ahi sabemos el tamaño.
            if (area.xMin >= -1f && area.yMin >= -1f
                && area.xMax <= res.width + 1f && area.yMax <= res.height + 1f)
                return pantallaPrincipal;

            // Margen que Unity reserva, medido contra un dato conocido: el principal.
            float margen = 4f;
            Rect? areaPrincipal = AreaDeTrabajo(pantallaPrincipal.center);
            if (areaPrincipal != null)
                margen = Mathf.Max(0f, (res.width - areaPrincipal.Value.width) * 0.5f);

            return new Rect(area.x - margen, area.y - margen,
                            area.width + margen * 2f, area.height + margen * 2f);
        }

        /// <summary>Area de trabajo del monitor que contiene <paramref name="punto"/>, o la
        /// del monitor donde esta el mouse si se pasa null.</summary>
        private static Rect? AreaDeTrabajo(Vector2? punto)
        {
            System.Type cw = EditorAssembly.GetType("UnityEditor.ContainerWindow");
            if (cw == null) return null;

            var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            MethodInfo fit = punto == null
                ? cw.GetMethod("FitRectToMouseScreen", flags)
                : cw.GetMethod("FitRectToScreen", flags);
            if (fit == null) return null;

            // Las firmas cambian entre versiones: se arman segun lo que pidan.
            ParameterInfo[] ps = fit.GetParameters();
            var args = new object[ps.Length];
            args[0] = new Rect(0f, 0f, 10000f, 10000f);
            for (int i = 1; i < ps.Length; i++)
            {
                System.Type t = ps[i].ParameterType;
                if (t == typeof(bool)) args[i] = true;                    // forceCompletelyVisible
                else if (t == typeof(Vector2)) args[i] = punto ?? Vector2.zero;
                else args[i] = null;                                       // ContainerWindow
            }

            try
            {
                object r = fit.Invoke(null, args);
                if (r is Rect rect && rect.width > 0f && rect.height > 0f) return rect;
            }
            catch { /* version sin esta API: se cae al monitor principal */ }

            return null;
        }
    }
}
