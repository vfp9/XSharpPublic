﻿//
// Copyright (c) XSharp B.V.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.
//


using System
using System.Collections.Generic
using XSharp.RDD.SqlRDD
using System.Data.Common
using System.Reflection
using XSharp.RDD.Enums
using XSharp.RDD.Support
begin namespace XSharp.RDD.SqlRDD.Providers

/// <summary>
/// The Oracle provider class.
/// </summary>
class MySql inherit SqlDbProvider
    override property DllName as string => "MySql.Data.dll"
    override property TypeName as string => "MySql.Data.MySqlClient.MySqlClientFactory"

    override property GetIdentity            as string => "select LAST_INSERT_ID()"
    override property GetRowCount            as string => "select FOUND_ROWS( )"
    override property SelectTopStatement     as string => "select "+ColumnsMacro+" from "+TableNameMacro+" top "+TopCountMacro

    constructor()
        super("MySql")
        return
    private static aFuncs := null as Dictionary<string, string>
    override method GetFunctions() as Dictionary<string, string>
        if aFuncs == null
            aFuncs := Dictionary<string, string>{StringComparer.OrdinalIgnoreCase} {;
                {"LEFT(%1%,%2%)"			,"LEFT(%1%,%2%)"},;
                {"LOWER(%1%)"			    ,"LOWER(%1%)"},;
                {"UPPER(%1%)"			    ,"UPPER(%1%)"},;
                {"DTOS(%1%)"				,"DATE_FORMAT(%1%,'%Y%m%d')"},;
                {"DAY(%1%)"					,"DAY(%1%)"},;
                {"MONTH(%1%)"				,"MONTH(%1%)"},;
                {"YEAR(%1%)"				,"YEAR(%1%)"},;
                {"TODAY()"					,"SYSDATE()"},;
                {"CHR(%1%)"					,"CHAR(%1%)"},;
                {"LEN(%1%)"					,"LENGTH(%1%)"},;
                {"REPL(%1%,%2%)"			,"REPEAT(%1%,%2%)"},;
                {"ASC(%1%)"					,"ASCII(%1%)"},;
                {"TRIM(%1%)"				,"RTRIM(%1%)"},;
                {"ALLTRIM(%1%)"				,"TRIM(%1%)"},;
                {"+"						,"||"}}
        endif
        return aFuncs

   override method GetSqlColumnInfo(oInfo as RddFieldInfo) as string
       local sResult as string
       switch oInfo:FieldType
       case DbFieldType.Character
       case DbFieldType.VarChar
           sResult := i"[{QuoteIdentifier(oInfo.ColumnName)}] nvarchar ({oInfo.Length}) default ''"
           if oInfo:Flags:HasFlag(DBFFieldFlags.Nullable)
               sResult += " null "
           endif
      case DbFieldType.Integer
           sResult := i"{QuoteIdentifier(oInfo.ColumnName)} int "
           if oInfo:Flags:HasFlag(DBFFieldFlags.AutoIncrement)
               sResult += " AUTO_INCREMENT "
           else
               sResult += "default 0"
           endif
        case DbFieldType.Memo
             sResult := i"{QuoteIdentifier(oInfo.ColumnName)} TEXT "
        case DbFieldType.Blob
        case DbFieldType.General
        case DbFieldType.Picture
        case DbFieldType.VarBinary
            sResult := i"{QuoteIdentifier(oInfo.ColumnName)} BLOB "


       otherwise
           sResult := super:GetSqlColumnInfo(oInfo)
       end switch
       return sResult
end class
end namespace // XSharp.RDD.SqlRDD.SupportClasses