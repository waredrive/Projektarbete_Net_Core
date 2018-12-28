namespace Forum.Validations {
  public class ImageCustomValidationResult : CustomValidationResult {
    public ImageCustomValidationResult DefaultError() {
      Errors.Add("Invalid Image file.");
      return this;
    }
  }
}