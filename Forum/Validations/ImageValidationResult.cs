using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Validations
{
    public class ImageValidationResult : ValidationResult
    {
      public ImageValidationResult DefaultError() {
        Errors.Add("Invalid Image file.");
        return this;
      }
  }
}
