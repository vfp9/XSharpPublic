﻿// EditBox.prg
//
// Copyright (c) XSharp B.V.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.

USING System
USING System.Collections.Generic
USING System.Text
USING System.Windows.Forms
USING System.Drawing
USING System.ComponentModel

BEGIN NAMESPACE XSharp.VFP.UI
	/// <summary>
	/// The VFP compatible EditBox class.
	/// </summary>
	PARTIAL CLASS EditBox INHERIT TextBox

		/// <summary>
		/// Specifies that the EditBox should insert linefeed characters (CHR(10)) after carriage return characters
		/// (CHR(13)) within the text of an EditBox whenever the Value property is read or whenever the value is
		/// stored to the ControlSource.
		/// </summary>
		/// <value></value>
		// TODO: Implement AddLineFeeds
		PROPERTY AddLineFeeds AS LOGIC AUTO
		/// <summary>
		/// Specifies whether to allow tabs in an EditBox control.
		/// </summary>
		/// <value></value>
		// TODO: Implement AllowTabs
		PROPERTY AllowTabs AS LOGIC AUTO

		CONSTRUCTOR( )
			SUPER()
			SELF:Multiline := TRUE
            SELF:ScrollBars := ScrollBars.Both
            SELF:Size := Size{100,75}

	END CLASS

END NAMESPACE
