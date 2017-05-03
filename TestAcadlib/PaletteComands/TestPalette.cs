﻿using AcadLib.PaletteCommands;
using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;

namespace TestAcadlib.PaletteComands
{
    public static class TestPalette
    {
        [CommandMethod("TestPaletteCommands")]
        public static void TestPaletteCommands ()
        {
            var commands = new List<IPaletteCommand>();

            var block1 = new PaletteInsertBlock("1", @"c:\temp\1.dwg", "Блок 1", null, "Вставка блока 1");
            commands.Add(block1);
            PaletteSetCommands.InitPalette(commands, "");
        }
    }
}
