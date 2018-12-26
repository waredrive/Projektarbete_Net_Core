using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Forum.Validations;
using Microsoft.AspNetCore.Http;

namespace Forum.Extensions {
  public static class FormFileExtension {
    public const int ImageMaxBytes = 100000;
    public const int ImageMinBytes = 1000;
    public const int ImagePixelWidth = 240;
    public const int ImagePixelHeight = 240;

    public static ImageValidationResult IsValidImage(this IFormFile postedFile) {

      var result = new ImageValidationResult();

      if (!string.Equals(postedFile.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(postedFile.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(postedFile.ContentType, "image/pjpeg", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(postedFile.ContentType, "image/gif", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(postedFile.ContentType, "image/x-png", StringComparison.OrdinalIgnoreCase) &&
          !string.Equals(postedFile.ContentType, "image/png", StringComparison.OrdinalIgnoreCase)) {
        result.Errors.Add("The image must be of an jpg, gif or png format.");
        return result;
      }

      var postedFileExtension = Path.GetExtension(postedFile.FileName);
      if (!string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
          && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
          && !string.Equals(postedFileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
          && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase)) {
        result.Errors.Add("The image must be of an jpg, gif or png format.");
        return result;
      }

      //  Attempt to read the file and check the first bytes
      try {
        if (!postedFile.OpenReadStream().CanRead) {
          return result.DefaultError();
        }

        //check if the image size exceeds the limits
        if (postedFile.Length > ImageMaxBytes) {
          result.Errors.Add($"The image must have less than {ImageMaxBytes/1000}Kb.");
          return result;
        }
        if (postedFile.Length < ImageMinBytes) {
          result.Errors.Add($"The image must have more than {ImageMinBytes/1000}Kb.");
          return result;
        }

        //check for html-injection
        var buffer = new byte[ImageMaxBytes];
        postedFile.OpenReadStream().Read(buffer, 0, ImageMaxBytes);
        var content = System.Text.Encoding.UTF8.GetString(buffer);
        if (Regex.IsMatch(content,
          @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline)) {
          return result.DefaultError();
        }
      } catch (Exception) {
        return result.DefaultError();
      } finally {
        postedFile.OpenReadStream().Dispose();
      }

      //  Try to instantiate new Bitmap, if .NET throws exception, the image is invalid
      // Check if sizing is within limits
      try {
        using (var bitmap = new Bitmap(postedFile.OpenReadStream())) {
          if (bitmap.Width != ImagePixelWidth) {
            result.Errors.Add($"The image must have {ImagePixelWidth}px width.");
          }
          if (bitmap.Height != ImagePixelHeight) {
            result.Errors.Add($"The image must have {ImagePixelHeight}px height.");
          }
          return result;
        }
      } catch (Exception) {
        return result.DefaultError();
      } finally {
        postedFile.OpenReadStream().Dispose();
      }
    }
  }
}
