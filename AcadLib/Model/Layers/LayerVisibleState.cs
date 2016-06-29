﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AcadLib.Layers
{
    /// <summary>
    /// Состояние слоев - для проверки видимости объектов на чертеже
    /// </summary>
    public class LayerVisibleState
    {
        Dictionary<string, bool> layerVisibleDict;

        /// <summary>
        /// Нужно создавать новый объект LayerVisibleState после возмоного изменения состояния слоев пользователем.
        /// </summary>
        /// <param name="db"></param>
        public LayerVisibleState (Database db)
        {
            layerVisibleDict = GetLayerVisibleState(db);
        }

        private Dictionary<string, bool> GetLayerVisibleState (Database db)
        {
            var res = new Dictionary<string, bool> ();
            var lt = db.LayerTableId.GetObject( OpenMode.ForRead) as LayerTable;
            foreach (var idLayer in lt)
            {
                var layer = idLayer.GetObject(OpenMode.ForRead) as LayerTableRecord;
                res.Add(layer.Name, !layer.IsOff && !layer.IsFrozen);
            }
            return res;
        }

        /// <summary>
        /// Объект на видим - не скрыт, не на выключенном или замороженном слое
        /// </summary>
        /// <param name="ent"></param>
        /// <returns></returns>
        public bool IsVisible (Entity ent)
        {
            bool res = true;
            if (!ent.Visible)
            {
                res = false;
            }
            else
            {
                // Слой выключен или заморожен                
                layerVisibleDict.TryGetValue(ent.Layer, out res);
            }
            return res;
        }
    }
}
