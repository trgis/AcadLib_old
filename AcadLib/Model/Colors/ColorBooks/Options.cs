﻿using System;
using System.ComponentModel;
using System.IO;
using AcadLib.Files;
using Autodesk.AutoCAD.ApplicationServices;

namespace AcadLib.Colors
{
    [Serializable]
    public class Options
    {
        private static readonly string fileOptions = Path.Combine(
                       AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder,
                       "АР\\Палитры цветов\\ColorOptions.xml");
        //@"z:\AutoCAD_server\ShareSettings\АР\Палитры цветов\ColorOptions.xml";
        
        private static Options _instance;
        public static Options Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Load();
                }
                return _instance;
            }
        }

        private Options() { }
        
        /// <summary>
        /// Путь к файлу палитры NCS
        /// </summary>
        [Category("Главное")]
        [DisplayName("Файл палитры NCS")]
        [Description("Имя Excel-файла палитры цветов.")]
        [DefaultValue(@"z:\AutoCAD_server\ShareSettings\АР\Палитры цветов\Цвета NCS в RGB.xlsx")]
        public string NCSFile { get; set; } = @"z:\AutoCAD_server\ShareSettings\АР\Палитры цветов\Цвета NCS в RGB.xlsx";

        /// <summary>
        /// Ширина листа
        /// </summary>
        [Category("Размещение")]
        [DisplayName("Ширина листа")]
        [Description("Ширина листа.")]
        [DefaultValue(210)]
        public int Width { get; set; } = 210;

        /// <summary>
        /// Высота листа
        /// </summary>
        [Category("Размещение")]
        [DisplayName("Высота листа")]
        [Description("Высота листа.")]
        [DefaultValue(297)]
        public int Height { get; set; } = 297;

        /// <summary>
        /// Кол столбцов
        /// </summary>
        [Category("Размещение")]
        [DisplayName("Столбцов")]
        [Description("Столбцов.")]
        [DefaultValue(7)]
        public int Columns { get; set; } = 7;

        /// <summary>
        /// Рядов
        /// </summary>
        [Category("Размещение")]
        [DisplayName("Рядов")]
        [Description("Рядов.")]
        [DefaultValue(18)]
        public int Rows { get; set; } = 18;        

        public static Options Load()
        {
            Options options = null;
            // загрузка из файла настроек
            if (File.Exists(fileOptions))
            {
                var xmlSer = new SerializerXml(fileOptions);
                try
                {
                    options = xmlSer.DeserializeXmlFile<Options>();
                    if (options != null)
                    {
                        return options;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"Не удалось десериализовать настройки из файла {fileOptions}");
                }
            }
            options = new Options();
            options.Save();
            return options;
        }

        public void Save()
        {
            try
            {
                if (!File.Exists(fileOptions))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileOptions));
                }
                var xmlSer = new SerializerXml(fileOptions);
                xmlSer.SerializeList(this);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"Не удалось сериализовать настройки в {fileOptions}");
            }
        }

        //private static Options DefaultOptions()
        //{
        //   Options options = new Options();

        //   options.LogFileName = "AR_ExportApartment_Log.xlsx";
        //   options.BlockApartmentNameMatch = "квартира";

        //   return options;
        //}      

        public static void Show()
        {
            var formOpt = new FormOptions((Options)Instance.MemberwiseClone());
            if (Application.ShowModalDialog(formOpt) == System.Windows.Forms.DialogResult.OK)
            {
                _instance = formOpt.Options;
                _instance.Save();
            }
        }
    }
}