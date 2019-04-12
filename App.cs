#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

#endregion

namespace Revit.Logger
{
  class App : IExternalApplication
  {

    public Result OnStartup(UIControlledApplication a)
    {
      try {

        a.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        a.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);
        a.ControlledApplication.FailuresProcessing += (OnFailureProcessings);
      }
      catch (Exception e) {
        Debug.WriteLine(e.Message);
        return Result.Failed;
      }
      return Result.Succeeded;
    }


    public Result OnShutdown(UIControlledApplication a)
    {
      a.ControlledApplication.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(
        OnDocumentChanged);
      a.ViewActivated += new EventHandler<ViewActivatedEventArgs>(OnViewActivated);

      return Result.Succeeded;
    }

    void OnFailureProcessings(object sender, FailuresProcessingEventArgs args)
    {
      FailuresAccessor f = args.GetFailuresAccessor();

      string transName = f.GetTransactionName();

      IList<FailureMessageAccessor> fmas = f.GetFailureMessages();

      if (transName.Equals("Change shared coordinates in source")) {
        Debug.Write("lolkek");
      }

      if (fmas.Count == 0) {
        args.SetProcessingResult(FailureProcessingResult.Continue);
      }

    }

    public static class Globals
    {
      public static string ActiveDocumentTitle;
    }

    void OnViewActivated(object sender, ViewActivatedEventArgs args)
    {
      View vCurrent = args.CurrentActiveView;
      Document currentActiveDoc = vCurrent.Document;

      // ���������� ����� ��������� ��������� ��� ������������� � 
      // ��������� ��������� �������� !�������� �����!
      Globals.ActiveDocumentTitle = currentActiveDoc.Title;
    }

    void OnDocumentChanged(object sender, DocumentChangedEventArgs args)
    {
      var elementDoc = args.GetDocument();

      var modifiedElementsId = args.GetModifiedElementIds();

      // ����������� ������ (������� ��������� ���������)
      ProjectLocation projectLocation = null;
      SiteLocation siteLocation = null;
      RevitLinkInstance rvtPositionProvider = null;
      ImportInstance dwgPositionProvider = null;

      foreach (var elementId in modifiedElementsId) {
        var element = elementDoc.GetElement(elementId);

        // ���������� ��������, �� ����������� � ������� �����
        if (element.Document.Title != Globals.ActiveDocumentTitle)
          continue;
        // ��������� ��������� ���������� �������� �� �������������� � �������� ��������� ����� ���������
        switch (element) {
          case ProjectLocation loc:
            projectLocation = loc.Document.ActiveProjectLocation;
            break;
          case RevitLinkInstance link:
            rvtPositionProvider = link;
            break;
          case ImportInstance link:
            dwgPositionProvider = link;
            break;
          case SiteLocation site:
            siteLocation = site;
            break;
        }
      }

      // ��������� �������, �� ����������� � ��������� ��������
      if (projectLocation == null) return;

      // ����� �������� ��������� �� �����
      if (siteLocation != null &&
          (rvtPositionProvider != null || dwgPositionProvider != null)) {

        // �� ����� ������� �������, �.�. ���������� � � ������ � dwg - �� ������������� ��� �������� ����������
        var positionProviderName = dwgPositionProvider != null ? dwgPositionProvider.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString() : rvtPositionProvider.Name;

        // ���������� ����� �������� �����-����������
        string extractedSiteName = null;
        var providerSiteName = positionProviderName.Split(':');
        if (providerSiteName.Count() > 1)
        {
          var providerFileName = providerSiteName.Last().Trim();
          var spaceIdx = providerFileName.IndexOf(" ", StringComparison.Ordinal) + 1;

          extractedSiteName = providerFileName.Substring(spaceIdx);
        }

        var logging = $"ProviderFileName: {positionProviderName}" +
                      $"SiteName: {projectLocation.Name}" +
                      $"ParentFile: {Globals.ActiveDocumentTitle}" +
                      $"ProviderSiteName: {extractedSiteName}";
      }

      // ����� ��������� ��������� � �������
      if (siteLocation == null) {
        var logging = $"ProviderFileName: {Globals.ActiveDocumentTitle}" +
                      $"SiteName: {projectLocation.Name}" +
                      $"ParentFile: {Globals.ActiveDocumentTitle}";
      }
    }
  }
}
