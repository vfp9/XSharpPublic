﻿// OptionButton.prg
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
	/// The VFP compatible OptionButton class.
	/// </summary>
	PARTIAL CLASS OptionButton INHERIT System.Windows.Forms.RadioButton
		// Common properties that all VFP Objects support
		#include "Headers/VFPObject.xh"

	#include ".\XSharp\VFPProperties.xh"

		CONSTRUCTOR(  ) STRICT
            SUPER()
            SELF:Size := Size{10,16}


		#include ".\Headers\ControlProperties.xh"

		#include ".\Headers\FontProperties.xh"

		#include ".\Headers\ControlSource.xh"

	END CLASS

END NAMESPACE
