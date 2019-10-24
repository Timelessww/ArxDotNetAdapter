using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using DotNetARX;

[assembly: CommandClass(typeof(GetLine.Class2))]
[assembly: ExtensionApplication(typeof(GetLine.Class2))]
namespace GetLine
{
    public  class Class2:IExtensionApplication
    {


        public static Document doc = Application.DocumentManager.MdiActiveDocument;
        public static Database db = doc.Database;
        public static Editor ed = doc.Editor;
        public  static Point3d ptStart;
        public static Point3d ptEnd;
        public static Point3d IntePoint;

        [CommandMethod("JigCircle")]
    public static void JigCircle()
    {       

            PromptPointResult resultp = ed.GetPoint("\n指定第一个顶点");
            if (resultp.Status != PromptStatus.OK) return;
            Point3d basePt = resultp.Value;
            resultp = ed.GetCorner("\n指定另一个顶点", basePt);
            if (resultp.Status != PromptStatus.OK) return;
            Point3d cornerPt = resultp.Value;
            Point2d point1 = new Point2d(basePt.X, basePt.Y);
            Point2d point2 = new Point2d(cornerPt.X, cornerPt.Y);
            Polyline polyline = new Polyline();
            Transaction(polyline, point1, point2);

            Point3d point3d2 = new Point3d(basePt.X, cornerPt.Y, 0);//第一个顶点竖下对着的点
            Point3d point3d3 = new Point3d(cornerPt.X, basePt.Y, 0);//第一个顶点横着对着的点


            CreatePL();


            PromptEntityOptions m_peo = new PromptEntityOptions("\n请选择第一条曲线:");
            PromptEntityResult m_per = ed.GetEntity(m_peo);
            if (m_per.Status != PromptStatus.OK) { return; }
            ObjectId m_objid1 = m_per.ObjectId;

            m_peo = new PromptEntityOptions("\n请选择第二条曲线:");
            m_per = ed.GetEntity(m_peo);
            if (m_per.Status != PromptStatus.OK) { return; }
            ObjectId m_objid2 = m_per.ObjectId;

            using (Transaction m_tr = db.TransactionManager.StartTransaction())
            {
                Curve m_cur1 = (Curve)m_tr.GetObject(m_objid1, OpenMode.ForRead);
                Curve m_cur2 = (Curve)m_tr.GetObject(m_objid2, OpenMode.ForRead);

                Point3dCollection m_ints = new Point3dCollection();
                m_cur1.IntersectWith(m_cur2, Intersect.OnBothOperands, new Plane(), m_ints, IntPtr.Zero, IntPtr.Zero); //得出的所有交点在c1曲线上
                foreach (Point3d m_pt in m_ints)
                {
                    ed.WriteMessage("\n第一条曲线与第二条曲线交点:{0}", m_pt);

                    if (PtInPolygon1(m_pt, polyline) == 0)
                    { ed.WriteMessage($"\n该点在矩形上，{PtInPolygon1(m_pt, polyline)}"); }
                    else if (PtInPolygon1(m_pt, polyline) == 1)
                    { ed.WriteMessage($"\n该点在矩形内，{PtInPolygon1(m_pt, polyline)}"); }
                    else if (PtInPolygon1(m_pt, polyline) == -1)
                    { ed.WriteMessage($"\n该点在矩形外，{PtInPolygon1(m_pt, polyline)}"); }

                }

                m_tr.Commit();
            }


            demo();


            //    ed.WriteMessage($"第一个点:{basePt},第二个点:{point3d3},第三个点:{cornerPt},第四个点:{point3d2}," +
            //    $"第一个交点：{ PLL(basePt, point3d2, ptStart, ptEnd)}" +
            //    $"第二个交点：{PLL(basePt,point3d3,ptStart,ptEnd)}");



            //ed.WriteMessage($"端点:{PLL(basePt, point3d2, ptStart, ptEnd)},夹角：" +
            //    $"{Angle(PLL(basePt, point3d2, ptStart, ptEnd),basePt, PLL(basePt, point3d3, ptStart, ptEnd))}");


        }

