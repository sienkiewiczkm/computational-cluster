﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.34209
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using ComputationalCluster.NetModule;
using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 


/// <uwagi/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.mini.pw.edu.pl/ucc/", IsNullable=false)]
public partial class Error : IMessage
{
    
    private ErrorErrorType errorTypeField;
    
    private string errorMessageField;
    
    /// <uwagi/>
    public ErrorErrorType ErrorType {
        get {
            return this.errorTypeField;
        }
        set {
            this.errorTypeField = value;
        }
    }
    
    /// <uwagi/>
    public string ErrorMessage {
        get {
            return this.errorMessageField;
        }
        set {
            this.errorMessageField = value;
        }
    }
}

/// <uwagi/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.mini.pw.edu.pl/ucc/")]
public enum ErrorErrorType {
    
    /// <uwagi/>
    UnknownSender,
    
    /// <uwagi/>
    InvalidOperation,
    
    /// <uwagi/>
    ExceptionOccured,
}
