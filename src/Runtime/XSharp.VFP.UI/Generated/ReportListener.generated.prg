
// Class ReportListener BaseClass   Reportlistener Class  Reportlistener
BEGIN NAMESPACE XSharp.VFP.UI
PARTIAL CLASS ReportListener  IMPLEMENTS IVFPObject
#include "VFPObject.xh"
PROPERTY ALLOWMODALMESSAGES AS USUAL AUTO
PROPERTY CALLADJUSTOBJECTSIZE AS USUAL AUTO
PROPERTY CALLEVALUATECONTENTS AS USUAL AUTO
METHOD CANCELREPORT AS USUAL CLIPPER
RETURN NIL
METHOD CLEARSTATUS AS USUAL CLIPPER
RETURN NIL
PROPERTY COMMANDCLAUSES AS USUAL AUTO
PROPERTY CURRENTDATASESSION AS USUAL AUTO
PROPERTY CURRENTPASS AS USUAL AUTO
METHOD DOMESSAGE AS USUAL CLIPPER
RETURN NIL
METHOD DOSTATUS AS USUAL CLIPPER
RETURN NIL
PROPERTY DYNAMICLINEHEIGHT AS USUAL AUTO
PROPERTY FRXDATASESSION AS USUAL AUTO
PROPERTY GDIPLUSGRAPHICS AS USUAL AUTO
METHOD GETPAGEHEIGHT AS USUAL CLIPPER
RETURN NIL
METHOD GETPAGEWIDTH AS USUAL CLIPPER
RETURN NIL
METHOD INCLUDEPAGEINOUTPUT AS USUAL CLIPPER
RETURN NIL
PROPERTY LISTENERTYPE AS USUAL AUTO
METHOD ONPREVIEWCLOSE AS USUAL CLIPPER
RETURN NIL
METHOD OUTPUTPAGE AS USUAL CLIPPER
RETURN NIL
PROPERTY OUTPUTPAGECOUNT AS USUAL AUTO
PROPERTY OUTPUTTYPE AS USUAL AUTO
PROPERTY PAGENO AS LONG AUTO
PROPERTY PAGETOTAL AS LONG AUTO
PROPERTY PREVIEWCONTAINER AS USUAL AUTO
METHOD PRINTCACHEDPAGES AS USUAL CLIPPER
RETURN NIL
PROPERTY PRINTJOBNAME AS USUAL AUTO
PROPERTY QUIETMODE AS USUAL AUTO
METHOD RENDER AS USUAL CLIPPER
RETURN NIL
PROPERTY SENDGDIPLUSIMAGE AS USUAL AUTO
METHOD SUPPORTSLISTENERTYPE AS USUAL CLIPPER
RETURN NIL
PROPERTY TWOPASSPROCESS AS USUAL AUTO
METHOD UPDATESTATUS AS USUAL CLIPPER
RETURN NIL

END CLASS
END NAMESPACE      