        public static void CreatePL()//创建多段线
        {


            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("\n选择起点:")
            {
                // 提示起点
                Message = "\n选择起点: "
            };
            pPtRes = ed.GetPoint(pPtOpts);
             ptStart = pPtRes.Value;

            // 如果用户按ESC键或取消命令，就退出
            if (pPtRes.Status == PromptStatus.Cancel) return;
            // 提示终点
            pPtOpts.Message = "\n选择终点 ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptStart;
            pPtRes = ed.GetPoint(pPtOpts);
            ptEnd = pPtRes.Value;
            if (pPtRes.Status == PromptStatus.Cancel) return;
            Point3dCollection point3DCollection = new Point3dCollection
            {
                ptStart,
                ptEnd
            };
            TransactionPL(point3DCollection);

            GetKeywordPolyLine();



        }

        public static void GetKeywordPolyLine()//是否重复创建多段线
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("")
            {
                Message = "\n是否继续创建多段线 "
            };
            pKeyOpts.Keywords.Add("Y");
            pKeyOpts.Keywords.Add("N"); 
            pKeyOpts.AllowNone = true;
            PromptResult pKeyRes = doc.Editor.GetKeywords(pKeyOpts);
           
           
              if (pKeyRes.StringResult == "Y")
             {

                CreatePL();
             }
            else
            {

                    return;
             }
            

        }
        /// <summary>
        /// 根据余弦定理求两个线段夹角
        /// </summary>
        /// <param name="o">端点</param>
        /// <param name="s">start点</param>
        /// <param name="e">end点</param>
        /// <returns></returns>
      public  static  double Angle(Point3d o, Point3d s, Point3d e)
        {
            double cosfi = 0, fi = 0, norm = 0;
            double dsx = s.X - o.X;
            double dsy = s.Y - o.Y;
            double dex = e.X - o.X;
            double dey = e.Y - o.Y;

            cosfi = dsx * dex + dsy * dey;
            norm = (dsx * dsx + dsy * dsy) * (dex * dex + dey * dey);
            cosfi /= Math.Sqrt(norm);

            if (cosfi >= 1.0) return 0;
            if (cosfi <= -1.0) return Math.PI;
            fi = Math.Acos(cosfi);

            if (180 * fi / Math.PI < 180)
            {
                return 180 * fi / Math.PI;
            }
            else
            {
                return 360 - 180 * fi / Math.PI;
            }
        }

        public static  int PtInPolygon1(Point3d pt, Polyline pPolyline)//判断点是否在多边形内
        {
            int count = pPolyline.NumberOfVertices;
            // 记录是否在多边形的边上
            bool isBeside = false;
            //构建多边形外接矩形
            double maxx = double.MinValue, maxy = double.MinValue, minx = double.MaxValue, miny = double.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (pPolyline.GetPoint3dAt(i).X > maxx)
                {
                    maxx = pPolyline.GetPoint3dAt(i).X;
                }
                if (pPolyline.GetPoint3dAt(i).Y > maxy)
                {
                    maxy = pPolyline.GetPoint3dAt(i).Y;
                }
                if (pPolyline.GetPoint3dAt(i).X < minx)
                {
                    minx = pPolyline.GetPoint3dAt(i).X;
                }
                if (pPolyline.GetPoint3dAt(i).Y < miny)
                {
                    miny = pPolyline.GetPoint3dAt(i).Y;
                }
            }
            if (pt.X > maxx || pt.Y > maxy || pt.X < minx || pt.Y < miny)
            {
                return -1;
            }

            Line line1 = new Line(new Point3d(maxx, pt.Y, 0), new Point3d(minx, pt.Y, 0));

            int crossCount = 0;

