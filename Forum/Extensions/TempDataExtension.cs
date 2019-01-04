using System;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Forum.Extensions {
  public static class TempDataExtension {

    private const string ModalHeaderKey = "ModalHeader";
    private const string ModalMessageKey = "ModalMessage";

    public static string GetModalHeader(this ITempDataDictionary tempData) {
      return (string)tempData[ModalHeaderKey];
    }

    public static string GetModalMessage(this ITempDataDictionary tempData) {
      return (string)tempData[ModalMessageKey];
    }

    private static void SetModalData(this ITempDataDictionary tempData, string header, string message) {
      tempData[ModalHeaderKey] = header;
      tempData[ModalMessageKey] = message;
    }

    public static bool ModalClear(this ITempDataDictionary tempData) {
      tempData.Clear();
      return true;
    }

    public static void ModalSuccess(this ITempDataDictionary tempData, string message) {
      tempData.SetModalData("Success!", message);
    }

    public static void ModalWarning(this ITempDataDictionary tempData, string message) {
      tempData.SetModalData("Warning!", message);
    }

    public static void ModalFailed(this ITempDataDictionary tempData, string message) {
      tempData.SetModalData("Failed!", message);
    }

    public static void ModalNoPermission(this ITempDataDictionary tempData) {
      tempData.SetModalData("Failed!", "You have no permission for this operation!");
    }
  }
}
