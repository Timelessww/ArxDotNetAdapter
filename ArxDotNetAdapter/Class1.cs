using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(ArxDotNetAdapter.Class1))]

namespace ArxDotNetAdapter
{
    public class Class1
    {
        private Action cmd1;

        public Class1()
        {
            Reload();
        }

        [CommandMethod("reload1")]
        public void Reload()
        {
            var adapterFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var targetFilePath = Path.Combine(adapterFileInfo.DirectoryName, "GetLine.dll");

            var targetAssembly = Assembly.Load(File.ReadAllBytes(targetFilePath));
            var targetType = targetAssembly.GetType("GetLine.Class2");
            var targetMethod = targetType.GetMethod("JigCircle");
            var targetObject = Activator.CreateInstance(targetType);

            cmd1 = () => targetMethod.Invoke(targetObject, null);
        }

        [CommandMethod("mycmdgrp", "JigCircle", CommandFlags.UsePickSet)]
        public void Cmd1()
        {
            cmd1?.Invoke();
        }
    }
}
