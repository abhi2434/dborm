using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.Common
{
    public enum AssemblyPathType
    {
        DefaultURL = 1,
        DefaultDirectory,
        CustomPath
    };
    public class ObjectFactory
    {
        #region Variables
        string msAssemblyPath = string.Empty;
        string msAssemblyName = string.Empty;
        object[] moConstructorParameters;
        string msClassName = string.Empty;
        AssemblyPathType moPathType = AssemblyPathType.DefaultDirectory;
        #endregion

        #region Constructors

        public ObjectFactory()
        {
            this.AssemblyName = "";
            this.PathType = AssemblyPathType.DefaultDirectory;
        }


        public ObjectFactory(string assemblyname, AssemblyPathType pathtype, string assemblypath, string classname, object[] constructorparameters)
        {
            this.AssemblyName = assemblyname;
            this.PathType = pathtype;
            this.AssemblyPath = assemblypath;
            this.ClassName = classname;
            this.ConstructorParameters = constructorparameters;
        }


        public ObjectFactory(string assemblyname, AssemblyPathType pathtype, string classname)
        {
            this.AssemblyName = assemblyname;
            this.PathType = pathtype;
            this.ClassName = classname;
        }


        #endregion

        #region Properties

        public AssemblyPathType PathType
        {
            get { return this.moPathType; }
            set { this.moPathType = value; }
        }

        public string AssemblyPath
        {
            get
            {
                switch (this.PathType)
                {
                    case AssemblyPathType.DefaultDirectory:
                        return Environment.CurrentDirectory.ToString();
                    case AssemblyPathType.CustomPath:
                        return this.msAssemblyPath;
                }
                return Environment.CurrentDirectory.ToString();
            }
            set { this.msAssemblyPath = value; }
        }


        public string AssemblyName
        {
            get { return this.msAssemblyName; }
            set { this.msAssemblyName = value; }
        }

        public object[] ConstructorParameters
        {
            get { return this.moConstructorParameters; }
            set { this.moConstructorParameters = value; }
        }

        public string ClassName
        {
            get { return this.msClassName; }
            set { this.msClassName = value; }
        }


        #endregion

        #region Functions

        public object GetTypedObject()
        {
            Assembly oAssembly = Assembly.LoadFrom(this.AssemblyPath + "\\" + this.AssemblyName);
            if (this.ConstructorParameters == null)
            {
                object oClassInstance = oAssembly.CreateInstance(this.ClassName, true);
                return oClassInstance;
            }
            else
            {
                object oClassInstance = oAssembly.CreateInstance(this.ClassName, true, BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.CreateInstance, null, this.ConstructorParameters, null, null);
                return oClassInstance;
            }

        }

        #endregion
    }
}
