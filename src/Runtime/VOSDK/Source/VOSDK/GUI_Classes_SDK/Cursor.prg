/// <include file="Gui.xml" path="doc/Cursor/*" />
CLASS Cursor INHERIT VObject
	PROTECT oWnd AS window
	PROTECT oDim AS Dimension
	PROTECT oBitmap AS Bitmap
	PROTECT oPoint AS Point
	PROTECT lDimmed AS LOGIC
	PROTECT lCreated AS LOGIC
	PROTECT lVisible AS LOGIC


	//PP-030828 Strong typing
 /// <exclude />
	METHOD __Update(lCreate AS LOGIC) AS VOID STRICT
	//PP-030828 Strong typing


	IF lCreate
		IF !lCreated
			IF (oBitmap != NULL_OBJECT)
				CreateCaret(oWnd:Handle(), oBitmap:Handle(), 0, 0)
			ELSE
				CreateCaret(oWnd:Handle(), DWORD(_CAST,lDimmed), oDim:Width, oDim:Height)
			ENDIF
			lCreated := TRUE
			IF (oPoint != NULL_OBJECT)
				SELF:Position := oPoint
			ENDIF
		ENDIF
		IF lVisible
			ShowCaret(oWnd:Handle())
		ENDIF
	ELSE
		IF lCreated
			DestroyCaret()
			lCreated := FALSE
		ENDIF
	ENDIF
	RETURN




/// <include file="Gui.xml" path="doc/Cursor.Destroy/*" />
METHOD Destroy()  AS USUAL CLIPPER
	LOCAL oWndCursor AS Cursor


	IF lCreated
		DestroyCaret()
		lCreated := FALSE
	ENDIF


	IF !InCollect()
		UnregisterAxit(SELF)
		oWndCursor := oWnd:__Cursor
		IF (oWndCursor == SELF)
			oWnd:__Cursor := NULL_OBJECT
		ENDIF
		oWnd := NULL_OBJECT
		oDim := NULL_OBJECT
		oBitmap := NULL_OBJECT
		oPoint := NULL_OBJECT
	ENDIF


	RETURN NIL


/// <include file="Gui.xml" path="doc/Cursor.Handle/*" />
METHOD Handle(nHandleType)




	IF !IsNil(nHandleType)
		IF !IsLong(nHandleType)
			WCError{#Handle,#Cursor,__WCSTypeError,nHandleType,1}:Throw()
		ENDIF
	ENDIF


	RETURN oWnd:Handle()


/// <include file="Gui.xml" path="doc/Cursor.Hide/*" />
METHOD Hide()




	IF lCreated .AND. lVisible
		HideCaret(oWnd:Handle())
	ENDIF


	lVisible := FALSE


	RETURN SELF


/// <include file="Gui.xml" path="doc/Cursor.ctor/*" />
CONSTRUCTOR(oOwner, oObject, lDimmed)
	LOCAL oOldCursor AS Cursor


	SUPER()




	IF !(oOwner IS Window)
		WCError{#Init,#Cursor,__WCSTypeError,oOwner,1}:Throw()
	ENDIF


	oWnd := oOwner
	IF (oObject IS Dimension)
		oDim := oObject
		IF !IsNil(lDimmed)
			IF !IsLogic(lDimmed)
				WCError{#Init,#Cursor,__WCSTypeError,lDimmed,3}:Throw()
			ENDIF
			SELF:lDimmed := lDimmed
		ELSE
			SELF:lDimmed := TRUE
		ENDIF
		SELF:Position := Point{0,0}
	ELSE
		IF !(oObject IS Bitmap)
			WCError{#Init,#Cursor,__WCSTypeError,oObject,2}:Throw()
		ENDIF
		IF !IsNil(lDimmed)
			WCError{#Init,#Cursor,__WCSTypeError,lDimmed,3}:Throw()
		ENDIF
		oBitmap := oObject
		oDim := oBitmap:Size
	ENDIF


	// If Window already has caret destroy it
	oOldCursor := oWnd:__Cursor
	IF (oOldCursor != NULL_OBJECT)
		oOldCursor:__Update(FALSE)
	ENDIF


	// If Window has focus create caret now
	oWnd:__Cursor := SELF
	IF GetFocus() == oWnd:Handle()
		SELF:__Update(TRUE)
	ENDIF


	RETURN


/// <include file="Gui.xml" path="doc/Cursor.Position/*" />
ACCESS Position
	LOCAL strucPoint IS _WinPoint






	IF !lCreated
		RETURN oPoint
	ENDIF


	GetCaretPos( @strucPoint )


	RETURN oPoint := __WCConvertPoint(oWnd,Point{strucPoint:X,StrucPoint:Y})


/// <include file="Gui.xml" path="doc/Cursor.Position/*" />
ASSIGN Position(oNewPoint)
	LOCAL oTempPoint AS Point






	IF !(oNewPoint IS Point)
		WCError{#Position,#Cursor,__WCSTypeError,oNewPoint,1}:Throw()
	ENDIF


	oPoint := oNewPoint
	IF lCreated
		oTempPoint := __WCConvertPoint(oWnd,oPoint)
		SetCaretPos ( oTempPoint:X, oTempPoint:Y)
	ENDIF


	RETURN


/// <include file="Gui.xml" path="doc/Cursor.Show/*" />
METHOD Show()




	IF lCreated .AND. !lVisible
		ShowCaret (oWnd:Handle())
	ENDIF
	lVisible := TRUE


	RETURN SELF
END CLASS


