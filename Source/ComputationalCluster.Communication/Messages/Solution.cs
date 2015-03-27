﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;
using ComputationalCluster.NetModule;

namespace ComputationalCluster.Communication.Messages
{

    // 
    // This source code was auto-generated by xsd, Version=4.0.30319.18020.
    // 


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.mini.pw.edu.pl/ucc/", IsNullable = false)]
    public partial class Solutions : IMessage
    {

        private string problemTypeField;

        private ulong idField;

        private string commonDataField;

        private SolutionsSolution[] solutions1Field;

        /// <remarks/>
        public string ProblemType
        {
            get
            {
                return this.problemTypeField;
            }
            set
            {
                this.problemTypeField = value;
            }
        }

        /// <remarks/>
        public ulong Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public string CommonData
        {
            get
            {
                return this.commonDataField;
            }
            set
            {
                this.commonDataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute("Solutions")]
        [System.Xml.Serialization.XmlArrayItemAttribute("Solution", IsNullable = false)]
        public SolutionsSolution[] Solutions1
        {
            get
            {
                return this.solutions1Field;
            }
            set
            {
                this.solutions1Field = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public partial class SolutionsSolution
    {

        private ulong taskIdField;

        private bool taskIdFieldSpecified;

        private bool timeoutOccuredField;

        private SolutionsSolutionType typeField;

        private ulong computationsTimeField;

        private string dataField;

        /// <remarks/>
        public ulong TaskId
        {
            get
            {
                return this.taskIdField;
            }
            set
            {
                this.taskIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TaskIdSpecified
        {
            get
            {
                return this.taskIdFieldSpecified;
            }
            set
            {
                this.taskIdFieldSpecified = value;
            }
        }

        /// <remarks/>
        public bool TimeoutOccured
        {
            get
            {
                return this.timeoutOccuredField;
            }
            set
            {
                this.timeoutOccuredField = value;
            }
        }

        /// <remarks/>
        public SolutionsSolutionType Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        public ulong ComputationsTime
        {
            get
            {
                return this.computationsTimeField;
            }
            set
            {
                this.computationsTimeField = value;
            }
        }

        /// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary")]
        public string Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.18020")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public enum SolutionsSolutionType
    {

        /// <remarks/>
        Ongoing,

        /// <remarks/>
        Partial,

        /// <remarks/>
        Final,
    }
}