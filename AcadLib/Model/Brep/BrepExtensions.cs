﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AcadLib.Geometry;

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

        public static Polyline3d GetRegionContour(this Region reg)
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
                        List<Point3d> ptsVertex = new List<Point3d>();                                                
                        foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex vert in loop.Vertices)
                        {
                            if (!ptsVertex.Any(p => p.IsEqualTo(vert.Point, Tolerance.Global)))
                            {
                                ptsVertex.Add(vert.Point);
                            }                            
                        }
                        Point3dCollection pts = new Point3dCollection(ptsVertex.ToArray());
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

        public static List<KeyValuePair<Polyline, BrepLoopType>> GetPolylines (this Region reg)
        {            
            var resVal = new List<KeyValuePair<Polyline, BrepLoopType>>(); ;
            Brep brep = new Brep(reg);
            foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
            {
                foreach (BoundaryLoop loop in face.Loops)
                {
                    List<Point2d> ptsVertex = new List<Point2d>();
                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex vert in loop.Vertices)                    
                        ptsVertex.Add(vert.Point.Convert2d());

                    var pl = ptsVertex.CreatePolyline();
                    resVal.Add(new KeyValuePair<Polyline, BrepLoopType>(pl, (BrepLoopType)loop.LoopType));
                }
            }
            return resVal;
        }

        public static List<KeyValuePair<Point2dCollection, BrepLoopType>> GetPoints2dByLoopType (this Region reg)
        {            
            var resVal = new List<KeyValuePair<Point2dCollection, BrepLoopType>>(); ;
            Brep brep = new Brep(reg);
            foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
            {
                foreach (BoundaryLoop loop in face.Loops)
                {
                    List<Point2d> ptsVertex = new List<Point2d>();
                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex vert in loop.Vertices)
                        ptsVertex.Add(vert.Point.Convert2d());                    
                    var pts2dCol = new Point2dCollection(ptsVertex.ToArray());
                    resVal.Add(new KeyValuePair<Point2dCollection, BrepLoopType>(pts2dCol, (BrepLoopType)loop.LoopType));
                }
            }
            return resVal;
        }

        public static List<Point3d> GetVertices (this Region reg)
        {
            var ptsVertex = new List<Point3d>();
            Brep brep = new Brep(reg);
            foreach (Autodesk.AutoCAD.BoundaryRepresentation.Face face in brep.Faces)
            {
                foreach (BoundaryLoop loop in face.Loops)
                {                    
                    foreach (Autodesk.AutoCAD.BoundaryRepresentation.Vertex vert in loop.Vertices)
                    {
                        ptsVertex.Add(vert.Point);
                    }
                }
            }
            return ptsVertex;
        }


        /// <summary>
        /// Объекдинение полилиний.
        /// Полилинии должны быть замкнуты!
        /// </summary>        
        /// <param name="over">Контур который должен быть "над" объединенными полилиниями. Т.е. контур этой полилинии вырезается из полученного контура, если попадает на него.</param>
        public static Region Union (this List<Polyline> pls, Region over)
        {
            if (pls == null || pls.Count == 0) return null;            
            var regions = createRegion(pls);
            var union = unionRegions(regions); 
            
            // Вырезание over региона
            if (over != null)
            {
                union.BooleanOperation(BooleanOperationType.BoolSubtract, over);
            }                                          
            return union;
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
            if (pl == null) return null;
            var dbs = new DBObjectCollection();
            dbs.Add(pl);
            var dbsRegs = Region.CreateFromCurves(dbs);
            return (Region)dbsRegs[0];
        }

        private static List<Polyline3d> createPl (List<Region> regions)
        {
            if (regions == null || regions.Count == 0) return null;
            List<Polyline3d> res = new List<Polyline3d>();
            foreach (var r in regions)
            {
                var pl = GetRegionContour(r);
                res.Add(pl);
            }
            return res;
        }

        private static Region unionRegions (List<Region> regions)
        {
            if (regions == null || regions.Count == 0) return null;
            if (regions.Count == 1) return regions[0];           

            var union = regions[0];            
            for (int i = 1; i < regions.Count; i++)
            {
                var cr = regions[i];
                union.BooleanOperation(BooleanOperationType.BoolUnite, cr);                
            }
            return union;
        }
    }    
}
