//
// Copyright (c) XSharp B.V.  All Rights Reserved.
// Licensed under the Apache License, Version 2.0.
// See License.txt in the project root for license information.
//

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using XSharpModel;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
namespace XSharp.Project
{
    /// <summary>
    /// This class extends the FileNode in order to represent a file
    /// within the hierarchy.
    /// </summary>
    public class XSharpFileNode : XFileNode
    {
        #region Fields
        private OAXSharpFileItem automationObject;

        #endregion
        #region Constructors

        static XSharpFileNode()
        {
            AddExtension(".vnfrm", KnownMonikers.FormInstance);
            AddExtension(".xsfrm", KnownMonikers.FormInstance);
            AddExtension(".vndbs", KnownMonikers.Database);
            AddExtension(".vnmnu", KnownMonikers.MainMenuControl);
            AddExtension(".xsmnu", KnownMonikers.MainMenuControl);
            AddExtension(".xsdbs", KnownMonikers.Database);
            AddExtension(".vnfs", KnownMonikers.ValidationRule);
            AddExtension(".xsfs", KnownMonikers.ValidationRule);
            AddExtension(".xssql", KnownMonikers.Database);
            AddExtension(".xsrep", KnownMonikers.Report);
            AddExtension(".vnsqs", KnownMonikers.Database);
            AddExtension(".vnrep", KnownMonikers.Report);
            AddExtension(".xaml", KnownMonikers.WPFFile);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XSharpFileNode"/> class.
        /// </summary>
        /// <param name="root">The project node.</param>
        /// <param name="e">The project element node.</param>
        internal XSharpFileNode(XSharpProjectNode root, ProjectElement element)
            : this(root, element, false)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="XSharpFileNode"/> class.
        /// </summary>
        /// <param name="root">The root <see cref="XSharpProjectNode"/> that contains this node.</param>
        /// <param name="element">The element that contains MSBuild properties.</param>
        /// <param name="isNonMemberItem">Flag that indicates if the file is not part of the project.</param>
        public XSharpFileNode(XSharpProjectNode root, ProjectElement element, bool isNonMemberItem)
            : base(root, element, isNonMemberItem)
        {
            this.UpdateHasDesigner();
            this.CheckItemType();
            root.AddURL(this.Url, this);

        }

        #endregion
        #region Properties

        internal string ItemType => this.ItemNode.ItemName;
        /// <summary>
        /// Gets an index into the default <b>ImageList</b> of the icon to show for this file.
        /// </summary>
        /// <value>An index into the default  <b>ImageList</b> of the icon to show for this file.</value>

        protected override bool SupportsIconMonikers
        {
            get
            {
                if (IsForm || IsUserControl || IsNonMemberItem)
                    return true;
#if DEV17
                if (!File.Exists(this.Url))
                    return true;
                return false;
#else
                return base.SupportsIconMonikers;
#endif
            }
        }
        protected override ImageMoniker GetIconMoniker(bool open)
        {
            if (IsForm)
                return KnownMonikers.WindowsForm;
            if (IsUserControl)
                return KnownMonikers.UserControl;
            if (IsNonMemberItem)
                return KnownMonikers.HiddenFile;
#if DEV17
            if (!File.Exists(this.Url))
                return KnownMonikers.MissingFile;
#endif
            return base.GetIconMoniker(open);

        }

        public override int ImageIndex
        {
            get
            {
                int ret = -1;
                if (SupportsIconMonikers)
                    return ret;
                if (!File.Exists(Url))
                {
                    return XSharpImageListIndex.MissingFile + XProjectNode.imageOffset;
                }
                ret = XSharpFileType.ImageIndex(this.Url);
                if (ret == -1)
                {
                    ret = base.ImageIndex;
                }
                return ret;
            }
        }

        #endregion
        protected override void DeleteFromStorage(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal); // make sure it's not readonly.
                OurNativeMethods.ShellDelete(path, OurNativeMethods.RecycleOption.SendToRecycleBin,
                   OurNativeMethods.UICancelOption.DoNothing, OurNativeMethods.FileOrDirectory.Directory);

            }
        }


        protected override int IncludeInProject()
        {
            int result = base.IncludeInProject();
            DetermineSubType();
            if (this.ProjectMgr is XSharpProjectNode prjNode)
            {
                prjNode.ProjectModel.AddFile(this.Url);
                prjNode.ResetDependencies();
            }
            return result;
        }

