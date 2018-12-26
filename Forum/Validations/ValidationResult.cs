using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Validations
{
    public class ValidationResult
    {
      public bool Success => Errors.Count == 0;
      public List<string> Errors { get; } = new List<string>();
  }
}
