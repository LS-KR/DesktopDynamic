using System;
using System.Runtime.InteropServices;
using BackgroundDynamic.Common.Helpers.Pinvoke;

//using Topshelf.Runtime.Windows;

namespace BackgroundDynamic {
    public static class WindowOperations {
        private const int LWA_ALPHA = 0x2;
        private const int LWA_COLORKEY = 0x1;

        public static void SetParentSafe(IntPtr child, IntPtr parent) {
            var ret = NativeMethods.SetParent(child, parent);
            if (ret.Equals(IntPtr.Zero)) {
                //LogUtil.LogWin32Error("Failed to set window parent");
            }
        }

        /// <summary>
        ///     Makes window toolwindow and force remove from taskbar.
        /// </summary>
        /// <param name="handle">window handle</param>
        public static void RemoveWindowFromTaskbar(IntPtr handle) {
            var styleCurrentWindowExtended = NativeMethods.GetWindowLongPtr(handle, (int)NativeMethods.GWL.GWL_EXSTYLE);

            var styleNewWindowExtended = styleCurrentWindowExtended.ToInt64() |
                                         NativeMethods.WindowStyles.WS_EX_NOACTIVATE |
                                         NativeMethods.WindowStyles.WS_EX_TOOLWINDOW;

            //update window styles
            //https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongptra
            //Certain window data is cached, so changes you make using SetWindowLongPtr will not take effect until you call the SetWindowPos function?
            NativeMethods.ShowWindow(handle, (int)NativeMethods.SHOWWINDOW.SW_HIDE);
            if (NativeMethods.SetWindowLongPtr(new HandleRef(null, handle), (int)NativeMethods.GWL.GWL_EXSTYLE,
                    (IntPtr)styleNewWindowExtended) == IntPtr.Zero) {
                //LogUtil.LogWin32Error("Failed to modify window style");
            }

            NativeMethods.ShowWindow(handle, (int)NativeMethods.SHOWWINDOW.SW_SHOW);
        }

        /// <summary>
        ///     Removes window border and some menuitems. Won't remove everything in apps with custom UI system.
        ///     <para>
        ///         Ref: https://github.com/Codeusa/Borderless-Gaming
        ///     </para>
        /// </summary>
        /// <param name="handle">Window handle</param>
        public static void BorderlessWinStyle(IntPtr handle) {
            // Get window styles
            var styleCurrentWindowStandard = NativeMethods.GetWindowLongPtr(handle, (int)NativeMethods.GWL.GWL_STYLE);
            var styleCurrentWindowExtended = NativeMethods.GetWindowLongPtr(handle, (int)NativeMethods.GWL.GWL_EXSTYLE);

            // Compute new styles (XOR of the inverse of all the bits to filter)
            var styleNewWindowStandard =
                styleCurrentWindowStandard.ToInt64()
                & ~(
                    NativeMethods.WindowStyles.WS_CAPTION // composite of Border and DialogFrame          
                    | (long)NativeMethods.WindowStyles.WS_THICKFRAME
                    | NativeMethods.WindowStyles.WS_SYSMENU
                    | NativeMethods.WindowStyles.WS_MAXIMIZEBOX // same as TabStop
                    | NativeMethods.WindowStyles.WS_MINIMIZEBOX // same as Group
                );


            var styleNewWindowExtended =
                styleCurrentWindowExtended.ToInt64()
                & ~(
                    NativeMethods.WindowStyles.WS_EX_DLGMODALFRAME
                    | (long)NativeMethods.WindowStyles.WS_EX_COMPOSITED
                    | NativeMethods.WindowStyles.WS_EX_WINDOWEDGE
                    | NativeMethods.WindowStyles.WS_EX_CLIENTEDGE
                    | NativeMethods.WindowStyles.WS_EX_LAYERED
                    | NativeMethods.WindowStyles.WS_EX_STATICEDGE
                    | NativeMethods.WindowStyles.WS_EX_TOOLWINDOW
                    | NativeMethods.WindowStyles.WS_EX_APPWINDOW
                );

            // update window styles
            if (NativeMethods.SetWindowLongPtr(new HandleRef(null, handle), (int)NativeMethods.GWL.GWL_STYLE,
                    (IntPtr)styleNewWindowStandard) == IntPtr.Zero) {
                //LogUtil.LogWin32Error("Failed to modify window style(1)");
            }

            if (NativeMethods.SetWindowLongPtr(new HandleRef(null, handle), (int)NativeMethods.GWL.GWL_EXSTYLE,
                    (IntPtr)styleNewWindowExtended) == IntPtr.Zero) {
                //LogUtil.LogWin32Error("Failed to modify window style(2)");
            }

            // remove the menu and menuitems and force a redraw
            var menuHandle = NativeMethods.GetMenu(handle);
            if (menuHandle != IntPtr.Zero) {
                var menuItemCount = NativeMethods.GetMenuItemCount(menuHandle);

                for (var i = 0; i < menuItemCount; i++)
                    NativeMethods.RemoveMenu(menuHandle, 0, NativeMethods.MF_BYPOSITION | NativeMethods.MF_REMOVE);
                NativeMethods.DrawMenuBar(handle);
            }
        }

        /// <summary>
        ///     Set window alpha.
        /// </summary>
        /// <param name="Handle"></param>
        public static void SetWindowTransparency(IntPtr Handle) {
            var styleCurrentWindowExtended = NativeMethods.GetWindowLongPtr(Handle, -20);
            var styleNewWindowExtended =
                styleCurrentWindowExtended.ToInt64() ^
                NativeMethods.WindowStyles.WS_EX_LAYERED;

            NativeMethods.SetWindowLongPtr(new HandleRef(null, Handle), (int)NativeMethods.GWL.GWL_EXSTYLE,
                (IntPtr)styleNewWindowExtended);
            NativeMethods.SetLayeredWindowAttributes(Handle, 0, 128, LWA_ALPHA);
        }
    }
}