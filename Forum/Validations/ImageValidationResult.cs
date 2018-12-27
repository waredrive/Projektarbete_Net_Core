namespace Forum.Validations {
  public class ImageValidationResult : ValidationResult {
    public ImageValidationResult DefaultError() {
      Errors.Add("Invalid Image file.");
      return this;
    }
  }
}