﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
#pragma warning disable 618

namespace AcadLib.XData
{
    public static class ExtDicHelper
    {
        public const string PikApp = "PIK";
        private static readonly RXClass rxRecord = RXObject.GetClass(typeof(Xrecord));
        private static readonly RXClass rxDBDic = RXObject.GetClass(typeof(DBDictionary));

        /// <summary>
        /// Получение записи XRecord словаря по имени.
        /// Если такая запись есть в словаре то возвращается ее id, а Data записи очищаются.
        /// То, что id точно XRecord, а не DBDictionary - не проверяется!!!
        /// Если нет, то создается новая если create = true.
        /// <param name="clear">Очищать ли Xrecord если она уже есть.</param>
        /// </summary>        
        public static ObjectId GetRec (ObjectId dicId, string key, bool create, bool clear)
        {
            var res = ObjectId.Null;
            if (!dicId.IsValidEx() || string.IsNullOrEmpty(key)) return res;
            using (var dic = dicId.Open(OpenMode.ForRead) as DBDictionary)
            {
                if (dic == null) return res;
                if (dic.Contains(key))
                {
                    res = dic.GetAt(key);
                    if (!clear) return res;
                    using (var xr = res.Open(OpenMode.ForWrite) as Xrecord)
                    {
                        if (xr != null)
                            xr.Data = null;
                    }
                }
                else if (create)
                {
                    using (var xRec = new Xrecord())
                    {
                        if (!dic.IsWriteEnabled)
                            dic.UpgradeOpen();
                        res = dic.SetAt(key, xRec);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Удаление словаря
        /// </summary>
        /// <param name="dicId">Словарь</param>
        /// <param name="dbo">объект</param>
        public static void DeleteDic (ObjectId dicId, DBObject dbo)
        {
            if (!dicId.IsValidEx()) return;
            using (var dic = dicId.Open(OpenMode.ForWrite) as DBDictionary)
            {
                if (dic != null)
                {
                    dic.Erase();
                }
            }
            // Проверить. Если больше нет словарей в объекте, то очистить словарь объекта.
            var dboExtDicId = GetDboExtDic(dbo, false);
            if (dboExtDicId.IsNull) return;
            using (var dboExtDic = dboExtDicId.Open(OpenMode.ForRead) as DBDictionary)
            {
                if (dboExtDic == null) return;                
                if (dboExtDic.Count == 0 || (dboExtDic.Count == 1 && dboExtDic.Contains(PikApp)))
                {
                    dboExtDic.UpgradeOpen();
                    dboExtDic.Erase();                    
                }
            }        
        }

        /// <summary>
        /// Получение вложенного словаря.
        /// Если create=true то если такой словарь существует - он очищается.
        /// </summary>
        /// <param name="dicId">Родительский словарь DBDictionary</param>
        /// <param name="key">Имя вложенного словаря - который нужно получить</param>
        /// <param name="create">Создавать если его нет.</param>
        /// <param name="clear">Очищать ли словарь если он уже есть</param>
        /// <returns>Id DBDictionary вложенного словаря</returns>
        public static ObjectId GetDic (ObjectId dicId, string key, bool create, bool clear)
        {
            var res = ObjectId.Null;
            if (!dicId.IsValidEx() || string.IsNullOrEmpty(key)) return res;
            using (var dic = dicId.Open(OpenMode.ForRead) as DBDictionary)
            {
                if (dic == null) return res;
                if (dic.Contains(key))
                {
                    res = dic.GetAt(key);
                    if (!clear) return res;
                    using (var resDic = res.Open(OpenMode.ForWrite) as DBDictionary)
                    {
                        if (resDic == null) return res;
                        foreach (var item in resDic)
                        {
                            using (var entry = item.Value.Open(OpenMode.ForWrite))
                            {
                                entry.Erase();
                            }
                        }
                    }
                }
                else if (create)
                {
                    using (var dicInner = new DBDictionary())
                    {
                        if (!dic.IsWriteEnabled)
                            dic.UpgradeOpen();
                        res = dic.SetAt(key, dicInner);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Получение id словаря расширенных данных объекта dbo.ExtensionDictionary
        /// </summary>
        /// <param name="dbo">Объект</param>
        /// <param name="create">Создавать словарь, если его нет</param>
        /// <returns>Id словаря ExtensionDictionary</returns>
        public static ObjectId GetDboExtDic (DBObject dbo, bool create)
        {
            var res = ObjectId.Null;
            if (dbo == null) return res;
            if (dbo.ExtensionDictionary.IsNull)
            {
                if (create)
                {
                    if (!dbo.IsWriteEnabled)
                        dbo.UpgradeOpen();
                    dbo.CreateExtensionDictionary();
                    res = dbo.ExtensionDictionary;
                }
            }
            else
            {
                res = dbo.ExtensionDictionary;
            }
            return res;
        }        

        /// <summary>
        /// Получение создержимого словаря
        /// </summary>
        /// <param name="dicId">Словарь DBDictionary</param>
        /// <returns>Содержимое словаря. Имя не заполняется!</returns>
        public static DicED GetDicEd (ObjectId dicId)
        {
            DicED res;
            if (!dicId.IsValidEx()) return null;

            using (var dic = dicId.Open(OpenMode.ForRead) as DBDictionary)
            {
                if (dic == null) return null;

                res = new DicED
                {
                    Inners = new List<DicED>(),
                    Recs = new List<RecXD>()
                };

                foreach (var item in dic)
                {   
                    if (item.Value.ObjectClass == rxRecord)
                    {
                        using (var xrec = item.Value.Open(OpenMode.ForRead) as Xrecord)
                        {
                            if (xrec == null) continue;
                            var values = xrec.Data.AsArray();
                            var rec = new RecXD { Name = item.Key, Values = values.ToList() };
                            res.Recs.Add(rec);
                        }
                    }
                    else if (item.Value.ObjectClass == rxDBDic)
                    {
                        var dicEdInner = GetDicEd(item.Value);
                        dicEdInner.Name = item.Key;
                        res.Inners.Add(dicEdInner);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Запись значений из RecED в DBDictionary
        /// В переданном родительском словаре создается вложенный словарь с именем DicED.Name
        /// </summary>        
        public static void SetDicED (ObjectId idDicParent, DicED edDic)
        {
            if (edDic == null) return;
            var dicId = GetDic(idDicParent, edDic.Name, true, true);
            if (!dicId.IsValidEx()) return;

            // Запись списка значений в XRecord
            if (edDic.Recs != null)
            {
                foreach (var item in edDic.Recs)
                {
                    SetRecXD(dicId, item);
                }
            }
            // Запись вложенных словарей
            if (edDic.Inners != null)
            {
                foreach (var item in edDic.Inners)
                {
                    SetDicED(dicId, item);
                }
            }
        }

        /// <summary>
        /// Создание записи XRecord в словаре dicId
        /// </summary>
        /// <param name="dicId">Словарь DBDictionary</param>
        /// <param name="rec">Запись XRecord</param>
        public static void SetRecXD (ObjectId dicId, RecXD rec)
        {
            if (rec?.Values == null || rec.Values.Count==0) return;
            var idXrec = GetRec(dicId, rec.Name, true, true);
            if (!idXrec.IsValidEx()) return;
            using (var xrec = idXrec.Open(OpenMode.ForWrite) as Xrecord)
            {
                using (var rb = new ResultBuffer(rec.Values.ToArray()))
                {
                    if (xrec != null) xrec.Data = rb;
                }
            }
        }
    }
}
