﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadLib
{
    public static class General
    {
        private static string userDataFolder;

        /// <summary>
        /// Отменено пользователем.
        /// Сообщение для исключения при отмене команды пользователем.
        /// </summary>
        public const string CanceledByUser = "Отменено пользователем";        

        /// <summary>
        /// Символы строковые
        /// </summary>
        public static class Symbols
        {
            /// <summary>
            /// Диаметр ⌀
            /// </summary>
            public const string Diam = "⌀";
            /// <summary>
            /// Кубическая степень- ³
            /// </summary>
            public const string Cubic = "³";
            /// <summary>
            /// Квадратная степень- ²
            /// </summary>
            public const string Square = "²";
        }

        /// <summary>
        /// Файл из папки пользовательских данных
        /// </summary>
        /// <param name="folderName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetUserDataFile (string folderName, string fileName)
        {
            if (userDataFolder == null)
            {
                userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), @"PIK\AutoCAD");
                if (!Directory.Exists(userDataFolder))
                    Directory.CreateDirectory(userDataFolder);
            }
            var folder = Path.Combine(userDataFolder, folderName);
            if (!Directory.Exists(folder))            
                Directory.CreateDirectory(folder);            
            var file = Path.Combine(folder, fileName);
            return file;
        }
    }
}
