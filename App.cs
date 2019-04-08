#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class SiteWarning : IFailuresPreprocessor
    {
      public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
      {
        IList<FailureMessageAccessor> failures = failuresAccessor.GetFailureMessages();
        foreach (var f in failures)
        {
          var id = f.GetFailureDefinitionId();
        }

        return FailureProcessingResult.Continue;
      }
    }
    public Result OnStartup(UIControlledApplication a)
    {
      try
      {
        a.ControlledApplication.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        a.DialogBoxShowing += new EventHandler<Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs>(AppDialogShowing);
        a.ControlledApplication.FailuresProcessing += (OnFailureProcessings);


      }
      catch (Exception e)
      {
        Debug.WriteLine(e.Message);
        return Result.Failed;
      }

      return Result.Succeeded;
    }


    public Result OnShutdown(UIControlledApplication a)
    {
      a.ControlledApplication.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(
        OnDocumentChanged );
      return Result.Succeeded;
    }

    void OnFailureProcessings(object sender, FailuresProcessingEventArgs args)
    {
      FailuresAccessor f = args.GetFailuresAccessor();

      string transName = f.GetTransactionName();

      IList<FailureMessageAccessor> fmas = f.GetFailureMessages();

      if (transName.Equals("Change shared coordinates in source"))
      {
        Debug.Write("lolkek");
      }

      if (fmas.Count == 0)
      {
        args.SetProcessingResult(FailureProcessingResult.Continue);
      }

    }
    void OnDysplayOptinsDialog(object sender, DisplayingOptionsDialogEventArgs e)
    {
      var c = e.PagesCount;
    }
    void AppDialogShowing(object sender, DialogBoxShowingEventArgs args)
    {
      if (args is TaskDialogShowingEventArgs e)
      {
        var type = e.DialogId;
        var msg = e.Message;

      }

    }
    void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
    {
      var list = new List<String>();
      var op = e.Operation;
      var kek = e.GetModifiedElementIds();
      var doc = e.GetDocument();
      var docname = doc.Title;
      foreach (var element in kek)
      {
        var el = doc.GetElement(element);

        list.Add(el.Name);



      }
    }
  }
}
