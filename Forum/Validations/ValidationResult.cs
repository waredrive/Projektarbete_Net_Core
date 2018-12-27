using System.Collections.Generic;

namespace Forum.Validations {
  public class ValidationResult {
    public bool Success => Errors.Count == 0;
    public List<string> Errors { get; } = new List<string>();
  }
}