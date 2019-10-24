using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using GetLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using DotNetARX;

[assembly: CommandClass(typeof(GetLine.Class1))]
//[assembly: ExtensionApplication(typeof(GetLine.Class1))]
namespace GetLine
{
    public class Class1:IExtensionApplication
    {

        [CommandMethod("EcdLine")]
        public static void EcdLine()
        {
            
            
            // 获取当前数据库，启动事务管理器
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");
            // 提示起点
            pPtOpts.Message = "\n选择起点: ";
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;
           
            // 如果用户按ESC键或取消命令，就退出
            if (pPtRes.Status == PromptStatus.Cancel) return;
            // 提示终点
            pPtOpts.Message = "\n选择终点 ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptStart;
            pPtRes = acDoc.Editor.GetPoint(pPtOpts);
            Point3d ptEnd = pPtRes.Value;
            if (pPtRes.Status == PromptStatus.Cancel) return;
            // 启动事务
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl;
                BlockTableRecord acBlkTblRec;

                // 以写模式打开模型空间
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                // 创建直线
                Line acLine = new Line(ptStart, ptEnd);
                // 添加直线
                acBlkTblRec.AppendEntity(acLine);
                acTrans.AddNewlyCreatedDBObject(acLine, true);
                // 缩放图形到全部显示
                // acDoc.SendStringToExecute("._zoom _all ", true, false, false);
                // 提交修改，关闭事务
                acTrans.Commit();

                //var doc = Application.DocumentManager.MdiActiveDocument;
                //var editor = doc.Editor;
                //var db = doc.Database;
                //PromptEntityOptions promptEntityOptions = new PromptEntityOptions("请选择多段线");
                //PromptEntityResult promptEntity = editor.GetEntity(promptEntityOptions);
                //editor.WriteMessage(promptEntity);


            }

            }

        [CommandMethod("GetKeyword")]//关键字
        public static void GetKeywordFromUser2()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            PromptKeywordOptions pKeyOpts = new PromptKeywordOptions("");
            pKeyOpts.Message = "\nEnter an option ";
            pKeyOpts.Keywords.Add("Line");
            pKeyOpts.Keywords.Add("Circle");
            pKeyOpts.Keywords.Add("Arc");
            pKeyOpts.Keywords.Default = "Arc";
            pKeyOpts.AllowNone = true;
            PromptResult pKeyRes = acDoc.Editor.GetKeywords(pKeyOpts);
            Application.ShowAlertDialog("Entered keyword: " +
            pKeyRes.StringResult);
        }

        [CommandMethod("GetIntegerOrKeywordFromUser")]//提示用户输入一个非零的正整数或一个关键字
        public static void GetIntegerOrKeywordFromUser()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            PromptIntegerOptions pIntOpts = new PromptIntegerOptions("");
            pIntOpts.Message = "\nEnter the size or ";
            // 限制输入必须大于0
            pIntOpts.AllowZero = false;
            pIntOpts.AllowNegative = false;
            // 定义合法关键字并允许直接按Enter键
            pIntOpts.Keywords.Add("Big");
            pIntOpts.Keywords.Add("Small");
            pIntOpts.Keywords.Add("Regular");
            pIntOpts.Keywords.Default = "Regular";
            pIntOpts.AllowNone = true;
            // 获取用户键入的值
            PromptIntegerResult pIntRes = acDoc.Editor.GetInteger(pIntOpts);
            if (pIntRes.Status == PromptStatus.Keyword)
            {
                Application.ShowAlertDialog("Entered keyword: " +
                pIntRes.StringResult);
            }
            else
            {
                Application.ShowAlertDialog("Entered value: " +
                pIntRes.Value.ToString());
            }
        }
        /// <summary>
        /// 求两条曲线的交点,本函数为应对Polyline.IntersectWith函数的Bug
        /// Vrsion : 2009.02.10 Sieben
        /// Vrsion : 2010.12.25 增加判断输入实体是否为平面实体
        /// </summary>
        /// <param name="ent1"></param>
        /// <param name="ent2"></param>
        /// <returns></returns>
        public static Point3dCollection IntersectWith(Entity ent1, Entity ent2, Intersect intersectType)
        {
            try
            {
                if (ent1 is Polyline || ent2 is Polyline)
                {
                    if (ent1 is Polyline && ent2 is Polyline)
                    {
                        Polyline pline1 = (Polyline)ent1;
                        Polyline pline2 = (Polyline)ent2;
                        return IntersectWith(pline1.ConvertTo(false), pline2.ConvertTo(false), intersectType);
                    }
                    else if (ent1 is Polyline)
                    {
                        Polyline pline1 = (Polyline)ent1;
                        return IntersectWith(pline1.ConvertTo(false), ent2, intersectType);
                    }
                    else
                    {
                        Polyline pline2 = (Polyline)ent2;
                        return IntersectWith(ent1, pline2.ConvertTo(false), intersectType);
                    }
                }
                else
                {
                    Point3dCollection interPs = new Point3dCollection();
                    if (ent1.IsPlanar && ent2.IsPlanar)
                        ent1.IntersectWith(ent2, intersectType, new Plane(Point3d.Origin, Vector3d.ZAxis), interPs, 0, 0);
                    else
                        ent1.IntersectWith(ent2, intersectType, interPs, 0, 0);
                    return interPs;
                }
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}