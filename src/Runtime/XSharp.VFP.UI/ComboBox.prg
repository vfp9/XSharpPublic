﻿// ComboBox.prg
//
// Copyright (c) XSharp B.V.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.


USING System
USING System.Collections.Generic
USING System.Text
USING System.Windows.Forms
USING System.ComponentModel
USING System.Drawing
USING System.ComponentModel

BEGIN NAMESPACE XSharp.VFP.UI

	/// <summary>
    /// The VFP compatible ComboBox class.
    /// </summary>
	PARTIAL CLASS ComboBox INHERIT System.Windows.Forms.ComboBox

		// Common properties that all VFP Objects support
		#include "Headers/VFPObject.xh"

		CONSTRUCTOR(  ) STRICT
            SUPER()
            SELF:Size := Size{100,24}
			RETURN

#include ".\Headers\ControlProperties.xh"

#include ".\Headers\ControlSource.xh"

		PROPERTY DisplayCount AS LONG AUTO
		PROPERTY InputMask AS STRING AUTO
		PROPERTY NullDisplay AS String AUTO
		PROPERTY OLEDropTextInsertion AS LONG AUTO

		PROPERTY Picture AS STRING AUTO
		PROPERTY PictureSelectionDisplay  AS LONG AUTO
		PROPERTY ReadOnly AS LOGIC AUTO

		PROPERTY SelLength AS LONG GET SELF:SelectionLength SET SELF:SelectionLength := Value
		PROPERTY SelStart AS LONG GET SELF:SelectionStart SET SELF:SelectionStart := Value
		PROPERTY SelText AS STRING GET SELF:SelectedText  SET SelectedText  := Value
		PROPERTY SelectOnEntry AS LOGIC AUTO
		PROPERTY Style AS LONG AUTO


	END CLASS
END NAMESPACE // XSharp.VFP.UI