        private void ResetFileType()
        {
            DetermineFileType();
        }

        internal void BuildActionChanged( )
        {
            this.ResetFileType();
            var prjNode = this.ProjectMgr as XSharpProjectNode;
            prjNode.ProjectModel.RemoveFile(this.Url);
            prjNode.ProjectModel.AddFile(this.Url, this.FileType);
        }

        protected override int ExcludeFromProject()
        {
            return ThreadHelper.JoinableTaskFactory.Run(async delegate
         {
             await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
             //if (this.FileType == XFileType.SourceCode)
             {
                 var prjNode = this.ProjectMgr as XSharpProjectNode;
                 prjNode.ProjectModel.RemoveFile(this.Url);
             }
             return base.ExcludeFromProject();
         });
        }

        private static string TypeNameToSubtype(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "form":
                case "system.windows.forms.form":
                    return ProjectFileAttributeValue.Form;
                case "usercontrol":
                case "system.windows.forms.usercontrol":
                    return ProjectFileAttributeValue.UserControl;
            }
            return null;
        }

        internal void DetermineSubType()
        {
            // Parse the contents of the file and see if we have a windows form or a windows control
            XSharpProjectNode projectNode = ProjectMgr as XSharpProjectNode;
            this.ResetFileType();
            XSharpModel.XFile xFile = projectNode.ProjectModel.FindXFile(this.Url);
            if (xFile == null)
            {
                projectNode.ProjectModel.AddFile(this.Url);
                xFile = projectNode.ProjectModel.FindXFile(this.Url);
            }
            if (xFile != null)
            {
                xFile.ParseContents();
                // (something that inherits from system.windows.forms.form or system.windows.forms.usercontrol
                if (this.FileType == XFileType.SourceCode && this.Url.IndexOf(".designer.", StringComparison.OrdinalIgnoreCase) == -1)
                {
                    var classes = XDatabase.GetTypesInFile(xFile);
                    if (classes.Count > 0)
                    {
                        var first = classes.FirstOrDefault(c => c.TypeName != XLiterals.GlobalName);
                        if (first != null)
                        {
                            var parentClass = first.BaseTypeName;
                            SubType = TypeNameToSubtype(parentClass);
                            if (string.IsNullOrEmpty(SubType))
                            {
                                var usings = new List<string>();
                                if (xFile != null)
                                {
                                    usings.AddRange(xFile.Usings);
                                    usings.AddRange(xFile.StaticUsings);
                                }
                                var mgr = this.ProjectMgr as XSharpProjectNode;
                                var type = mgr.ResolveExternalType(parentClass, usings);
                                if (type != null)
                                {
                                    while (type?.BaseType != null)
                                    {
                                        var btName = type.BaseTypeName;
                                        SubType = TypeNameToSubtype(btName);
                                        if (!string.IsNullOrEmpty(SubType))
                                            break;
                                        type = mgr.ResolveExternalType(btName, usings);
                                    }
                                }
                                else
                                {
                                    var xType = mgr.ResolveXType(parentClass, usings);
                                    if (xType != null)
                                    {
                                        while (xType != null && !string.IsNullOrEmpty(xType.ParentName))
                                        {
                                            var parent = xType.ParentName;
                                            SubType = TypeNameToSubtype(parent);
                                            if (!string.IsNullOrEmpty(SubType))
                                                break;
                                            xType = mgr.ResolveXType(parent, usings);
                                        }
                                    }
                                }

                            }
                            if (SubType != null && this.ItemNode.GetMetadata(ProjectFileConstants.SubType) != SubType)
                            {
                                this.ItemNode.SetMetadata(ProjectFileConstants.SubType, SubType);
                                this.ItemNode.RefreshProperties();
                                this.UpdateHasDesigner();
                                this.ReDraw(UIHierarchyElement.Icon);
                            }
                        }
                    }
                }
            }
            if (this.FileType == XFileType.TextTemplate)
            {
                this.ItemNode.SetMetadata(ProjectFileConstants.Generator, "TextTemplatingFileGenerator");
                this.ItemNode.RefreshProperties();
            }
            return;
        }


        public void UpdateHasDesigner()
        {
            HasDesigner = XSharpFileType.HasDesigner(this.Url, SubType);
        }

        #region Dependent Items
        internal String GetParentName()
        {
            // There needs to be a better way to handle this
            // CS uses a table with
            // Parent extension, Allowed child extensions
            // look at HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\9.0\Projects\{FAE04EC0-301F-11d3-BF4B-00C04F79EFBC}\RelatedFiles
            // .xaml .xaml.cs
            // .cs   .designer.cs, .resx
            // .xsd  .cs, .xsc, .xss, .xsx
            // .resx files (like Resources.resx) seem to be handled differently..
            // we can hard code a similar table or read it from the registry like C# does.
            // CS also defines a 'relationtype'. See the CS Project System source code.
            String path = Path.GetFileName(this.Url).ToLowerInvariant();
            String folder = Path.GetDirectoryName(this.Url) + "\\";
            XSharpProjectNode project = this.ProjectMgr as XSharpProjectNode;
            int relationIndex = path.IndexOf(".");
            switch (this.FileType)
            {
                case XFileType.Header:
                case XFileType.ManagedResource:
                    path = Path.ChangeExtension(path, ".prg");
                    if (project.FindURL(folder + path) == null)
                    {
                        // this could be Form1.Language.resx
                        // so strip the last part and try again
                        if (relationIndex >= 0)
                        {
                            path = path.Substring(0, relationIndex) + ".prg";
                            if (project.FindURL(folder + path) == null)
                                path = null;
                        }
                        else
                        {
                            path = null;
                        }
                    }
                    break;
                case XFileType.VODBServer:
                case XFileType.VOFieldSpec:
                case XFileType.VOForm:
                case XFileType.VOIndex:
                case XFileType.VOMenu:
                case XFileType.VOOrder:
                case XFileType.VOSqlTable:
                case XFileType.VOReport:
                case XFileType.NativeResource:
                    if (relationIndex >= 0)
                    {
                        path = path.Substring(0, relationIndex) + ".prg";
                        if (project.FindURL(folder + path) == null)
                            path = null;
                    }
                    else
                        path = null;
                    break;
                default:
                    if (path.EndsWith(".designer.prg"))
                    {
                        // could be Form.Prg
                        // Resources.resx
                        // Settings.Settings
                        path = path.Substring(0, relationIndex);
                        string parent = folder + path + ".prg";
                        if (project.FindURL(parent) != null)
                            return parent;
                        parent = folder + path + ".resx";
                        if (project.FindURL(parent) != null)
                            return parent;
                        parent = folder + path + ".settings";
                        if (project.FindURL(parent) != null)
                            return parent;
                        return "";
                    }
                    else if (path.EndsWith(".xaml.prg"))
                    {
                        path = path.Substring(0, relationIndex) + ".xaml";
                        if (project.FindURL(folder + path) == null)
                            path = null;
                    }
                    else
                    {
                        path = string.Empty;
                    }
                    break;
            }
            if (!string.IsNullOrEmpty(path))
            {
                string dir = Path.GetDirectoryName(this.Url);
                if (dir.EndsWith("\\resources"))
                {
                    dir = dir.Substring(0, dir.Length - "\\resources".Length);
                }
                path = dir + "\\" + path;
            }
            return path;
        }
        public override object GetProperty(int propId)
        {
            if (propId == (int)__VSHPROPID8.VSHPROPID_DiagHubLanguage)
                return Constants.Product;
            return base.GetProperty(propId);
        }


        /// <summary>
        /// This method is used to move dependent items from the main level (1st level below project node) to
        /// and make them children of the modules they belong to.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal bool AddDependent(HierarchyNode child)
        {
            // If the file is not a XSharpFileNode then drop it and create a new XSharpFileNode
            XSharpFileNode dependent;
            String fileName = child.Url;
            try
            {
                child.Remove(false);
            }
            catch (Exception e)
            {
                Logger.Exception(e, "AddDependent failed");
            }
            dependent = (XSharpFileNode)ProjectMgr.CreateDependentFileNode(fileName);

            // Like the C# project system we do not put a path in front of the parent name, even when we are in a subfolder
            // but we do put a path before the parent name when the parent is in a different folder
            // In that case the path is the path from the base project folder
            string parent = this.ItemNode.GetMetadata(ProjectFileConstants.Include);
            parent = Path.GetFileName(parent);
            if (!this.IsNonMemberItem)
            {
                string parentPath = Path.GetDirectoryName(Path.GetFullPath(this.Url));
                string childPath = Path.GetDirectoryName(Path.GetFullPath(dependent.Url));
                if (string.Equals(parentPath, childPath, StringComparison.OrdinalIgnoreCase))
                {
                    dependent.ItemNode.SetMetadata(ProjectFileConstants.DependentUpon, parent);
                }
                else
                {
                    string projectPath = this.ProjectMgr.ProjectFolder;
                    Uri projectFolder = new Uri(projectPath);
                    Uri relative = projectFolder.MakeRelativeUri(new Uri(parentPath));
                    parentPath = relative.ToString() + Path.DirectorySeparatorChar;
                    dependent.ItemNode.SetMetadata(ProjectFileConstants.DependentUpon, parentPath + parent);
                }
            }
            // Make the item a dependent item
            dependent.HasParentNodeNameRelation = true;
            // Insert in the list of children
            dependent.NextSibling = this.FirstChild;
            this.FirstChild = dependent;
            ProjectMgr.OnItemsAppended(this);
            this.OnItemAdded(this, dependent);
            // Set parent and inherit the NonMember Status
            dependent.Parent = this;
            dependent.IsDependent = true;
            dependent.IsNonMemberItem = this.IsNonMemberItem;
            return true;
        }

        #endregion
        #region ItemTypes


        internal void SetSpecialPropertiesEx()
        {
            SetSpecialProperties();
        }
        protected override void SetSpecialProperties()
        {
            var type = this.FileType;
            var msBuildItem = this.ItemNode?.Item;
            if (msBuildItem != null)
            {
                switch (type)
                {
                    case XFileType.ManagedResource:
                        if (this.Parent != null && !this.Parent.HasDesigner)
                        {
                            if (!msBuildItem.HasMetadata(ProjectFileConstants.Generator))
                            {
                                this.SubType = ProjectFileAttributeValue.Designer;
                                this.Generator = "ResXFileCodeGenerator";
                            }
                        }
                        else
                        {
                            if (msBuildItem.HasMetadata(ProjectFileConstants.Generator))
                            {
                                this.SubType = null;
                                this.Generator = null;
                            }
                        }
                        break;
                    case XFileType.Settings:
                        if (!msBuildItem.HasMetadata(ProjectFileConstants.Generator))
                        {
                            this.Generator = "SettingsSingleFileGenerator";
                        }
                        break;
                    case XFileType.TextTemplate:
                        if (!msBuildItem.HasMetadata(ProjectFileConstants.Generator))
                        {
                            this.Generator = "TextTemplatingFileGenerator";
                        }
                        break;
                    default:
                        var xproj = ProjectMgr as XSharpProjectNode;
                        if (!xproj.IsLoading)
                        {
                            DetermineSubType();
                        }
                        break;
                }
            }

        }

        private bool HasSubType(string value)
        {
            string result = SubType;
            return !String.IsNullOrEmpty(result) && String.Equals(result, value, StringComparison.OrdinalIgnoreCase);

        }
        public bool IsXAML
        {
            get
            {
                return this.FileType == XFileType.XAML;
            }
        }
        public bool IsVOBinary
        {
            get
            {
                return this.FileType.IsVOBinary();
            }
        }

        public bool IsForm
        {
            get
            {
                return HasSubType(ProjectFileAttributeValue.Form);
            }
        }
        public bool IsUserControl
        {
            get
            {
                return HasSubType(ProjectFileAttributeValue.UserControl);
            }
        }
        public override int SortPriority
        {
            get
            {
                if (FileType.IsVOBinary())
                    return DefaultSortOrderNode.VOBinaryNode + (int)FileType;
                return DefaultSortOrderNode.HierarchyNode;
            }
        }
        private XFileType _fileType = XFileType.Unknown;

        private void DetermineFileType()
        {
            _fileType = XFileTypeHelpers.GetFileType(this.Url);
            if (_fileType == XFileType.SourceCode)
            {
                if (!string.Equals(ItemType, "Compile", StringComparison.OrdinalIgnoreCase))
                {
                    _fileType = XFileType.Other;
                }
            }

        }
        internal XFileType FileType
        {
            get
            {
                if (_fileType == XFileType.Unknown)
                {
                    DetermineFileType();
                }
                return _fileType;
            }
        }

        private void CheckItemType()
        {
            string itemType = this.ItemNode.ItemName;
            switch (this.FileType)
            {
                case XFileType.XAML:
                    // do not change the type when not needed
                    if (String.Equals(itemType, ProjectFileConstants.Page, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else if (String.Equals(itemType, ProjectFileConstants.ApplicationDefinition, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    else if (String.Equals(itemType, ProjectFileConstants.Resource, StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                    this.ItemNode.ItemName = ProjectFileConstants.Page;
                    break;
                case XFileType.SourceCode:
                    if (String.IsNullOrEmpty(itemType))
                    {
                        this.ItemNode.ItemName = SR.Compile;
                    }
                    break;
                case XFileType.ManagedResource:
                case XFileType.License:
                    if (!String.Equals(itemType, ProjectFileConstants.EmbeddedResource, StringComparison.OrdinalIgnoreCase))
                    {
                        this.ItemNode.ItemName = ProjectFileConstants.EmbeddedResource;
                    }
                    break;


                default:
                    break;
            }
        }
        /// <summary>
        /// Returns the SubType of an XSharp FileNode. It is
        /// </summary>
        public string SubType
        {
            get
            {
                return ItemNode.GetMetadata(ProjectFileConstants.SubType);
            }
            set
            {
                try
                {
                    ItemNode.SetMetadata(ProjectFileConstants.SubType, value);
                    // Don't forget to update...
                    UpdateHasDesigner();
                }
                catch (Exception)
                {
                    // This sometimes failes and causes an exception in VS.
                }
            }
        }
        public string Generator
        {
            get
            {
                return ItemNode.GetMetadata(ProjectFileConstants.Generator);
            }
            set
            {
                ItemNode.SetMetadata(ProjectFileConstants.Generator, value);
            }
        }


        #endregion

        #region Code Generation and Code Parsing
        /// <summary>
        /// factory method for creating single file generators.
        /// </summary>
        /// <returns></returns>
        protected override ISingleFileGenerator CreateSingleFileGenerator()
        {
            return new XSharpSingleFileGenerator(this.ProjectMgr);
        }

        private VSXSharpCodeDomProvider _codeDomProvider;
        protected internal VSXSharpCodeDomProvider CodeDomProvider
        {
            get
            {
                if (_codeDomProvider == null)
                {
                    _codeDomProvider = new VSXSharpCodeDomProvider(this);
                }
                return _codeDomProvider;
            }
        }




        #endregion

        #region Overriden implementation

        /// <summary>
        /// Creates an object derived from <see cref="NodeProperties"/> that will be used to expose
        /// properties specific for this object to the property browser.
        /// </summary>
        /// <returns>A new <see cref="NodeProperties"/> object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            if (IsNonMemberItem)
            {
                return new XFileNodeNonMemberProperties(this);
            }
            else if (!String.IsNullOrEmpty(this.ItemNode.GetMetadata("Link")))
            {
                return new XSharpLinkedFileNodeProperties(this);
            }
            //else if (this.IsVoBinary)
            //{

            //    return new XSharpVOBinaryFileNodeProperties(this);
            //}
            else
            {
                XSharpFileNodeProperties xprops = new XSharpFileNodeProperties(this);
                xprops.IsDependent = IsDependent;
                xprops.OnCustomToolChanged += new EventHandler<HierarchyNodeEventArgs>(OnCustomToolChanged);
                xprops.OnCustomToolNameSpaceChanged += new EventHandler<HierarchyNodeEventArgs>(OnCustomToolNameSpaceChanged);
                return xprops;
            }
        }



        /// <summary>
        /// Gets the automation object for the file node.
        /// </summary>
        /// <returns></returns>
        public override object GetAutomationObject()
        {
            if (automationObject == null)
            {
                automationObject = new OAXSharpFileItem(this.ProjectMgr.GetAutomationObject() as OAProject, this);
            }

            return automationObject;
        }
        protected override void Dispose(bool disposing)
        {
            if (this.ProjectMgr is XSharpProjectNode projectNode)
            {
                if (projectNode != null)
                    projectNode.RemoveURL(this);
            }
            base.Dispose(disposing);
        }

        public override DocumentManager GetDocumentManager()
        {
            return new XSharpFileDocumentManager(this);
        }

        /// <summary>
        /// Open a file depending on the SubType property associated with the file item in the project file
        /// </summary>
        protected override void DoDefaultAction()
        {
            var manager = (FileDocumentManager)this.GetDocumentManager();
            Debug.Assert(manager != null, "Could not get the FileDocumentManager");

            Guid viewGuid;
            string projectItemType = XSharpFileType.GetItemType(this.FileName);
            if (HasDesigner)
            {
                viewGuid = VSConstants.LOGVIEWID.Designer_guid;
            }
            else if (projectItemType == ProjectFileConstants.Compile)
            {
                viewGuid = VSConstants.LOGVIEWID.Code_guid;
            }
            else if (projectItemType == ProjectFileConstants.NativeResource)
            {
                viewGuid = VSConstants.LOGVIEWID.Code_guid;
            }
            else
            {
                viewGuid = VSConstants.LOGVIEWID.Primary_guid;
            }


            IVsWindowFrame frame;
            manager.Open(false, false, viewGuid, out frame, WindowFrameShowAction.Show);
        }

        public override void OnItemAdded(HierarchyNode parent, HierarchyNode child)
        {
            base.OnItemAdded(parent, child);
            if (child is XSharpFileNode xfile)
            {
                xfile.SetSpecialPropertiesEx();
            }
        }

        /// <summary>
        /// Handles command status on a node. Should be overridden by descendant nodes. If a command cannot be handled then the base should be called.
        /// </summary>
        /// <param name="guidCmdGroup">A unique identifier of the command group. The pguidCmdGroup parameter can be NULL to specify the standard group.</param>
        /// <param name="cmd">The command to query status for.</param>
        /// <param name="pCmdText">Pointer to an OLECMDTEXT structure in which to return the name and/or status information of a single command. Can be NULL to indicate that the caller does not require this information.</param>
        /// <param name="result">An out parameter specifying the QueryStatusResult of the command.</param>
        /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
        protected override int QueryStatusOnNode(Guid guidCmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            CurrentItem = this;
            if (guidCmdGroup == Microsoft.VisualStudio.Shell.VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    // we shouldn't get these on a file node...
                    //case VsCommands.AddNewItem:
                    //case VsCommands.AddExistingItem:

                    case VsCommands.ViewCode:
                        if (this.IsNonMemberItem || this.IsVOBinary)
                        {
                            result = QueryStatusResult.NOTSUPPORTED;
                            return (int)OleConstants.MSOCMDERR_E_NOTSUPPORTED;
                        }
                        else
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                            return VSConstants.S_OK;
                        }

                    case VsCommands.ViewForm:
                        if (HasDesigner)
                        {
                            result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        }
                        return VSConstants.S_OK;
                }
            }

            int returnCode;
            if (XHelperMethods.QueryStatusOnProjectSourceNode(this, guidCmdGroup, cmd, ref result, out returnCode))
            {
                return returnCode;
            }

            return base.QueryStatusOnNode(guidCmdGroup, cmd, pCmdText, ref result);
        }
        internal static XSharpFileNode CurrentItem = null;

        public override void Remove(bool removeFromStorage)
        {
            // Remove here because later the URL is gone
            var project = (XSharpProjectNode)this.ProjectMgr;
            var name = this.GetMkDocument();
            base.Remove(removeFromStorage);
            project.ProjectModel.RemoveFile(name);
            project.RemoveURL(name);
        }
        protected override bool RenameDocument(string oldName, string newName, out HierarchyNode newNodeOut)
        {

            var result = base.RenameDocument(oldName, newName, out newNodeOut);
            if (result)
            {
                if (ProjectMgr is XSharpProjectNode project)
                {
                    project.ProjectModel.RemoveFile(oldName);
                    project.ProjectModel.AddFile(newName);
                    project.ProjectModel.Walk();
                }
                _fileType = XFileTypeHelpers.GetFileType(newName);
            }
            return result;
        }

        #endregion

        #region Private implementation
        internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
        {
            get { return new OleServiceProvider.ServiceCreatorCallback(this.CreateServices); }
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(EnvDTE.ProjectItem) == serviceType)
            {
                service = GetAutomationObject();
            }
            return service;
        }

        #endregion

        #region Operate on Open Files
        private IVsTextLines TextLines
        {
            get
            {
                IVsHierarchy hierarchy;
                uint vsitemid = VSConstants.VSITEMID_NIL;
                IVsPersistDocData docData;
                uint docCookie;
                VsShellUtilities.GetRDTDocumentInfo(this.ProjectMgr.Site, this.Url, out hierarchy, out vsitemid, out docData, out docCookie);
                if (hierarchy == null || docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL)
                    return null;
                IVsTextLines buffer = docData as IVsTextLines;
                return buffer;

            }
        }

        #endregion
    }
}