            using (Transaction tran = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord pBlockTableRecord = tran.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                Point3dCollection crossPoint = new Point3dCollection();

                line1.IntersectWith(pPolyline, Intersect.OnBothOperands, crossPoint, IntPtr.Zero, IntPtr.Zero);
                if (crossPoint.Count >= 1)
                {
                    for (int n = 0; n < crossPoint.Count; n++)
                    {
                        Point3d crossPt = crossPoint[n];
                        if (crossPt.X > pt.X)
                        {
                            crossCount++;

                            //Circle pCircle = new Circle(crossPt, Vector3d.ZAxis, 2);

                            //pBlockTableRecord.AppendEntity(pCircle);
                            //tran.AddNewlyCreatedDBObject(pCircle, true);
                        }
                        else if(crossPt.X==pt.X)
                        {
                            isBeside = true;
                        }
                    }

                }
                //Circle circle = new Circle(pt, Vector3d.ZAxis, 2);
                //pBlockTableRecord.AppendEntity(circle);
                //tran.AddNewlyCreatedDBObject(circle, true);

                //pBlockTableRecord.AppendEntity(line1);
                //tran.AddNewlyCreatedDBObject(line1, true);
                tran.Commit();
            }
            if (isBeside)
            { return 0; }
            else if(crossCount % 2==1)
            
