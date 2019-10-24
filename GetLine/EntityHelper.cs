using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DotNetARX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetLine
{
    public static class EntityHelper
    {

        /// <summary>
        /// 创建矩形
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pt1">矩形的角点</param>
        /// <param name="pt2">矩形的角点</param>
        public static void CreateRectangle(this Polyline pline, Point2d pt1, Point2d pt2)
        {
            //设置矩形的4个顶点
            double minX = Math.Min(pt1.X, pt2.X);
            double maxX = Math.Max(pt1.X, pt2.X);
            double minY = Math.Min(pt1.Y, pt2.Y);
            double maxY = Math.Max(pt1.Y, pt2.Y);
            Point2dCollection pts = new Point2dCollection();
            pts.Add(new Point2d(minX, minY));
            pts.Add(new Point2d(minX, maxY));
            pts.Add(new Point2d(maxX, maxY));
            pts.Add(new Point2d(maxX, minY));
            pline.CreatePolyline(pts);
            pline.Closed = true;//闭合多段线以形成矩形
        }
        /// <summary>
        /// 通过三维点集合创建多段线
        /// </summary>
        /// <param name="pline">多段线对象</param>
        /// <param name="pts">多段线的顶点</param>
        public static void CreatePolyline(this Polyline pline, Point3dCollection pts)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                //添加多段线的顶点
                pline.AddVertexAt(i, new Point2d(pts[i].X, pts[i].Y), 0, 0, 0);
            }
        }
    }
}
