﻿namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices.Core;

    public static class SystemVariableExt
    {
        public static void SetSystemVariable(this string name, object value)
        {
            Application.SetSystemVariable(name, value);
        }

        public static void SetSystemVariableTry(this string name, object value)
        {
            try
            {
                Application.SetSystemVariable(name, value);
            }
            catch (Exception ex)
            {
                Logger.Log.Error($"SetSystemVariableTry name={name}, value={value}", ex);
            }
        }

        public static object GetSystemVariable(this string name)
        {
            return Application.GetSystemVariable(name);
        }
    }
}