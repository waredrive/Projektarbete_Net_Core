using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Forum.Validations;
using Microsoft.AspNetCore.Http;

namespace Forum.Extensions {
  public static class FormFileExtension {
    private const int ImageMaxBytes = 100000;
    private const int ImageMinBytes = 3000;
    private const int ImageMinPixelWidth = 240;
    private const int ImageMinPixelHeight = 240;
    private const int ImageMaxPixelWidth = 480;
    private const int ImageMaxPixelHeight = 480;

    public static ImageCustomValidationResult IsValidImage(this IFormFile postedFile) {
      var result = new ImageCustomValidationResult();

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
        if (!postedFile.OpenReadStream().CanRead)
          return result.DefaultError();

        //check if the image size exceeds the limits
        if (postedFile.Length > ImageMaxBytes) {
          result.Errors.Add($"The image must have less than {ImageMaxBytes / 1000}kB.");
          return result;
        }

        if (postedFile.Length < ImageMinBytes) {
          result.Errors.Add($"The image must have more than {ImageMinBytes / 1000}kB.");
          return result;
        }

        //check for html-injection
        var buffer = new byte[ImageMaxBytes];
        postedFile.OpenReadStream().Read(buffer, 0, ImageMaxBytes);
        var content = Encoding.UTF8.GetString(buffer);
        if (Regex.IsMatch(content,
          @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
          return result.DefaultError();
      }
      catch (Exception) {
        return result.DefaultError();
      }
      finally {
        postedFile.OpenReadStream().Dispose();
      }

      //  Try to instantiate new Bitmap, if .NET throws exception, the image is invalid.
      // Check if dimensions are within limits.
      try {
        using (var bitmap = new Bitmap(postedFile.OpenReadStream())) {
          if (bitmap.Width > ImageMaxPixelWidth)
            result.Errors.Add($"The image must have less than {ImageMaxPixelWidth}px width.");
          if (bitmap.Height > ImageMaxPixelHeight)
            result.Errors.Add($"The image must have less than {ImageMaxPixelHeight}px height.");
          if (bitmap.Width < ImageMinPixelWidth)
            result.Errors.Add($"The image must have more than {ImageMinPixelWidth}px width.");
          if (bitmap.Height < ImageMinPixelHeight)
            result.Errors.Add($"The image must have more than {ImageMinPixelHeight}px height.");
          if (bitmap.Width != bitmap.Height)
            result.Errors.Add("The image must be square.");
          return result;
        }
      }
      catch (Exception) {
        return result.DefaultError();
      }
      finally {
        postedFile.OpenReadStream().Dispose();
      }
    }
  }
}