               return 1;
              return -1;
            


            }

        /// <summary>
        /// 求两两连线的交点
        /// </summary>
        /// <param name="P11">第一组点</param>
        /// <param name="P12">第一组点</param>
        /// <param name="P21">第二组点</param>
        /// <param name="P22">第二组点</param>
        /// <returns>若有交点就返回交点，否则返回P11</returns>
        public static  Point3d PLL(Point3d P11, Point3d P12, Point3d P21, Point3d P22)
        {
            double A1 = P12.Y - P11.Y;
            double B1 = P11.X - P12.X;
            double C1 = -A1 * P11.X - B1 * P11.Y;
            double A2 = P22.Y - P21.Y;
            double B2 = P21.X - P22.X;
            double C2 = -A2 * P21.X - B2 * P21.Y;
            double dlt = A1 * B2 - A2 * B1;
            double dltx = C1 * B2 - C2 * B1;
            double dlty = A1 * C2 - A2 * C1;
            if (Math.Abs(dlt) < 0.00000001)
            {
                return P11;
            }
            else
            {
                return new Point3d(-1.0 * (dltx / dlt), -1.0 * (dlty / dlt), 0);
              
            }
        }
        public static void TransactionPL(Point3dCollection point3DCollection)//生成多段线添加模型空间
        {

          
            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Polyline polyline1 = new Polyline();
                EntityHelper.CreatePolyline(polyline1, point3DCollection);
                btr.AppendEntity(polyline1);
                tr.AddNewlyCreatedDBObject(polyline1, true);
                tr.Commit();
                tr.Dispose();


            }

        }
        public static void Transaction(Polyline polyline,Point2d point1,Point2d point2)//将生成的矩形添加到模型空间
        {
           
            using (var tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                EntityHelper.CreateRectangle(polyline, point1, point2);
                btr.AppendEntity(polyline);
                tr.AddNewlyCreatedDBObject(polyline, true);
                tr.Commit();
                tr.Dispose();
            }

        }

  
        
            public  static  void demo()
            {

             
                PromptEntityOptions peo = new PromptEntityOptions("\n选择一条多段线: ");
                peo.SetRejectMessage("\n必须为多段线！");
                peo.AddAllowedClass(typeof(Polyline), true);
                PromptEntityResult per = ed.GetEntity(peo);
                if (per.Status != PromptStatus.OK)
                    return;
                using (var tr = db.TransactionManager.StartTransaction())
                {
                    Polyline pline = (Polyline)tr.GetObject(per.ObjectId, OpenMode.ForRead);

                    if (pline.Closed)
                    {
                        ed.WriteMessage("多段线闭合了.");
                        return;
                    }
                    var pdr = ed.GetDistance("\n指定偏移距离: ");
                    if (pdr.Status != PromptStatus.OK)
                        return;
                    var offsetCurves = pline.GetOffsetCurves(pdr.Value);
                    if (offsetCurves.Count != 1)
                    {
                        ed.WriteMessage("\n曲线创建偏移出现错误");
                        foreach (DBObject obj in offsetCurves) obj.Dispose();
                        return;
                    }
                    using (var polygon = (Polyline)offsetCurves[0])
                    {
                        //offsetCurves = pline.GetOffsetCurves(-pdr.Value);
                        //if (offsetCurves.Count != 1)
                        //{
                        //    ed.WriteMessage("\n曲线创建偏移出现错误");
                        //    foreach (DBObject obj in offsetCurves) obj.Dispose();
                        //    return;
                        //}
                        using (var curve = (Polyline)offsetCurves[0])
                        using (var line = new Line(polygon.EndPoint, curve.EndPoint))
                        {
                            polygon.JoinEntities(new Entity[] { new Line(polygon.EndPoint, curve.EndPoint), curve });
                            polygon.Closed = true;
                            var curSpace = (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                            curSpace.AppendEntity(polygon);
                            tr.AddNewlyCreatedDBObject(polygon, true);
                        }
                    }
                    tr.Commit();
                }
            }
 

    public class CircleJigger : EntityJig
    {
        private Circle _circle;
        public int step = 1;
        private Point3d _center = new Point3d();
        private double _radius = 0.0001;

        public CircleJigger(Circle circle)
            : base(circle)
        {
            _circle = circle;
            _circle.Center = _center;
            _circle.Radius = _radius;
        }

        protected override bool Update()
        {
            switch (step)
            {
                case 1:
                    _circle.Center = _center;
                    break;
                case 2:
                    _circle.Radius = _radius;
                    break;
                default:
                    return false;
            }

            return true;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            switch (step)
            {
                case 1:
                    JigPromptPointOptions prOptions1 = new JigPromptPointOptions("\n圆心:");
                    PromptPointResult prResult1 = prompts.AcquirePoint(prOptions1);
                    if (prResult1.Status == PromptStatus.Cancel)
                        return SamplerStatus.Cancel;

                    if (prResult1.Value.Equals(_center))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        _center = prResult1.Value;
                        return SamplerStatus.OK;
                    }
                case 2:
                    JigPromptDistanceOptions prOptions2 = new JigPromptDistanceOptions("\n半径:");
                    prOptions2.BasePoint = _center;
                    PromptDoubleResult prResult2 = prompts.AcquireDistance(prOptions2);
                    if (prResult2.Status == PromptStatus.Cancel)
                        return SamplerStatus.Cancel;

                    if (prResult2.Value.Equals(_radius))
                    {
                        return SamplerStatus.NoChange;
                    }
                    else
                    {
                        if (prResult2.Value < 0.0001)
                        {
                            return SamplerStatus.NoChange;
                        }
                        else
                        {
                            _radius = prResult2.Value;
                            return SamplerStatus.OK;
                        }
                    }
                default:
                    break;
            }

            return SamplerStatus.OK;
        }

        public static bool Jig()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Database db = doc.Database;

                Circle circle = new Circle();
                CircleJigger jigger = new CircleJigger(circle);
                PromptResult pr;
                do
                {
                    pr = doc.Editor.Drag(jigger);
                    jigger.step++;
                }
                while (pr.Status != PromptStatus.Cancel
                    && jigger.step <= 2);

                if (pr.Status != PromptStatus.Cancel)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        BlockTable bt = tr.GetObject(db.BlockTableId,
                            OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = tr.GetObject(
                            bt[BlockTableRecord.ModelSpace],
                            OpenMode.ForWrite) as BlockTableRecord;

                        btr.AppendEntity(jigger.Entity);
                        tr.AddNewlyCreatedDBObject(jigger.Entity, true);
                        tr.Commit();
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }






    public  void Initialize()
        {
            throw new NotImplementedException();
        }

        public  void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}