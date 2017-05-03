﻿using Autodesk.AutoCAD.DatabaseServices;

namespace AcadLib.Extensions
{
    public static class EntityExt
    {
        /// <summary>
        /// Установка аннотативности объекту и масштаба с удалением текущего масштаба чертежа.
        /// </summary>
        /// <param name="ent">Объект поддерживающий аннотативность (текст, размер и т.п.)</param>
        /// <param name="scale">Масштаб в виде 100, 25 и т.п.</param>
        public static void SetAnnotativeScale(this Entity ent, int scale)
        {
            // Проверка, есть ли нужный масштаб в чертеже
            var nameScale = string.Format("1:{0}", scale);
            var ocm = ent.Database.ObjectContextManager;
            var occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");
            ObjectContext contextAnnoScale;
            if (!occ.HasContext(nameScale))
            {
                var annoScale = new AnnotationScale();
                annoScale.Name = nameScale;
                annoScale.PaperUnits = 1;
                annoScale.DrawingUnits = scale;
                occ.AddContext(annoScale);
                contextAnnoScale = annoScale;
            }
            else
            {
                contextAnnoScale = occ.GetContext(nameScale);
            }
            ent.Annotative = AnnotativeStates.True;
            ent.AddContext(contextAnnoScale);
            ent.RemoveContext(ent.Database.Cannoscale);
        }

        /// <summary>
        /// Видим на чертеже, слой включен и разморожен
        /// </summary>
        /// <param name="ent">Объект чертежа</param>
        /// <returns>Да - видим, Нет - не видим, слой выключен или заморожен</returns>
        public static bool IsVisibleLayerOnAndUnfrozen(this Entity ent)
        {
            var lt = ent.Database.LayerTableId.GetObject(OpenMode.ForRead) as LayerTable;
            var lay = lt[ent.Layer].GetObject(OpenMode.ForRead) as LayerTableRecord;
            return !lay.IsOff && !lay.IsFrozen && ent.Visible; // Слой включен и разморожен  и объект видимый
        }
    }
}
