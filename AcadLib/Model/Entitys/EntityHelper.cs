﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using NetLib;

namespace AcadLib
{
    public static class EntityHelper
    {
        public static void AddEntityToCurrentSpace(this IEnumerable<Entity> ents, EntityOptions entityOptions)
        {
            if (ents == null || !ents.Any()) return;
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            using (doc.LockDocument())
            using (var t = db.TransactionManager.StartTransaction())
            {
                var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                foreach (var ent in ents)
                {
                    if (ent.Id != ObjectId.Null || ent.IsDisposed) continue;
                    if (!ent.IsWriteEnabled) ent.UpgradeOpen();
                    ent.SetOptions(entityOptions);
                    cs.AppendEntity(ent);
                    t.AddNewlyCreatedDBObject(ent, true);
                }
                t.Commit();
            }
        }

        public static void AddEntityToCurrentSpace(this IEnumerable<Entity> ents)
        {
            AddEntityToCurrentSpace(ents, null);
        }

        public static void AddEntityToCurrentSpace (this Entity ent)
        {            
            AddEntityToCurrentSpace(ent?.Yield(), null);            
        }

        public static void AddEntityToCurrentSpace(this Entity ent, EntityOptions entityOptions)
        {
            AddEntityToCurrentSpace(ent?.Yield(), entityOptions);
        }

        public static DBText CreateText (string text, Point3d pos, EntityOptions entityOptions= null)
        {
            var dbText = new DBText();
            dbText.TextString = text;
            dbText.Position = pos;
            dbText.SetOptions(entityOptions);
            return dbText;
        }
    }
}
