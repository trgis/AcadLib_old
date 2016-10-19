﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace AcadLib
{
    public static class BrepExtensions
    {
        /// <summary>
        /// Определение контура для набора полилиний - объекдинением в регион и извлечением внешнего его контура.
        /// Должна быть запущена транзакция
        /// </summary>        
        public static Polyline3d GetExteriorContour(this List<Polyline> idsPl)
        {
            List<Region> colReg = new List<Region>();
            foreach (var pl in idsPl)
            {                
                if (pl == null || pl.Area == 0) continue;

                // Создание региона из полилинии
                var dbs = new DBObjectCollection();
                dbs.Add(pl);
                var dbsRegions = Region.CreateFromCurves(dbs);
                if (dbsRegions.Count > 0)
                {
                    Region r = (Region)dbsRegions[0];
                    colReg.Add(r);
                }                
            }

            // Объединение регионов
            Region r1 = colReg.First();
            foreach (var iReg in colReg.Skip(1))
            {
                r1.BooleanOperation(BooleanOperationType.BoolUnite, iReg);
            }            
            return GetRegionContour(r1);            
        }

        private static Polyline3d GetRegionContour(Region reg)
        {
            Polyline3d resVal = null;
            double maxArea = 0;
            Brep brep = new Brep(reg);
            foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
            {
                foreach (BoundaryLoop loop in face.Loops)
                {
                    if (loop.LoopType == LoopType.LoopExterior)
                    {
                        HashSet<Point3d> ptsHash = new HashSet<Point3d>();                                                
                        foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex vert in loop.Vertices)
                        {
                            if (!ptsHash.Any(p => p.IsEqualTo(vert.Point, Tolerance.Global)))
                            {
                                ptsHash.Add(vert.Point);
                            }                            
                        }
                        Point3dCollection pts = new Point3dCollection(ptsHash.ToArray());
                        var pl = new Polyline3d(Poly3dType.SimplePoly, pts, true);
                        if (pl.Area>maxArea)
                        {
                            resVal = pl;
                        }
                    }
                }
            }
            return resVal;
        }

        /// <summary>
        /// Объекдинение полилиний.
        /// Полилинии должны быть замкнуты!
        /// </summary>        
        /// <param name="over">Контур который должен быть "над" объединенными полилиниями. Т.е. контур этой полилинии вырезается из полученного контура, если попадает на него.</param>
        public static List<Polyline3d> Union (this List<Polyline> pls, Polyline over)
        {
            if (pls == null || pls.Count == 0) return null;
            List<Polyline3d> res;
            var regions = createRegion(pls);
            res = createPl(regions);
            return res;
        }       

        private static List<Region> createRegion (List<Polyline> pls)
        {
            List<Region> res = new List<Region>();
            var dbs = new DBObjectCollection();
            foreach (var pl in pls)
            {
                dbs.Add(pl);
            }
            var dbsRegions = Region.CreateFromCurves(dbs);
            foreach (var item in dbsRegions)
            {
                res.Add((Region)item);
            }
            return res;
        }

        private static Region createRegion (Polyline pl)
        {
            var dbs = new DBObjectCollection();
            dbs.Add(pl);
            var dbsRegs = Region.CreateFromCurves(dbs);
            return (Region)dbsRegs[0];
        }

        private static List<Polyline3d> createPl (List<Region> regions)
        {
            List<Polyline3d> res = new List<Polyline3d>();
            foreach (var r in regions)
            {
                var pl = GetRegionContour(r);
                res.Add(pl);
            }
            return res;
        }
    }    
}
