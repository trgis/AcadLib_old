﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

namespace AcadLib
{
    public static class CommandStart
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Start(Action<Document> action)
        {
            // определение имени команды по вызвавему методу и иего артрибуту CommandMethod;
            string command = string.Empty;
            try
            {
                var caller = new StackTrace().GetFrame(1).GetMethod();
                command = GetCallerCommand(caller);
            }
            catch { }      

            Logger.Log.StartCommand(command);
            CommandCounter.CountCommand(command);
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            try
            {
                action(doc);
            }
            catch (System.Exception ex)
            {
                if (!ex.Message.Contains(General.CanceledByUser))
                {
                    Logger.Log.Error(ex, command);
                }
                doc.Editor.WriteMessage(ex.Message);
            }
        }

        private static string GetCallerCommand(MethodBase caller)
        {
            if (caller == null) return "nullCallerMethod!?";
            var atrCom = (CommandMethodAttribute)caller.GetCustomAttribute(typeof(CommandMethodAttribute));
            return atrCom?.GlobalName;            
        }
    }
}
