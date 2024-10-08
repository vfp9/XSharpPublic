﻿//
// Copyright (c) XSharp B.V.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.


USING System
USING System.Collections.Generic
USING System.Text

BEGIN NAMESPACE XSharp.VFP.UI

    /// <summary>
    /// Interface for most objects in the VFP UI
    /// </summary>

	INTERFACE IVFPObject
		PROPERTY BaseClass AS STRING GET
		PROPERTY Class as STRING GET
		PROPERTY ClassLibrary as STRING GET
		PROPERTY Comment AS STRING GET

		METHOD ReadExpression(cPropertyName AS STRING) AS STRING
		METHOD WriteExpression(cPropertyName AS STRING, cExpression AS STRING) AS STRING
		METHOD ReadMethod(cMethod as STRING) AS STRING
		METHOD WriteMethod(cMethodName AS STRING, cMethodText AS STRING ,lCreateMethod AS LOGIC, nVisibility AS LONG, cDescription AS STRING) AS VOID
		METHOD ResetToDefault(cPropertyName AS STRING) AS VOID

	END INTERFACE

    /// <summary>
    /// Interface for most objects that can contain a picture
    /// </summary>

    INTERFACE IVfPGraphics
		PROPERTY Picture AS STRING GET SET
	END INTERFACE

    /// <summary>
    /// Interface for most objects that can contain an image
    /// </summary>
	INTERFACE IVFPImage INHERIT IVfPGraphics, IVFPText
		PROPERTY DisabledPicture AS STRING GET SET
		PROPERTY DownPicture AS STRING  GET SET
		PROPERTY PictureMargin AS LONG GET SET
		PROPERTY PicturePosition AS LONG GET SET
		PROPERTY PictureSpacing AS LONG GET SET
    END INTERFACE

    /// <summary>
    /// Interface for most objects that can have a Help Context ID
    /// </summary>

	INTERFACE IVFPHelp
		PROPERTY HelpContextID AS LONG GET SET
		PROPERTY WhatsThisHelpID AS LONG GET SET
    END INTERFACE
    /// <summary>
    /// Interface for controls that can be dragged
    /// </summary>
	INTERFACE IVFPControl INHERIT IVFPObject, IVFPHelp
		METHOD Drag(nAction) AS USUAL CLIPPER
		PROPERTY DragIcon AS STRING GET SET
		PROPERTY DragMode AS LONG GET SET
		METHOD SetFocus as VOID STRICT
	END INTERFACE

    /// <summary>
    /// Interface for controls that can own other controls
    /// </summary>
	INTERFACE IVFPOwner INHERIT IVFPObject
		METHOD AddObject(cName, cClass , cOLEClass , aInit1, aInit2 ) AS USUAL CLIPPER
		METHOD NewObject(cObjectName, cClassName , cModule , cInApplication, eParameter1, eParameter2) AS USUAL CLIPPER
		METHOD RemoveObject(cObjectName AS STRING) AS LOGIC
		PROPERTY Objects as Collection GET
		PROPERTY ControlCount AS LONG  GET SET


	END INTERFACE

    /// <summary>
    /// Interface for most objects that contain a list of items
    /// </summary>

	INTERFACE IVFPList INHERIT IVFPControl
		PROPERTY BoundColumn AS LONG GET SET
		PROPERTY BoundTo AS LOGIC GET SET
		METHOD AddItem(cItem , nIndex , nColumn) AS VOID  CLIPPER
		METHOD AddListItem(cItem , nIndex , nColumn) AS VOID  CLIPPER

		METHOD Clear AS VOID CLIPPER
		PROPERTY DisplayValue AS USUAL GET SET

		PROPERTY FirstElement AS LONG GET SET
		METHOD IndexToItemID(nIndex as LONG) AS LONG

		PROPERTY ItemBackColor AS LONG GET SET
		PROPERTY ItemData AS LONG  GET SET
		PROPERTY ItemForeColor AS LONG GET SET
			//PROPERTY ItemIDData[nItemId as LONG] AS LONG GET SET
		METHOD ItemIDToIndex(nItemId as LONG) AS LONG

		PROPERTY ItemTips AS LOGIC  GET SET
		PROPERTY List AS ARRAY  GET SET
		PROPERTY ListCount AS LONG  GET
		PROPERTY ListIndex AS LONG  GET SET
			//PROPERTY ListItem[nRow as INT, nCol as INT] AS USUAL GET SET
		PROPERTY ListItemID AS LONG  GET SET
		METHOD RemoveItem(nIndex AS LONG) AS VOID
		METHOD RemoveListItem(nIndex AS LONG) AS VOID
		METHOD Requery AS VOID STRICT
		PROPERTY RowSource AS STRING GET SET
		PROPERTY RowSourceType AS LONG  GET SET

			//PROPERTY Selected[nItem AS LONG] AS LOGIC  GET SET
			//PROPERTY SelectedID[nItem AS LONG] AS LOGIC GET SET

		PROPERTY SelectedItemBackColor AS LONG  GET SET
		PROPERTY SelectedItemForeColor  AS LONG  GET SET

    END INTERFACE
    /// <summary>
    /// Interface for groups
    /// </summary>

	INTERFACE IVFPGroup INHERIT IVFPObject
		PROPERTY MemberClass AS STRING GET SET
		PROPERTY MemberClassLibrary AS STRING GET SET
	END INTERFACE

    /// <summary>
    /// Interface for Text Controls
    /// </summary>
	INTERFACE IVFPText
		PROPERTY DisabledBackColor AS LONG GET SET
		PROPERTY DisabledForeColor AS LONG GET SET
	END INTERFACE

    /// <summary>
    /// Interface for Editable Controls
    /// </summary>
	INTERFACE IVFPEditable
		PROPERTY SelectOnEntry AS LOGIC GET SET
	END INTERFACE


END NAMESPACE // XSharp.VFP.UI.Generated
