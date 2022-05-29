#region License LGPL 3
// Copyright ?£ukasz Œwi¹tkowski 2007?010.
// http://www.lukesw.net/
//
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security;
using System.Security.Permissions;

namespace SparkFormEditor
{
    internal static class NativeMethods
    {
        private static HandleRef HWND_TOPMOST = new HandleRef(null, new IntPtr(-1));

        internal static void AnimateWindow(Control control, int time, AnimationFlags flags)
        {
            try
            {
                SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                sp.Demand();
                Win32Api.AnimateWindow(new HandleRef(control, control.Handle), time, flags);
            }
            catch (SecurityException) { }
        }

        internal static void SetTopMost(Control control)
        {
            try
            {
                SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                sp.Demand();
                Win32Api.SetWindowPos(new HandleRef(control, control.Handle), HWND_TOPMOST, 0, 0, 0, 0, 0x13);
            }
            catch (SecurityException) { }
        }

        internal static int HIWORD(int n)
        {
            return (short)((n >> 16) & 0xffff);
        }

        internal static int HIWORD(IntPtr n)
        {
            return HIWORD(unchecked((int)(long)n));
        }

        internal static int LOWORD(int n)
        {
            return (short)(n & 0xffff);
        }

        internal static int LOWORD(IntPtr n)
        {
            return LOWORD(unchecked((int)(long)n));
        }

        private static bool? _isRunningOnMono;
        public static bool IsRunningOnMono
        {
            get
            {
                if (!_isRunningOnMono.HasValue)
                    _isRunningOnMono = Type.GetType("Mono.Runtime") != null;
                return _isRunningOnMono.Value;
            }
        }
    }
}